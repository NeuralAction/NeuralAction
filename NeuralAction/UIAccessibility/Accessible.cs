// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Accessibility;
using System.Reflection;

namespace UIAccessibility
{
    public struct Point
    {
        public double X;
        public double Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal Point(NativeMethods.POINT pt)
        {
            X = pt.x;
            Y = pt.y;
        }
    }

    public enum UIElementType
    {
        ScrollViewer,
        Chrome,
        TextBox,
        Button,
        TitleBar,
        None,
    }

    public class VariantNotIntException : Exception
    {
        public string VariantType { get; private set; }

        public VariantNotIntException(object variant)
        {
            VariantType = variant == null ? "[NULL]" : variant.GetType().ToString();
        }
    }

    public class ChildCountInvalidException : Exception
    {
        public int ChildCount { get; private set; }

        public ChildCountInvalidException(int childCount)
        {
            ChildCount = childCount;
        }
    }

    public class UiHitResult
    {
        public Accessible RawHit { get; set; }
        public Accessible UiElement { get; set; }
    }

    public static class UiHitTest
    {
        public static Accessible GetHit(Point pt)
        {
            return Accessible.FromPoint(pt);
        }

        public static UiHitResult GetUiHit(Point pt)
        {
            var hit = GetHit(pt);
            var element = GetUiElement(hit, pt);

            var ret = new UiHitResult()
            {
                RawHit = hit,
                UiElement = element,
            };
            return ret;
        }

        static Accessible GetUiElement(Accessible acc, Point pt)
        {
            if (acc == null)
                return null;

            var type = acc.Type;
            var loc = acc.Location;

            if (type != UIElementType.None && loc.Contains((int)pt.X, (int)pt.Y))
                return acc;
            else
            {
                Accessible parent = acc.Parent;

                if (parent != null)
                    return GetUiElement(parent, pt);
                else
                    return null;
            }
        }
    }
    
    public class Accessible
    {
        static bool CheckParentClassIs(Accessible acc, string key)
        {
            var name = acc.ClassName;

            if (name == key)
            {
                return true;
            }
            else
            {
                var parent = acc.Parent;
                if (parent != null)
                    return CheckParentClassIs(parent, key);
                else
                    return false;
            }
        }

        public static UIElementType GetTypeFromRole(Accessible acc)
        {
            var role = acc.Role;
            var name = acc.Name;
            var state = acc.State;
            var className = acc.ClassName;

            switch (acc.Role)
            {
                case 0x01: //제목 표시줄
                    return UIElementType.TitleBar;
                case 0x3: // 스크롤 막대
                case 0x33: //슬라이더
                case 33: // 목록
                    if (className == "SysListView32" && CheckParentClassIs(acc, "WorkerW"))
                        return UIElementType.None;
                    return UIElementType.ScrollViewer;
                case 0x9: //창
                    if (name == "Chrome Legacy Window" || className.StartsWith("Chrome_"))
                        return UIElementType.Chrome;
                    else if (className == "SysTreeView32")
                        return UIElementType.ScrollViewer;
                    return UIElementType.None;
                case 0x24: //윤곽항목
                    if (className == "SysTreeView32")
                        return UIElementType.Button;
                    return UIElementType.None;
                case 42: //편집가능한 텍스트
                    if ((state & 0x40) != 0x40 //사용불가
                        && state != 0x0 //보통
                        )
                        return UIElementType.TextBox;
                    return UIElementType.None;
                case 0x2C: //확인란
                case 0x19: //열머리글
                case 0x2E: //콤보박스
                case 0x2D: //라디오 단추
                case 34: //목록 항목
                case 58: //그리드 드랍다운 단추
                case 0x38: //드랍다운 단추
                case 37: //페이지탭
                case 0x3E: //분활단추
                case 0xC: //메뉴항목
                case 0x39: //메뉴단추
                case 0x2B: //누름단추
                case 0x1E: //링크
                    return UIElementType.Button;
                default:
                    return UIElementType.None;
            }
        }

        public static int FromPoint(Point pt, out Accessible accessible)
        {
            accessible = null;
            IAccessible acc = null;
            object childId = null;
            int hr = NativeMethods.AccessibleObjectFromPoint(new NativeMethods.POINT(pt), ref acc, ref childId);

            if (hr == NativeMethods.S_OK && acc != null)
            {
                if (childId == null)
                    childId = 0;
                if (!(childId is int))
                    throw new VariantNotIntException(childId);

                accessible = new Accessible(acc, (int)childId);
            }

            return hr;
        }

        public static Accessible FromPoint(Point pt)
        {
            FromPoint(pt, out Accessible accessible);

            return accessible;
        }

        public static int FromWindow(IntPtr hwnd, out Accessible accessible)
        {
            accessible = null;
            Guid guid = NativeMethods.IID_IAccessible;

            object obj = null;
            int hr = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.ObjIdWindow, ref guid, ref obj);

            if (obj is IAccessible acc)
                accessible = new Accessible(acc, 0);

            return hr;
        }

        public static Accessible FromWindow(IntPtr hwnd)
        {
            FromWindow(hwnd, out Accessible accessible);

            return accessible;
        }

        public static int FromEvent(IntPtr hwnd, int idObject, int idChild, out Accessible accessible)
        {
            accessible = null;
            object childId = null;
            IAccessible acc = null;
            int hr = NativeMethods.AccessibleObjectFromEvent(hwnd, idObject, idChild, ref acc, ref childId);

            if (acc != null)
                accessible = new Accessible(acc, (int)childId);

            return hr;
        }

        public IAccessible IAccessible { get; private set; }
        public int ChildId { get; private set; }

        public Accessible Parent { get; private set; }
        public IntPtr Hwnd { get; private set; }
        public Rectangle Location { get; private set; }
        public UIElementType Type => GetTypeFromRole(this);
        public string Name { get; private set; }
        public string ClassName { get; private set; }
        public string RoleText { get; private set; }
        public string KeyboardShortcut { get; private set; }
        public string Value { get; private set; }
        public int Role { get; private set; }
        public int State { get; private set; }

        public Accessible(IAccessible accessible, int childId)
        {
            IAccessible = accessible;
            ChildId = childId;

            Update();
        }

        public void UpdateName()
        {
            Name = IAccessible.get_accName(ChildId);
        }

        public void UpdateRole()
        {
            object role = IAccessible.get_accRole(ChildId);
            if (!(role is int))
                throw new VariantNotIntException(role);

            Role = (int)role;
        }

        public void UpdateRoleText()
        {
            UpdateRole();

            StringBuilder roleText = new StringBuilder(255);
            NativeMethods.GetRoleText(Role, roleText, (uint)roleText.Capacity);

            RoleText = roleText.ToString();
        }

        public void UpdateState()
        {
            object state = IAccessible.get_accState(ChildId);
            if (!(state is int))
                throw new VariantNotIntException(state);

            State = (int)state;
        }

        public void UpdateValue()
        {
            string value = IAccessible.get_accValue(ChildId);
            if (value == null)
                value = "";

            Value = value;
        }

        public void UpdateLocation()
        {
            IAccessible.accLocation(out int left, out int top, out int width, out int height, ChildId);

            Location = new Rectangle(left, top, width, height);
        }

        public void UpdateKeyboardShortcut()
        {
            KeyboardShortcut = IAccessible.get_accKeyboardShortcut(ChildId);
        }

        public void UpdateHwnd()
        {
            IntPtr hwnd = IntPtr.Zero;
            NativeMethods.WindowFromAccessibleObject(IAccessible, ref hwnd);

            Hwnd = hwnd;
        }

        public void UpdateClassName()
        {
            UpdateHwnd();

            ClassName = NativeMethods.GetClassName(Hwnd);
        }

        public void UpdateParent()
        {
            object parent = null;

            if (ChildId == NativeMethods.CHILD_SELF)
            {
                IAccessible parentIAccessible = IAccessible.accParent as IAccessible;
                if (parentIAccessible == null)
                {
                    parent = null;
                }
                else
                {
                    parent = new Accessible(parentIAccessible, NativeMethods.CHILD_SELF);
                }
            }
            else
            {
                parent = new Accessible(IAccessible, NativeMethods.CHILD_SELF);
            }

            Parent = parent as Accessible;
        }

        public void Update()
        {
            try { UpdateClassName(); } catch { ClassName = ""; }

            try { UpdateHwnd(); } catch { Hwnd = IntPtr.Zero; }

            try { UpdateKeyboardShortcut(); } catch { KeyboardShortcut = ""; }

            try { UpdateLocation(); } catch { Location = Rectangle.Empty; }

            try { UpdateName(); } catch { Name = ""; }

            try { UpdateParent(); } catch { Parent = null; }

            try { UpdateRole(); } catch { Role = 0; }

            try { UpdateRoleText(); } catch { RoleText = ""; }

            try { UpdateState(); } catch { State = 0; }

            try { UpdateValue(); } catch { Value = ""; }
        }

        public int Children(out Accessible[] accessible)
        {
            accessible = null;
            if (ChildId != NativeMethods.CHILD_SELF)
            {
                accessible = new Accessible[] { };
                return NativeMethods.S_OK;
            }

            int childrenCount = IAccessible.accChildCount;
            if (childrenCount < 0)
            {
                throw new ChildCountInvalidException(childrenCount);
            }

            object[] children = new object[childrenCount];

            int hr = NativeMethods.AccessibleChildren(IAccessible, 0, childrenCount, children, out childrenCount);

            if (hr == NativeMethods.S_OK)
            {
                accessible = new Accessible[childrenCount];
                int i = 0;
                foreach (object child in children)
                {
                    if (child != null)
                    {
                        if (child is IAccessible)
                        {
                            accessible[i++] = new Accessible((IAccessible)child, NativeMethods.CHILD_SELF);
                        }
                        else if (child is int)
                        {
                            accessible[i++] = new Accessible(IAccessible, (int)child);
                        }
                    }
                }

                // null children don't occur very often but if they do it stops us from going on
                // So keep track of them so we can reallocate the array if necessary
                if (childrenCount != i)
                {
                    // if we had some null chilren create a smaller array to send the 
                    // children back in.
                    Accessible[] accessibleNew = new Accessible[i];
                    Array.Copy(accessible, accessibleNew, i);
                    accessible = accessibleNew;
                }
            }

            return hr;

        }

        public void Select(int flag)
        {
            IAccessible.accSelect(flag, ChildId);
        }

        public bool Compare(Accessible acc)
        {
            if (acc == null)
                return false;

            var loc = acc.Location;
            var thisLoc = Location;
            if (!thisLoc.Equals(loc))
                return false;

            var name = acc.Name;
            var thisName = Name;
            if (thisName != name)
                return false;

            var role = acc.Role;
            var thisRole = Role;
            if (thisRole != role)
                return false;

            var value = acc.Value;
            var thisValue = Value;
            if (thisValue != value)
                return false;

            return true;
        }

        public string GetParentTree()
        {
            StringBuilder stringBuilder = new StringBuilder();
            var buf = new List<Accessible>();
            GetParentTree(buf, this);
            for (int i = buf.Count - 1; i > -1; i--)
            {
                var item = buf[i];
                var indent = buf.Count - 1 - i;
                for (int ii = 0; ii < indent; ii++)
                {
                    if (ii == 0)
                        stringBuilder.Append("    ");
                    else
                        stringBuilder.Append(".   ");
                }
                stringBuilder.AppendLine(item.ToString());
            }
            return stringBuilder.ToString();
        }

        void GetParentTree(List<Accessible> buf, Accessible acc)
        {
            buf.Add(acc);
            var parent = acc.Parent;
            if (parent != null)
                GetParentTree(buf, parent);
        }

        override public string ToString()
        {
            return $"[ChildId({ChildId}) Role({RoleText}[0x{Role:X}]) Name({Name}) State(0x{State:X}) Class({ClassName}) HWND(0x{Hwnd:X})]";
        }
    }
}

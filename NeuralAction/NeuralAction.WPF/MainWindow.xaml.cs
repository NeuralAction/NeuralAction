using Microsoft.Expression.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace NeuralAction.WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        CursorIcon CursorIcon = new CursorIcon();

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public static IntPtr FocusedHandle { get; set; }

        void NotWindowsFocus() {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
            GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
            CursorIcon.Show();
        }

        public static bool RestoreClipboard { get; set; } = true;

        string[] koreainputchar = new string[3];
        string CurrentLanguage = "kr";
        int inputcount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotWindowsFocus();
            KeymapChange(GetKeymapArray(CurrentLanguage));
        }

        private static string ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        private static string JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        private static string JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        private static ushort UniCodeHangulBase = 0xAC00;

        public static string MergeJaso(string choSung, string jungSung, string jongSung) {
            int ChoSungPos, JungSungPos, JongSungPos;
            int UniCode;

            ChoSungPos = ChoSungTbl.IndexOf(choSung);
            JungSungPos = JungSungTbl.IndexOf(jungSung);
            JongSungPos = JongSungTbl.IndexOf(jongSung);

            UniCode = UniCodeHangulBase + (ChoSungPos * 21 + JungSungPos) * 28 + JongSungPos;

            char temp = Convert.ToChar(UniCode);

            return temp.ToString();
        }

        public void CenterTextMerge(string[] Keymap, int index) {

            if (inputcount == 1) {
                CenterText.Text = MergeJaso(koreainputchar[0], Keymap[index], "");
                koreainputchar[1] = Keymap[index];
            } else if (inputcount == 2) {
                CenterText.Text = MergeJaso(koreainputchar[0], koreainputchar[1], Keymap[index]);
            }

        }

        string[] EnglishKeymap = new string[28] { "a", "b", "c", "v", "d", "e", "f", "w", "g", "h", "i", "x", "j", "k", "l", "y", "m", "n", "o", "z", "p", "q", "r", "", "s", "t", "u", "" };
        string[] KoreanChosungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "", "ㄷ", "ㅌ", "ㄸ", "", "ㅈ", "ㅊ", "ㅉ", "", "ㅂ", "ㅍ", "ㅃ", "", "ㄴ", "ㅁ", "ㄹ", "", "ㅅ", "ㅆ", "", "", "ㅇ", "ㅎ", "", "" };
        string[] KoreanJungsungKeymap = new string[28] { "ㅏ", "ㅑ", "ㅐ", "", "ㅓ", "ㅕ", "ㅔ", "", "ㅗ", "ㅛ", "ㅚ", "", "ㅜ", "ㅠ", "ㅟ", "", "ㅡ", "ㅘ", "ㅙ", "", "ㅣ", "ㅒ", "ㅖ", "", "ㅢ", "ㅝ", "ㅞ", "" };
        string[] KoreanJongsungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "ㄺ", "ㄷ", "ㅌ", "ㄸ", "ㄼ", "ㅈ", "ㅊ", "ㅉ", "ㄶ", "ㅂ", "ㅍ", "ㅃ", "ㅄ", "ㄴ", "ㅁ", "ㄹ", "ㄻ", "ㅅ", "ㅆ", "ㅄ", "ㄳ", "ㅇ", "ㅎ", "ㄶ", "ㅀ" };
        string[] SpecialCharKeymap = new string[28] { ".", "5", "(", "", ",", "6", ")", "", "0", "7", "&", "", "1", "8", "+", "", "2", "9", "-", "", "3", "!", "*", "", "4", "?", "/", "" };

        public string[] GetKeymapArray(string CurrentLanguage)
        {

            if (CurrentLanguage == "kr") {
                if (inputcount == 0) {
                    return KoreanChosungKeymap;
                } else if (inputcount == 1) {
                    return KoreanJungsungKeymap;
                } else if (inputcount == 2) {
                    return KoreanJongsungKeymap;
                }
            } else if (CurrentLanguage == "en") {
                return EnglishKeymap;
            } else if (CurrentLanguage == "sp") {
                return SpecialCharKeymap;
            }
            return null;

        }

        public void KeymapChange(string[] keymap) {

            input0.Text = keymap[0];
            input1.Text = keymap[4];
            input2.Text = keymap[8];
            input3.Text = keymap[12];
            input4.Text = keymap[16];
            input5.Text = keymap[20];
            input6.Text = keymap[24];

        }

        public void InputingReset(bool onlytagreset = false) {

            if (onlytagreset == true) {

                pie0.Tag = 0;
                pie1.Tag = 4;
                pie2.Tag = 8;
                pie3.Tag = 12;
                pie4.Tag = 16;
                pie5.Tag = 20;
                pie6.Tag = 24;

                input0.Tag = 0;
                input1.Tag = 4;
                input2.Tag = 8;
                input3.Tag = 12;
                input4.Tag = 16;
                input5.Tag = 20;
                input6.Tag = 24;

            } else {

                koreainputchar[0] = "";
                koreainputchar[1] = "";
                koreainputchar[2] = "";
                CenterText.Text = "";
                inputcount = 0;

            }
        }

        private void KeyChange(object sender, System.Windows.Input.MouseEventArgs e) {

            int i = 0;
            string[] Keymap = GetKeymapArray(CurrentLanguage);

            if (sender.GetType().ToString() == "System.Windows.Controls.TextBlock") {
                i = Convert.ToInt32(((TextBlock)sender).Tag.ToString());
            }  else if (sender.GetType().ToString() == "Microsoft.Expression.Shapes.Arc") {
                i = Convert.ToInt32(((Arc)sender).Tag.ToString());
            }

            if (i < 4) {

                CenterText.Text = input0.Text;

                if (Keymap[i + 1] == "" || i == 3) {

                    if (CurrentLanguage == "kr"){
                        CenterTextMerge(Keymap, i);
                    }

                    pie0.Tag = 0;
                    input0.Tag = 0;
                    input0.Text = Keymap[0];

                } else {

                    pie0.Tag = (Convert.ToInt32(pie0.Tag.ToString()) + 1).ToString();
                    input0.Tag = (Convert.ToInt32(input0.Tag.ToString()) + 1).ToString();
                    input0.Text = Keymap[i + 1];

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                }

            } else if (i < 8) {

                CenterText.Text = input1.Text;

                if (Keymap[i + 1] == "" || i == 7) {

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                    pie1.Tag = 4;
                    input1.Tag = 4;
                    input1.Text = Keymap[4];

                } else {
                    pie1.Tag = (Convert.ToInt32(pie1.Tag.ToString()) + 1).ToString();
                    input1.Tag = (Convert.ToInt32(input1.Tag.ToString()) + 1).ToString();
                    input1.Text = Keymap[i + 1];

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }
                }

            } else if (i < 12) {

                CenterText.Text = input2.Text;

                if (Keymap[i + 1] == "" || i == 11)
                {

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                    pie2.Tag = 8;
                    input2.Tag = 8;
                    input2.Text = Keymap[8];

                } else  {

                    pie2.Tag = (Convert.ToInt32(pie2.Tag.ToString()) + 1).ToString();
                    input2.Tag = (Convert.ToInt32(input2.Tag.ToString()) + 1).ToString();
                    input2.Text = Keymap[i + 1];

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }
                }

            } else if (i < 16) {

                CenterText.Text = input3.Text;

                if (Keymap[i + 1] == "" || i == 15) {

                    if (CurrentLanguage == "kr")  {
                        CenterTextMerge(Keymap, i);
                    }

                    pie3.Tag = 12;
                    input3.Tag = 12;
                    input3.Text = Keymap[12];

                } else {
                    pie3.Tag = (Convert.ToInt32(pie3.Tag.ToString()) + 1).ToString();
                    input3.Tag = (Convert.ToInt32(input3.Tag.ToString()) + 1).ToString();
                    input3.Text = Keymap[i + 1];
                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }
                }

            } else if (i < 20) {

                CenterText.Text = input4.Text;

                if (Keymap[i + 1] == "" || i == 19)  {

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                    pie4.Tag = 16;
                    input4.Tag = 16;
                    input4.Text = Keymap[16];

                } else {
                    pie4.Tag = (Convert.ToInt32(pie4.Tag.ToString()) + 1).ToString();
                    input4.Tag = (Convert.ToInt32(input4.Tag.ToString()) + 1).ToString();
                    input4.Text = Keymap[i + 1];
                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }
                }

            } else if (i < 24)  {

                CenterText.Text = input5.Text;

                if (Keymap[i + 1] == "" || i == 23) {

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                    pie5.Tag = 20;
                    input5.Tag = 20;
                    input5.Text = Keymap[20];

                } else {

                    pie5.Tag = (Convert.ToInt32(pie5.Tag.ToString()) + 1).ToString();
                    input5.Tag = (Convert.ToInt32(input5.Tag.ToString()) + 1).ToString();
                    input5.Text = Keymap[i + 1];


                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }
                }

            } else if (i < 28) {

                CenterText.Text = input6.Text;

                if (i == 27 || Keymap[i + 1] == "")
                {

                    if (CurrentLanguage == "kr") {
                        CenterTextMerge(Keymap, i);
                    }

                    pie6.Tag = 24;
                    input6.Tag = 24;
                    input6.Text = Keymap[24];

                } else {

                    pie6.Tag = (Convert.ToInt32(pie6.Tag.ToString()) + 1).ToString();
                    input6.Tag = (Convert.ToInt32(input6.Tag.ToString()) + 1).ToString();
                    input6.Text = Keymap[i + 1];

                    if (CurrentLanguage == "kr")  {
                        CenterTextMerge(Keymap, i);
                    }
                }
            }
        }

        private void InputingSentence(object sender, System.Windows.Input.MouseEventArgs e)  {

            InputingReset();

            string RealSendKey = ((TextBlock)sender).Tag.ToString();
            CenterText.Text = RealSendKey;

            System.Windows.Forms.Clipboard.SetText(RealSendKey);
            Send sendkeys = new Send(RealSendKey, RealSendKey);
            sendkeys.Work();

        }

        private void InputingChar(object sender, System.Windows.Input.MouseEventArgs e)  {

            if(CenterText.Text != "") { 
            System.Windows.Forms.Clipboard.SetText(CenterText.Text);
            Send sendkeys = new Send(CenterText.Text, CenterText.Text);

            if (CenterText.Text == "←") {

                System.Windows.Forms.Clipboard.SetText("{BACK}");
                sendkeys = new Send("{BACK}", "{BACK}");

            } else  if (CurrentLanguage == "kr") {

                if (inputcount == 0) {

                    inputcount++;
                    KeymapChange(KoreanJungsungKeymap);
                    koreainputchar[0] = CenterText.Text;
                    InputingReset(true);

                }  else if (inputcount == 1) {

                    inputcount++;
                    KeymapChange(KoreanJongsungKeymap);
                    InputingReset(true);

                }  else if (inputcount == 2) {

                    inputcount = 0;
                    KeymapChange(KoreanChosungKeymap);
                    sendkeys = new Send(CenterText.Text, CenterText.Text);
                    sendkeys.Work();
                    InputingReset();

                }

            } else {

                sendkeys.Work();

            }
            }
        }

        private void BackSpace(object sender, System.Windows.Input.MouseEventArgs e)  {

            System.Windows.Forms.Clipboard.SetText("{BACK}");
            Send sendkeys = new Send("{BACK}", "{BACK}");
            sendkeys.Work();

            InputingReset();

            CenterText.Text = "←";

        }

        private void InputingSpace(object sender, System.Windows.Input.MouseEventArgs e) {

            System.Windows.Forms.Clipboard.SetText(" ");
            Send sendkeys = new Send(" ", " ");
            sendkeys.Work();

            InputingReset();

        }

        private void KeypadRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            NotWindowsFocus();
        }

        private void Ellipse_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            this.Topmost = false;
            CursorIcon.Topmost = true;
        }

        private void Ellipse_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.Topmost = true;
            CursorIcon.Topmost = false;
        }

        private void ChangeLanguage(object sender, MouseButtonEventArgs e) {

            if (CurrentLanguage == "kr") {

                CurrentLanguage = "en";
                KeymapChange(GetKeymapArray(CurrentLanguage));
                BlankText.Text = "Spacing";
                LauguageChangeText.Text = "Special";

            } else if (CurrentLanguage == "en") {

                CurrentLanguage = "sp";
                KeymapChange(GetKeymapArray(CurrentLanguage));
                LauguageChangeText.Text = "한국어";

            } else if (CurrentLanguage == "sp") {

                CurrentLanguage = "kr";
                KeymapChange(GetKeymapArray(CurrentLanguage));
                BlankText.Text = "띄어쓰기";
                LauguageChangeText.Text = "English";

            }

            InputingReset();

        }
    }
}
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
    public enum Languages
    {
        Korean = 0,
        English = 1,
        Special = 2
    }

    public partial class MainWindow : Window
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr FocusedHandle { get; set; }
        public static bool RestoreClipboard { get; set; } = true;

        static string ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        static string JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        static string JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        static ushort UniCodeHangulBase = 0xAC00;

        static string[] EnglishKeymap = new string[28] { "a", "b", "c", "v", "d", "e", "f", "w", "g", "h", "i", "x", "j", "k", "l", "y", "m", "n", "o", "z", "p", "q", "r", "", "s", "t", "u", "" };
        static string[] KoreanChosungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "", "ㄷ", "ㅌ", "ㄸ", "", "ㅈ", "ㅊ", "ㅉ", "", "ㅂ", "ㅍ", "ㅃ", "", "ㄴ", "ㅁ", "ㄹ", "", "ㅅ", "ㅆ", "", "", "ㅇ", "ㅎ", "", "" };
        static string[] KoreanJungsungKeymap = new string[28] { "ㅏ", "ㅑ", "ㅐ", "", "ㅓ", "ㅕ", "ㅔ", "", "ㅗ", "ㅛ", "ㅚ", "", "ㅜ", "ㅠ", "ㅟ", "", "ㅡ", "ㅘ", "ㅙ", "", "ㅣ", "ㅒ", "ㅖ", "", "ㅢ", "ㅝ", "ㅞ", "" };
        static string[] KoreanJongsungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "ㄺ", "ㄷ", "ㅌ", "ㄸ", "ㄼ", "ㅈ", "ㅊ", "ㅉ", "ㄶ", "ㅂ", "ㅍ", "ㅃ", "ㅄ", "ㄴ", "ㅁ", "ㄹ", "ㄻ", "ㅅ", "ㅆ", "ㅄ", "ㄳ", "ㅇ", "ㅎ", "ㄶ", "ㅀ" };
        static string[] SpecialCharKeymap = new string[28] { ".", "5", "(", "", ",", "6", ")", "", "0", "7", "&", "", "1", "8", "+", "", "2", "9", "-", "", "3", "!", "*", "", "4", "?", "/", "" };

        public static string MergeJaso(string choSung, string jungSung, string jongSung)
        {
            var ChoSungPos = ChoSungTbl.IndexOf(choSung);
            var JungSungPos = JungSungTbl.IndexOf(jungSung);
            var JongSungPos = JongSungTbl.IndexOf(jongSung);

            var UniCode = UniCodeHangulBase + (ChoSungPos * 21 + JungSungPos) * 28 + JongSungPos;
            
            return $"{(char)UniCode}";
        }

        CursorService cursorServcie;
        Languages CurrentLanguage = Languages.Korean;
        string[] koreaInputChar = new string[3];
        int inputCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            // TODO: get screen / dpi
            cursorServcie = new CursorService();
        }

        #region UI Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotWindowsFocus();

            KeymapChange(GetKeymapArray(CurrentLanguage));

            cursorServcie.StartAsync(0);
        }

        void NotWindowsFocus()
        {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
            //CursorIcon.Show();
        }

        private void KeypadRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //NotWindowsFocus();
        }

        private void Ellipse_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Topmost = false;
            //CursorIcon.Topmost = true;
        }

        private void Ellipse_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Topmost = true;
            //CursorIcon.Topmost = false;
        }

        #endregion UI Events

        #region Keyboard

        public void CenterTextMerge(string[] Keymap, int index)
        {
            if (inputCount == 1)
            {
                CenterText.Text = MergeJaso(koreaInputChar[0], Keymap[index], "");
                koreaInputChar[1] = Keymap[index];
            }
            else if (inputCount == 2)
            {
                CenterText.Text = MergeJaso(koreaInputChar[0], koreaInputChar[1], Keymap[index]);
            }
        }

        public string[] GetKeymapArray(Languages lang)
        {
            switch (lang)
            {
                case Languages.Korean:
                    if (inputCount == 0)
                    {
                        return KoreanChosungKeymap;
                    }
                    else if (inputCount == 1)
                    {
                        return KoreanJungsungKeymap;
                    }
                    else if (inputCount == 2)
                    {
                        return KoreanJongsungKeymap;
                    }
                    else
                    {
                        throw new Exception("wrong input count");
                    }
                case Languages.English:
                    return EnglishKeymap;
                case Languages.Special:
                    return SpecialCharKeymap;
                default:
                    return null;
            }
        }

        public void KeymapChange(string[] keymap)
        {
            input0.Text = keymap[0];
            input1.Text = keymap[4];
            input2.Text = keymap[8];
            input3.Text = keymap[12];
            input4.Text = keymap[16];
            input5.Text = keymap[20];
            input6.Text = keymap[24];
        }

        public void InputingReset(bool onlyTagReset = false)
        {
            if (onlyTagReset == true)
            {
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
            }
            else
            {
                koreaInputChar[0] = "";
                koreaInputChar[1] = "";
                koreaInputChar[2] = "";
                CenterText.Text = "";
                inputCount = 0;
            }
        }

        private void KeyChange(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int i = 0;
            string[] keymap = GetKeymapArray(CurrentLanguage);

            if (sender is TextBlock)
            {
                i = Convert.ToInt32(((TextBlock)sender).Tag.ToString());
            }
            else if (sender is Arc)
            {
                i = Convert.ToInt32(((Arc)sender).Tag.ToString());
            }

            if (i < 4)
            {
                CenterText.Text = input0.Text;

                if (keymap[i + 1] == "" || i == 3)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie0.Tag = 0;
                    input0.Tag = 0;
                    input0.Text = keymap[0];
                }
                else
                {
                    pie0.Tag = (Convert.ToInt32(pie0.Tag.ToString()) + 1).ToString();
                    input0.Tag = (Convert.ToInt32(input0.Tag.ToString()) + 1).ToString();
                    input0.Text = keymap[i + 1];

                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 8)
            {
                CenterText.Text = input1.Text;

                if (keymap[i + 1] == "" || i == 7)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie1.Tag = 4;
                    input1.Tag = 4;
                    input1.Text = keymap[4];
                }
                else
                {
                    pie1.Tag = (Convert.ToInt32(pie1.Tag.ToString()) + 1).ToString();
                    input1.Tag = (Convert.ToInt32(input1.Tag.ToString()) + 1).ToString();
                    input1.Text = keymap[i + 1];

                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 12)
            {
                CenterText.Text = input2.Text;

                if (keymap[i + 1] == "" || i == 11)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie2.Tag = 8;
                    input2.Tag = 8;
                    input2.Text = keymap[8];
                }
                else
                {
                    pie2.Tag = (Convert.ToInt32(pie2.Tag.ToString()) + 1).ToString();
                    input2.Tag = (Convert.ToInt32(input2.Tag.ToString()) + 1).ToString();
                    input2.Text = keymap[i + 1];

                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 16)
            {
                CenterText.Text = input3.Text;

                if (keymap[i + 1] == "" || i == 15)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie3.Tag = 12;
                    input3.Tag = 12;
                    input3.Text = keymap[12];
                }
                else
                {
                    pie3.Tag = (Convert.ToInt32(pie3.Tag.ToString()) + 1).ToString();
                    input3.Tag = (Convert.ToInt32(input3.Tag.ToString()) + 1).ToString();
                    input3.Text = keymap[i + 1];
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 20)
            {
                CenterText.Text = input4.Text;

                if (keymap[i + 1] == "" || i == 19)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie4.Tag = 16;
                    input4.Tag = 16;
                    input4.Text = keymap[16];
                }
                else
                {
                    pie4.Tag = (Convert.ToInt32(pie4.Tag.ToString()) + 1).ToString();
                    input4.Tag = (Convert.ToInt32(input4.Tag.ToString()) + 1).ToString();
                    input4.Text = keymap[i + 1];
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 24)
            {
                CenterText.Text = input5.Text;

                if (keymap[i + 1] == "" || i == 23)
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie5.Tag = 20;
                    input5.Tag = 20;
                    input5.Text = keymap[20];
                }
                else
                {
                    pie5.Tag = (Convert.ToInt32(pie5.Tag.ToString()) + 1).ToString();
                    input5.Tag = (Convert.ToInt32(input5.Tag.ToString()) + 1).ToString();
                    input5.Text = keymap[i + 1];


                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
            else if (i < 28)
            {
                CenterText.Text = input6.Text;

                if (i == 27 || keymap[i + 1] == "")
                {
                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }

                    pie6.Tag = 24;
                    input6.Tag = 24;
                    input6.Text = keymap[24];
                }
                else
                {
                    pie6.Tag = (Convert.ToInt32(pie6.Tag.ToString()) + 1).ToString();
                    input6.Tag = (Convert.ToInt32(input6.Tag.ToString()) + 1).ToString();
                    input6.Text = keymap[i + 1];

                    if (CurrentLanguage == Languages.Korean)
                    {
                        CenterTextMerge(keymap, i);
                    }
                }
            }
        }

        private void InputingSentence(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InputingReset();

            string RealSendKey = ((TextBlock)sender).Text;
            CenterText.Text = RealSendKey;

            System.Windows.Forms.Clipboard.SetText(RealSendKey);
            Send sendkeys = new Send(RealSendKey, RealSendKey);
            sendkeys.Work();
        }

        private void InputingChar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (CenterText.Text != "")
            {
                System.Windows.Forms.Clipboard.SetText(CenterText.Text);

                Send sendkeys = new Send(CenterText.Text, CenterText.Text);

                if (CenterText.Text == "←")
                {
                    System.Windows.Forms.Clipboard.SetText("{BACK}");
                    sendkeys = new Send("{BACK}", "{BACK}");
                }
                else if (CurrentLanguage == Languages.Korean)
                {
                    switch (inputCount)
                    {
                        case 0:
                            inputCount++;
                            KeymapChange(KoreanJungsungKeymap);
                            koreaInputChar[0] = CenterText.Text;
                            InputingReset(true);
                            break;
                        case 1:
                            inputCount++;
                            KeymapChange(KoreanJongsungKeymap);
                            InputingReset(true);
                            break;
                        case 2:
                            inputCount = 0;
                            KeymapChange(KoreanChosungKeymap);
                            sendkeys = new Send(CenterText.Text, CenterText.Text);
                            sendkeys.Work();
                            InputingReset();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    sendkeys.Work();
                }
            }
        }

        private void BackSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText("{BACK}");
            Send sendkeys = new Send("{BACK}", "{BACK}");
            sendkeys.Work();

            InputingReset();

            CenterText.Text = "←";
        }

        private void InputingSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(" ");
            Send sendkeys = new Send(" ", " ");
            sendkeys.Work();

            InputingReset();
        }

        private void ChangeLanguage(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentLanguage)
            {
                case Languages.Korean:
                    CurrentLanguage = Languages.English;
                    KeymapChange(GetKeymapArray(CurrentLanguage));
                    BlankText.Text = "Spacing";
                    LauguageChangeText.Text = "Special";
                    break;
                case Languages.English:
                    CurrentLanguage = Languages.Special;
                    KeymapChange(GetKeymapArray(CurrentLanguage));
                    LauguageChangeText.Text = "한국어";
                    break;
                case Languages.Special:
                    CurrentLanguage = Languages.Korean;
                    KeymapChange(GetKeymapArray(CurrentLanguage));
                    BlankText.Text = "띄어쓰기";
                    LauguageChangeText.Text = "English";
                    break;
                default:
                    throw new Exception("wrong language");
            }

            InputingReset();
        }

        #endregion Keyboard
    }
}
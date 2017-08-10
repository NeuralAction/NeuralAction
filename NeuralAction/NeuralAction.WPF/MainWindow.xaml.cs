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

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public static IntPtr FocusedHandle { get; set; }
        public static bool RestoreClipboard { get; set; } = true;

        string currentlanguage = "kr";
        int inputcount = 0;
        string[] koreainputchar = new string[3];


        public MainWindow()
        {
            InitializeComponent();

        }

        private static string m_ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        private static string m_JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        private static string m_JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        private static ushort m_UniCodeHangulBase = 0xAC00;

        public static string MergeJaso(string choSung, string jungSung, string jongSung)
        {
            int ChoSungPos, JungSungPos, JongSungPos;
            int nUniCode;

            ChoSungPos = m_ChoSungTbl.IndexOf(choSung);    
            JungSungPos = m_JungSungTbl.IndexOf(jungSung); 
            JongSungPos = m_JongSungTbl.IndexOf(jongSung);  


            nUniCode = m_UniCodeHangulBase + (ChoSungPos * 21 + JungSungPos) * 28 + JongSungPos;

            char temp = Convert.ToChar(nUniCode);

            return temp.ToString();
        }



        public void english_keypad_change()
        {

            blank_text.Text = "Spacing";
            blank_text.Tag = " ";

            onepie.Tag = "g";
            twopie.Tag = "j";
            threepie.Tag = "m";
            fourpie.Tag = "p";
            fivepie.Tag = "s";
            sevenpie.Tag = "a";
            eightpie.Tag = "d";

            textblock_one.Text = "a";
            textblock_one_one.Text = "b";
            textblock_one_two.Text = "c";
            textblock_one_three.Text = "v";
            textblock_two.Text = "d";
            textblock_two_one.Text = "e";
            textblock_two_two.Text = "f";
            textblock_two_three.Text = "w";
            textblock_three.Text = "g";
            textblock_three_one.Text = "h";
            textblock_three_two.Text = "i";
            textblock_three_three.Text = "x";
            textblock_four.Text = "j";
            textblock_four_one.Text = "k";
            textblock_four_two.Text = "l";
            textblock_four_three.Text = "y";
            textblock_five.Text = "m";
            textblock_five_one.Text = "n";
            textblock_five_two.Text = "o";
            textblock_five_three.Text = "z";
            textblock_six.Text = "p";
            textblock_six_one.Text = "q";
            textblock_six_two.Text = "r";
            textblock_six_three.Text = "";
            textblock_seven.Text = "s";
            textblock_seven_one.Text = "t";
            textblock_seven_two.Text = "u";
            textblock_seven_three.Text = "";

            textblock_one.Tag = "a";
            textblock_one_one.Tag = "b";
            textblock_one_two.Tag = "c";
            textblock_one_three.Tag = "v";
            textblock_two.Tag = "d";
            textblock_two_one.Tag = "e";
            textblock_two_two.Tag = "f";
            textblock_two_three.Tag = "w";
            textblock_three.Tag = "g";
            textblock_three_one.Tag = "h";
            textblock_three_two.Tag = "i";
            textblock_three_three.Tag = "x";
            textblock_four.Tag = "j";
            textblock_four_one.Tag = "k";
            textblock_four_two.Tag = "l";
            textblock_four_three.Tag = "y";
            textblock_five.Tag = "m";
            textblock_five_one.Tag = "n";
            textblock_five_two.Tag = "o";
            textblock_five_three.Tag = "z";
            textblock_six.Tag = "p";
            textblock_six_one.Tag = "q";
            textblock_six_two.Tag = "r";
            textblock_six_three.Tag = "";
            textblock_seven.Tag = "s";
            textblock_seven_one.Tag = "t";
            textblock_seven_two.Tag = "u";
            textblock_seven_three.Tag = "";


        }

        public void korean_chosung_keypad_change() {

            blank_text.Text = "띄어쓰기";

            onepie.Tag = "ㅈ";
            twopie.Tag = "ㅂ";
            threepie.Tag = "ㄴ";
            fourpie.Tag = "ㅅ";
            fivepie.Tag = "ㅇ";
            sevenpie.Tag = "ㄱ";
            eightpie.Tag = "ㄷ";

            textblock_one.Text = "ㄱ";
            textblock_one_one.Text = "ㄲ";
            textblock_one_two.Text = "ㅋ";
            textblock_one_three.Text = "";
            textblock_two.Text = "ㄷ";
            textblock_two_one.Text = "ㅌ";
            textblock_two_two.Text = "ㄸ";
            textblock_two_three.Text = "";
            textblock_three.Text = "ㅈ";
            textblock_three_one.Text = "ㅊ";
            textblock_three_two.Text = "ㅉ";
            textblock_three_three.Text = "";
            textblock_four.Text = "ㅂ";
            textblock_four_one.Text = "ㅍ";
            textblock_four_two.Text = "ㅃ";
            textblock_four_three.Text = "";
            textblock_five.Text = "ㄴ";
            textblock_five_one.Text = "ㅁ";
            textblock_five_two.Text = "ㄹ";
            textblock_five_three.Text = "";
            textblock_six.Text = "ㅅ";
            textblock_six_one.Text = "ㅆ";
            textblock_six_two.Text = "";
            textblock_six_three.Text = "";
            textblock_seven.Text = "ㅇ";
            textblock_seven_one.Text = "ㅎ";
            textblock_seven_two.Text = "";
            textblock_seven_three.Text = "";

            textblock_one.Tag = "ㄱ";
            textblock_one_one.Tag = "ㄲ";
            textblock_one_two.Tag = "ㅋ";
            textblock_one_three.Tag = "";
            textblock_two.Tag = "ㄷ";
            textblock_two_one.Tag = "ㅌ";
            textblock_two_two.Tag = "ㄸ";
            textblock_two_three.Tag = "";
            textblock_three.Tag = "ㅈ";
            textblock_three_one.Tag = "ㅊ";
            textblock_three_two.Tag = "ㅉ";
            textblock_three_three.Tag = "";
            textblock_four.Tag = "ㅂ";
            textblock_four_one.Tag = "ㅍ";
            textblock_four_two.Tag = "ㅃ";
            textblock_four_three.Tag = "";
            textblock_five.Tag = "ㄴ";
            textblock_five_one.Tag = "ㅁ";
            textblock_five_two.Tag = "ㄹ";
            textblock_five_three.Tag = "";
            textblock_six.Tag = "ㅅ";
            textblock_six_one.Tag = "ㅆ";
            textblock_six_two.Tag = "";
            textblock_six_three.Tag = "";
            textblock_seven.Tag = "ㅇ";
            textblock_seven_one.Tag = "ㅎ";
            textblock_seven_two.Tag = "";
            textblock_seven_three.Tag = "";


        }

        public void korean_jungsung_keypad_change()
        {

            onepie.Tag = ".";
            twopie.Tag = ",";
            threepie.Tag = "0";
            fourpie.Tag = "1";
            fivepie.Tag = "2";
            sevenpie.Tag = "3";
            eightpie.Tag = "4";

            textblock_one.Text = "ㅏ";
            textblock_one_one.Text = "ㅑ";
            textblock_one_two.Text = "ㅐ";
            textblock_one_three.Text = "";
            textblock_two.Text = "ㅓ";
            textblock_two_one.Text = "ㅕ";
            textblock_two_two.Text = "ㅔ";
            textblock_two_three.Text = "";
            textblock_three.Text = "ㅗ";
            textblock_three_one.Text = "ㅛ";
            textblock_three_two.Text = "ㅚ";
            textblock_three_three.Text = "";
            textblock_four.Text = "ㅜ";
            textblock_four_one.Text = "ㅠ";
            textblock_four_two.Text = "ㅟ";
            textblock_four_three.Text = "";
            textblock_five.Text = "ㅡ";
            textblock_five_one.Text = "ㅘ";
            textblock_five_two.Text = "ㅙ";
            textblock_five_three.Text = "";
            textblock_six.Text = "ㅣ";
            textblock_six_one.Text = "ㅒ";
            textblock_six_two.Text = "ㅖ";
            textblock_six_three.Text = "";
            textblock_seven.Text = "ㅢ";
            textblock_seven_one.Text = "ㅝ";
            textblock_seven_two.Text = "ㅞ";
            textblock_seven_three.Text = "";

            textblock_one.Tag = "ㅏ";
            textblock_one_one.Tag = "ㅑ";
            textblock_one_two.Tag = "ㅐ";
            textblock_one_three.Tag = "";
            textblock_two.Tag = "ㅓ";
            textblock_two_one.Tag = "ㅕ";
            textblock_two_two.Tag = "ㅔ";
            textblock_two_three.Tag = "";
            textblock_three.Tag = "ㅗ";
            textblock_three_one.Tag = "ㅛ";
            textblock_three_two.Tag = "ㅚ";
            textblock_three_three.Tag = "";
            textblock_four.Tag = "ㅜ";
            textblock_four_one.Tag = "ㅠ";
            textblock_four_two.Tag = "ㅟ";
            textblock_four_three.Tag = "";
            textblock_five.Tag = "ㅡ";
            textblock_five_one.Tag = "ㅘ";
            textblock_five_two.Tag = "ㅙ";
            textblock_five_three.Tag = "";
            textblock_six.Tag = "ㅣ";
            textblock_six_one.Tag = "ㅒ";
            textblock_six_two.Tag = "ㅖ";
            textblock_six_three.Tag = "";
            textblock_seven.Tag = "ㅢ";
            textblock_seven_one.Tag = "ㅝ";
            textblock_seven_two.Tag = "ㅞ";
            textblock_seven_three.Tag = "";


        }

        public void korean_jongsung_keypad_change()
        {

            onepie.Tag = "ㅈ";
            twopie.Tag = "ㅂ";
            threepie.Tag = "ㄴ";
            fourpie.Tag = "ㅅ";
            fivepie.Tag = "ㅇ";
            sevenpie.Tag = "ㄱ";
            eightpie.Tag = "ㄷ";

            textblock_one.Text = "ㄱ";
            textblock_one_one.Text = "ㄲ";
            textblock_one_two.Text = "ㅋ";
            textblock_one_three.Text = "ㄺ";
            textblock_two.Text = "ㄷ";
            textblock_two_one.Text = "ㅌ";
            textblock_two_two.Text = "ㄸ";
            textblock_two_three.Text = "ㄼ";
            textblock_three.Text = "ㅈ";
            textblock_three_one.Text = "ㅊ";
            textblock_three_two.Text = "ㅉ";
            textblock_three_three.Text = "ㄶ";
            textblock_four.Text = "ㅂ";
            textblock_four_one.Text = "ㅍ";
            textblock_four_two.Text = "ㅃ";
            textblock_four_three.Text = "ㅄ";
            textblock_five.Text = "ㄴ";
            textblock_five_one.Text = "ㅁ";
            textblock_five_two.Text = "ㄹ";
            textblock_five_three.Text = "ㄻ";
            textblock_six.Text = "ㅅ";
            textblock_six_one.Text = "ㅆ";
            textblock_six_two.Text = "ㅄ";
            textblock_six_three.Text = "ㄳ";
            textblock_seven.Text = "ㅇ";
            textblock_seven_one.Text = "ㅎ";
            textblock_seven_two.Text = "ㄶ";
            textblock_seven_three.Text = "ㅀ";

            textblock_one.Tag = "ㄱ";
            textblock_one_one.Tag = "ㄲ";
            textblock_one_two.Tag = "ㅋ";
            textblock_one_three.Tag = "ㄺ";
            textblock_two.Tag = "ㄷ";
            textblock_two_one.Tag = "ㅌ";
            textblock_two_two.Tag = "ㄸ";
            textblock_two_three.Tag = "ㄼ";
            textblock_three.Tag = "ㅈ";
            textblock_three_one.Tag = "ㅊ";
            textblock_three_two.Tag = "ㅉ";
            textblock_three_three.Tag = "ㄶ";
            textblock_four.Tag = "ㅂ";
            textblock_four_one.Tag = "ㅍ";
            textblock_four_two.Tag = "ㅃ";
            textblock_four_three.Tag = "ㅄ";
            textblock_five.Tag = "ㄴ";
            textblock_five_one.Tag = "ㅁ";
            textblock_five_two.Tag = "ㄹ";
            textblock_five_three.Tag = "ㄻ";
            textblock_six.Tag = "ㅅ";
            textblock_six_one.Tag = "ㅆ";
            textblock_six_two.Tag = "ㅄ";
            textblock_six_three.Tag = "ㄳ";
            textblock_seven.Tag = "ㅇ";
            textblock_seven_one.Tag = "ㅎ";
            textblock_seven_two.Tag = "ㄶ";
            textblock_seven_three.Tag = "ㅀ";



        }

        public void specialchar_keypad_change()
        {

            onepie.Tag = ".";
            twopie.Tag = ",";
            threepie.Tag = "0";
            fourpie.Tag = "1";
            fivepie.Tag = "2";
            sevenpie.Tag = "3";
            eightpie.Tag = "4";
      

            textblock_one.Text = ".";
            textblock_one_one.Text = "5";
            textblock_one_two.Text = "(";
            textblock_one_three.Text = "";
            textblock_two.Text = ",";
            textblock_two_one.Text = "6";
            textblock_two_two.Text = ")";
            textblock_two_three.Text = "";
            textblock_three.Text = "0";
            textblock_three_one.Text = "7";
            textblock_three_two.Text = "&";
            textblock_three_three.Text = "";
            textblock_four.Text = "1";
            textblock_four_one.Text = "8";
            textblock_four_two.Text = "+";
            textblock_four_three.Text = "";
            textblock_five.Text = "2";
            textblock_five_one.Text = "9";
            textblock_five_two.Text = "-";
            textblock_five_three.Text = "";
            textblock_six.Text = "3";
            textblock_six_one.Text = "!";
            textblock_six_two.Text = "*";
            textblock_six_three.Text = "";
            textblock_seven.Text = "4";
            textblock_seven_one.Text = "?";
            textblock_seven_two.Text = "/";
            textblock_seven_three.Text = "";


            textblock_one.Tag = ".";
            textblock_one_one.Tag = "5";
            textblock_one_two.Tag = "(";
            textblock_one_three.Tag = "";
            textblock_two.Tag = ",";
            textblock_two_one.Tag = "6";
            textblock_two_two.Tag = ")";
            textblock_two_three.Tag = "";
            textblock_three.Tag = "0";
            textblock_three_one.Tag = "7";
            textblock_three_two.Tag = "&";
            textblock_three_three.Tag = "";
            textblock_four.Tag = "1";
            textblock_four_one.Tag = "8";
            textblock_four_two.Tag = "+";
            textblock_four_three.Tag = "";
            textblock_five.Tag = "2";
            textblock_five_one.Tag = "9";
            textblock_five_two.Tag = "-";
            textblock_five_three.Tag = "";
            textblock_six.Tag = "3";
            textblock_six_one.Tag = "!";
            textblock_six_two.Tag = "*";
            textblock_six_three.Tag = "";
            textblock_seven.Tag = "4";
            textblock_seven_one.Tag = "?";
            textblock_seven_two.Tag = "/";
            textblock_seven_three.Tag = "";


        }

        CursorIcon CursorIcon = new CursorIcon();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotWindowsFocus();
        }

        void NotWindowsFocus() {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
            GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
            CursorIcon.Show();
        }

        private void Inputing_eyes(object sender, System.Windows.Input.MouseEventArgs e)
        {



            if (currentlanguage == "kr") {

                if (sender.GetType().ToString() == "System.Windows.Controls.TextBlock")
                {

                    string RealSendKey = ((TextBlock)sender).Tag.ToString();
                    string CenterKey = ((TextBlock)sender).Tag.ToString();



                    if (RealSendKey == "Backspace")
                    {

                        RealSendKey = "{BACK}";
                        centertext.Text = "←";
                        inputcount = 0;

                        korean_chosung_keypad_change();

                        System.Windows.Forms.Clipboard.SetText(RealSendKey);

                        Send sendkeys = new Send(CenterKey, RealSendKey);

                        sendkeys.Work();

                    }
                    else
                    {

                        centertext.Text = CenterKey;

                        if (inputcount == 0)
                        {
                            korean_jungsung_keypad_change();
                            inputcount++;
                            koreainputchar[0] = RealSendKey;
                        }
                        else if (inputcount == 1)
                        {
                            korean_jongsung_keypad_change();
                            inputcount++;
                            koreainputchar[1] = RealSendKey;
                            centertext.Text = MergeJaso(koreainputchar[0], koreainputchar[1], "");
                        }
                        else if (inputcount == 2)
                        {
                            korean_chosung_keypad_change();
                            inputcount = 0;
                            koreainputchar[2] = RealSendKey;
                            centertext.Text = MergeJaso(koreainputchar[0], koreainputchar[1], koreainputchar[2]);

                            System.Windows.Forms.Clipboard.SetText(RealSendKey);
                            Send sendkeys = new Send(CenterKey, RealSendKey);
                            sendkeys.Work();

                            koreainputchar[0] = "";
                            koreainputchar[1] = "";
                            koreainputchar[2] = "";

                        }
                    }

                }
                else if (sender.GetType().ToString() == "Microsoft.Expression.Shapes.Arc")
                {

                    string RealSendKey = ((Arc)sender).Tag.ToString();
                    string CenterKey = ((Arc)sender).Tag.ToString();

                    if (RealSendKey == "Backspace")
                    {

                        RealSendKey = "{BACK}";
                        centertext.Text = "←";
                        inputcount = 0;
                        korean_chosung_keypad_change();

                        System.Windows.Forms.Clipboard.SetText(RealSendKey);

                        Send sendkeys = new Send(CenterKey, RealSendKey);

                        sendkeys.Work();

                    }
                    else
                    {

                        if (inputcount == 0)
                        {
                            korean_jungsung_keypad_change();
                            inputcount++;
                            koreainputchar[0] = RealSendKey;
                        }
                        else if (inputcount == 1)
                        {
                            korean_jongsung_keypad_change();
                            inputcount++;
                            koreainputchar[1] = RealSendKey;
                            centertext.Text = MergeJaso(koreainputchar[0], koreainputchar[1], "");
                        }
                        else if (inputcount == 2)
                        {
                            korean_chosung_keypad_change();
                            inputcount = 0;
                            koreainputchar[2] = RealSendKey;
                            centertext.Text = MergeJaso(koreainputchar[0], koreainputchar[1], koreainputchar[2]);

                            System.Windows.Forms.Clipboard.SetText(RealSendKey);
                            Send sendkeys = new Send(CenterKey, RealSendKey);
                            sendkeys.Work();
                            centertext.Text = "";
                            koreainputchar[0] = "";
                            koreainputchar[1] = "";
                            koreainputchar[2] = "";


                        }
                    }


                }

            } else if (currentlanguage == "en" || currentlanguage == "sp") {

                if (sender.GetType().ToString() == "System.Windows.Controls.TextBlock")
                {

                    string RealSendKey = ((TextBlock)sender).Tag.ToString();
                    string CenterKey = ((TextBlock)sender).Tag.ToString();

                    if (RealSendKey == "Backspace")  {

                        RealSendKey = "{BACK}";
                        centertext.Text = "←";
                        inputcount = 0;
                       
                        System.Windows.Forms.Clipboard.SetText(RealSendKey);
                        Send sendkeys = new Send(CenterKey, RealSendKey);
                        sendkeys.Work();

                    }  else  {

                        centertext.Text = CenterKey;
                        System.Windows.Forms.Clipboard.SetText(RealSendKey);
                        Send sendkeys = new Send(CenterKey, RealSendKey);
                        sendkeys.Work();
                    }

                }  else if (sender.GetType().ToString() == "Microsoft.Expression.Shapes.Arc") {

                    string RealSendKey = ((Arc)sender).Tag.ToString();
                    string CenterKey = ((Arc)sender).Tag.ToString();

                    if (RealSendKey == "Backspace")
                    {

                        RealSendKey = "{BACK}";
                        centertext.Text = "←";
                        inputcount = 0;

                        System.Windows.Forms.Clipboard.SetText(RealSendKey);

                        Send sendkeys = new Send(CenterKey, RealSendKey);

                        sendkeys.Work();

                    } else  {

                        centertext.Text = CenterKey;
                        System.Windows.Forms.Clipboard.SetText(RealSendKey);
                        Send sendkeys = new Send(CenterKey, RealSendKey);
                        sendkeys.Work();
                    }
                }
            }
        }

        private void Inputing_sentence(object sender, System.Windows.Input.MouseEventArgs e)
        {

            string RealSendKey = ((TextBlock)sender).Tag.ToString();

            korean_chosung_keypad_change();

            koreainputchar[0] = "";
            koreainputchar[1] = "";
            koreainputchar[2] = "";

            centertext.Text = RealSendKey;

            inputcount = 0;

            korean_chosung_keypad_change();

            System.Windows.Forms.Clipboard.SetText(RealSendKey);

            Send sendkeys = new Send(RealSendKey, RealSendKey);

            sendkeys.Work();

        }

        private void Inputing_char(object sender, System.Windows.Input.MouseEventArgs e)
        {


            string inputtext = centertext.Text;

            if (inputtext == "") {

            }  else {

                if (inputtext == "←")
                {
                    inputtext = "{BACK}";
                }

                System.Windows.Forms.Clipboard.SetText(inputtext);

                Send sendkeys = new Send(centertext.Text, centertext.Text);

                sendkeys.Work();

                koreainputchar[0] = "";
                koreainputchar[1] = "";
                koreainputchar[2] = "";

                centertext.Text = "";

                inputcount = 0;

            }

        }
    



        private void inputing_space(object sender, System.Windows.Input.MouseEventArgs e)
        {


                System.Windows.Forms.Clipboard.SetText(" ");

                Send sendkeys = new Send(" ", " ");

                sendkeys.Work();

                koreainputchar[0] = "";
                koreainputchar[1] = "";
                koreainputchar[2] = "";

                centertext.Text = "";

            
        }

        private void KeypadRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NotWindowsFocus();
        }

        private void Ellipse_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Topmost = false;
            CursorIcon.Topmost = true;
        }

        private void Ellipse_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Topmost = true;
            CursorIcon.Topmost = false;
        }

        private void change_language(object sender, MouseButtonEventArgs e)
        {
            if(currentlanguage == "kr")
            {

                currentlanguage = "en";
                english_keypad_change();



                mark_text.Text = "Special";

            } else if(currentlanguage == "en") {

                currentlanguage = "sp";
                specialchar_keypad_change();

                mark_text.Text = "한국어";

            } else if(currentlanguage == "sp") {

                currentlanguage = "kr";
                korean_chosung_keypad_change();

                mark_text.Text = "English";

            }

            koreainputchar[0] = "";
            koreainputchar[1] = "";
            koreainputchar[2] = "";

            centertext.Text = "";

            inputcount = 0;

        }

       
    }
}

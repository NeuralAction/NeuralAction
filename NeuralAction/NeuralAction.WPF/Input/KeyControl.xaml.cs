using Microsoft.Expression.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Predict;

namespace NeuralAction.WPF
{
    public enum Languages
    {
        Korean = 0,
        English = 1,
        Special = 2
    }

    public partial class KeyControl : UserControl
    {

        WordCorrecter AutocompleteWord;

        static string ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        static string JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        static string JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        static ushort UniCodeHangulBase = 0xAC00;

        static string[] EnglishKeymap = new string[28] { "a", "b", "c", "v", "d", "e", "f", "w", "g", "h", "i", "x", "j", "k", "l", "y", "m", "n", "o", "z", "p", "q", "r", "", "s", "t", "u", "" };
        static string[] KoreanChosungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "", "ㄷ", "ㅌ", "ㄸ", "", "ㅈ", "ㅊ", "ㅉ", "", "ㅂ", "ㅍ", "ㅃ", "", "ㄴ", "ㅁ", "ㄹ", "", "ㅅ", "ㅆ", "", "", "ㅇ", "ㅎ", "", "" };
        static string[] KoreanJungsungKeymap = new string[28] { "ㅏ", "ㅑ", "ㅐ", "", "ㅓ", "ㅕ", "ㅔ", "", "ㅗ", "ㅛ", "ㅚ", "", "ㅜ", "ㅠ", "ㅟ", "", "ㅡ", "ㅘ", "ㅙ", "", "ㅣ", "ㅒ", "ㅖ", "", "ㅢ", "ㅝ", "ㅞ", "" };
        static string[] KoreanJongsungKeymap = new string[28] { "ㄱ", "ㄲ", "ㅋ", "ㄺ", "ㄷ", "ㅌ", "ㄸ", "ㄼ", "ㅈ", "ㅊ", "ㅉ", "ㄶ", "ㅂ", "ㅍ", "ㅃ", "ㅄ", "ㄴ", "ㅁ", "ㄹ", "ㄻ", "ㅅ", "ㅆ", "ㅄ", "ㄳ", "ㅇ", "ㅎ", "ㄶ", "ㅀ" };
        static string[] SpecialCharKeymap = new string[28] { ".", "5", "(", "", ",", "6", ")", "", "0", "7", "&", "", "1", "8", "+", "", "2", "9", "-", "", "3", "!", "*", "", "4", "?", "/", "" };


        string wordtemp = "";
        public static string MergeJaso(string choSung, string jungSung, string jongSung)
        {
            var ChoSungInd = ChoSungTbl.IndexOf(choSung);
            var JungSungInd = JungSungTbl.IndexOf(jungSung);
            var JongSungInd = JongSungTbl.IndexOf(jongSung);

            var Unicode = UniCodeHangulBase + (ChoSungInd * 21 + JungSungInd) * 28 + JongSungInd;

            return $"{(char)Unicode}";
        }

        public Languages CurrentLanguage { get; set; } = Languages.Korean;

        string[] koreaInputChar = new string[3];
        int inputCount = 0;
        
        public KeyControl()
        {
            InitializeComponent();
        }

        #region UI Events

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

        private void PieMouseEnter(object sender, MouseEventArgs e)
        {
            Arc MouseEnterArc = (Arc)sender;

            var hoverColor = new SolidColorBrush(Color.FromRgb(54, 222, 155));
            var hoverTextColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            hoverColor.Freeze();
            hoverTextColor.Freeze();

            MouseEnterArc.Fill = hoverColor;

            switch (MouseEnterArc.Name)
            {
                case "pie0":
                    input0.Foreground = hoverTextColor;
                    break;
                case "pie1":
                    input1.Foreground = hoverTextColor;
                    break;
                case "pie2":
                    input2.Foreground = hoverTextColor;
                    break;
                case "pie3":
                    input3.Foreground = hoverTextColor;
                    break;
                case "pie4":
                    input4.Foreground = hoverTextColor;
                    break;
                case "pie5":
                    input5.Foreground = hoverTextColor;
                    break;
                case "pie6":
                    input6.Foreground = hoverTextColor;
                    break;
                case "pie7":
                    input7.Foreground = hoverTextColor;
                    break;
                default:
                    throw new ArgumentException("unknown exception");
            }
        }

        private void PieMouseLeave(object sender, MouseEventArgs e)
        {
            Arc MouseEnterArc = (Arc)sender;

            var normalColor = new SolidColorBrush(Color.FromRgb(54, 54, 54));
            var normalTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            normalColor.Freeze();
            normalTextColor.Freeze();

            MouseEnterArc.Fill = normalColor;

            switch (MouseEnterArc.Name)
            {
                case "pie0":
                    input0.Foreground = normalTextColor;
                    break;
                case "pie1":
                    input1.Foreground = normalTextColor;
                    break;
                case "pie2":
                    input2.Foreground = normalTextColor;
                    break;
                case "pie3":
                    input3.Foreground = normalTextColor;
                    break;
                case "pie4":
                    input4.Foreground = normalTextColor;
                    break;
                case "pie5":
                    input5.Foreground = normalTextColor;
                    break;
                case "pie6":
                    input6.Foreground = normalTextColor;
                    break;
                case "pie7":
                    input7.Foreground = normalTextColor;
                    break;
                default:
                    throw new ArgumentException("unknown exception");
            }
        }


        private void TextMouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock MouseEnterTextBlock = (TextBlock)sender;

            var hoverColor = new SolidColorBrush(Color.FromRgb(54, 222, 155));
            var hoverTextColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            hoverColor.Freeze();
            hoverTextColor.Freeze();

            MouseEnterTextBlock.Foreground = hoverTextColor;

            switch (MouseEnterTextBlock.Name)
            {
                case "input0":
                    pie0.Fill= hoverColor;
                    break;
                case "input1":
                    pie1.Fill = hoverColor;
                    break;
                case "input2":
                    pie2.Fill = hoverColor;
                    break;
                case "input3":
                    pie3.Fill = hoverColor;
                    break;
                case "input4":
                    pie4.Fill = hoverColor;
                    break;
                case "input5":
                    pie5.Fill = hoverColor;
                    break;
                case "input6":
                    pie6.Fill = hoverColor;
                    break;
                case "input7":
                    pie7.Fill = hoverColor;
                    break;
                default:
                    throw new ArgumentException("unknown exception");
            }
        }

        private void TextMouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock MouseEnterTextBlock = (TextBlock)sender;

            var normalColor = new SolidColorBrush(Color.FromRgb(54, 54, 54));
            var normalTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            normalColor.Freeze();
            normalTextColor.Freeze();

            MouseEnterTextBlock.Foreground = normalColor;

            switch (MouseEnterTextBlock.Name)
            {
                case "input0":
                    pie0.Fill = normalColor;
                    break;
                case "input1":
                    pie1.Fill = normalColor;
                    break;
                case "input2":
                    pie2.Fill = normalColor;
                    break;
                case "input3":
                    pie3.Fill = normalColor;
                    break;
                case "input4":
                    pie4.Fill = normalColor;
                    break;
                case "input5":
                    pie5.Fill = normalColor;
                    break;
                case "input6":
                    pie6.Fill = normalColor;
                    break;
                case "input7":
                    pie7.Fill = normalColor;
                    break;
                default:
                    throw new ArgumentException("unknown exception");
            }
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


            if (wordtemp.Length > 1)
            {
                wordtemp.Substring(0, wordtemp.Length);
                wordtemp.Insert(wordtemp.Length, CenterText.Text);

            } else if(wordtemp.Length <= 1) {
                wordtemp = CenterText.Text;
            }

            MessageBox.Show(wordtemp.Length + "  " + wordtemp);

           WordSuggestions[] Autocompletes =  AutocompleteWord.Correcting(wordtemp.Trim());

            if (Autocompletes != null)
            {


                int counts = Autocompletes.Length;
               

                autocomplete1.Text = counts <= 0 ? "완성된 단어가 없습니다." : Autocompletes[0].Name;
                autocomplete2.Text = counts >= 2 ? Autocompletes[1].Name : "완성된 단어가 없습니다.";
                autocomplete3.Text = counts >= 3 ? Autocompletes[2].Name : "완성된 단어가 없습니다.";
                autocomplete4.Text = counts >= 4 ? Autocompletes[3].Name : "완성된 단어가 없습니다.";
                autocomplete5.Text = counts >= 5 ? Autocompletes[4].Name : "완성된 단어가 없습니다.";
                autocomplete6.Text = counts >= 6 ? Autocompletes[5].Name : "완성된 단어가 없습니다.";

                Gautocomplete1.Tag = counts <= 0 ? "" : Autocompletes[0].Name;
                Gautocomplete2.Tag = counts >= 2 ? Autocompletes[1].Name : "";
                Gautocomplete3.Tag = counts >= 3 ? Autocompletes[2].Name : "";
                Gautocomplete4.Tag = counts >= 4 ? Autocompletes[3].Name : "";
                Gautocomplete5.Tag = counts >= 5 ? Autocompletes[4].Name : "";
                Gautocomplete6.Tag = counts >= 6 ? Autocompletes[5].Name : "";

            } else {
                autocomplete1.Text = "완성된 단어가 없습니다.";
                autocomplete2.Text = "완성된 단어가 없습니다.";
                autocomplete3.Text = "완성된 단어가 없습니다.";
                autocomplete4.Text = "완성된 단어가 없습니다.";
                autocomplete5.Text = "완성된 단어가 없습니다.";
                autocomplete6.Text = "완성된 단어가 없습니다.";
                Gautocomplete1.Tag = "";
                Gautocomplete2.Tag = "";
                Gautocomplete3.Tag = "";
                Gautocomplete4.Tag = "";
                Gautocomplete5.Tag = "";
                Gautocomplete6.Tag = "";
            }

        }

        private void InputingSentence(object sender, System.Windows.Input.MouseEventArgs e)
        {
            wordtemp = "";
            InputingReset(true);

            string RealSendKey = ((Grid)sender).Tag.ToString();

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
                            wordtemp.Insert(wordtemp.Length, " ");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    wordtemp.Insert(wordtemp.Length, " ");
                    InputingReset();
                    sendkeys.Work();
                }
            }
        }

        private void BackSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText("{BACK}");
            Send sendkeys = new Send("{BACK}", "{BACK}");
            sendkeys.Work();
            wordtemp = "";
            InputingReset();

            CenterText.Text = "←";
        }

        private void InputingSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(" ");
            Send sendkeys = new Send(" ", " ");
            sendkeys.Work();
            wordtemp = "";
            InputingReset();
        }


        private void ChangeLanguage(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentLanguage)
            {
                case Languages.Korean:
                    CurrentLanguage = Languages.English;
                    AutocompleteWord = new WordCorrecter(System.Environment.CurrentDirectory + "\\Database\\englishdatabase.xml");
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
                    AutocompleteWord = new WordCorrecter(Environment.CurrentDirectory + "\\Database\\koreandatabase.xml");
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AutocompleteWord = new WordCorrecter(Environment.CurrentDirectory + "\\Database\\koreandatabase.xml");
        }
    }
}

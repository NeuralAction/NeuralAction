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
using System.Windows.Markup;
using System.Windows.Media.Animation;

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
            var ChoSungInd = ChoSungTbl.IndexOf(choSung);
            var JungSungInd = JungSungTbl.IndexOf(jungSung);
            var JongSungInd = JongSungTbl.IndexOf(jongSung);

            var Unicode = UniCodeHangulBase + (ChoSungInd * 21 + JungSungInd) * 28 + JongSungInd;

            return $"{(char)Unicode}";
        }

        public event EventHandler Closed;

        public Languages CurrentLanguage { get; set; } = Languages.Korean;

        WordCorrecter AutocompleteWord
        {
            get
            {
                switch (CurrentLanguage)
                {
                    case Languages.Korean:
                        return KoreanCorrecter;
                    case Languages.English:
                        return EnglishCorrecter;
                    case Languages.Special:
                    default:
                        return null;
                }
            }
        }
        WordCorrecter KoreanCorrecter;
        WordCorrecter EnglishCorrecter;

        Storyboard KeyOn;
        Storyboard KeyOff;

        string[] koreaInputChar = new string[3];
        string wordtemp = "";
        int inputCount = 0;
        bool temporcomplete = false;

        public KeyControl()
        {
            InitializeComponent();

            SignCache(Grid_Big);

            Task.Factory.StartNew(() =>
            {
                KoreanCorrecter = new WordCorrecter(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "CorrectionKorean.xml"));
                EnglishCorrecter = new WordCorrecter(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "CorrectionEnglish.xml"));
            });

            KeyOn = (Storyboard)FindResource("KeyOn");
            KeyOff = (Storyboard)FindResource("KeyOff");
            KeyOff.Completed += delegate
            {
                Closed?.Invoke(this, null);
            };
        }

        void SignCache(Panel element)
        {
            foreach (var item in element.Children)
            {
                var control = (UIElement)item;
                if (control.CacheMode == null)
                {
                    control.CacheMode = new BitmapCache()
                    {
                        SnapsToDevicePixels = true,
                    };
                }
                if (control is Panel)
                    SignCache((Panel)control);
            }
        }

        #region UI Events

        public void Show()
        {
            KeyOn.Begin();
        }

        bool isClosed = false;
        public void Close()
        {
            if (isClosed)
                return;
            isClosed = true;
            KeyOff.Begin();

            Task.Factory.StartNew(() => 
            {
                KoreanCorrecter?.Dispose();
                KoreanCorrecter = null;
                EnglishCorrecter?.Dispose();
                EnglishCorrecter = null;
            });
        }

        void PieMouseEnter(object sender, MouseEventArgs e)
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

        void PieMouseLeave(object sender, MouseEventArgs e)
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
                    switch (inputCount)
                    {
                        case 0:
                            return KoreanChosungKeymap;
                        case 1:
                            return KoreanJungsungKeymap;
                        case 2:
                            return KoreanJongsungKeymap;
                        default:
                            throw new NotImplementedException();
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

        void KeyChange(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int i = 0;
            string[] keymap = GetKeymapArray(CurrentLanguage);

            if (sender is TextBlock)
            {
                throw new Exception("need to be arc. trun off hitTestVisible of textblock");
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
                wordtemp = wordtemp.Substring(0, wordtemp.Length - 1);
                wordtemp += CenterText.Text;
            }
            else if (wordtemp.Length <= 1)
            {
                wordtemp = CenterText.Text;
            }

            WordSuggestions[] Autocompletes = null;

            if (AutocompleteWord != null)
            {
                 Autocompletes = AutocompleteWord.Correcting(wordtemp.Replace(" ", ""));
            }

            if (Autocompletes != null)
            {
                int counts = Autocompletes.Length;

                autocomplete1.Text = counts <= 0 ? "" : Autocompletes[0].Name;
                autocomplete2.Text = counts >= 2 ? Autocompletes[1].Name : "";
                autocomplete3.Text = counts >= 3 ? Autocompletes[2].Name : "";
                autocomplete4.Text = counts >= 4 ? Autocompletes[3].Name : "";
                autocomplete5.Text = counts >= 5 ? Autocompletes[4].Name : "";
                autocomplete6.Text = counts >= 6 ? Autocompletes[5].Name : "";

                Gautocomplete1.Tag = counts <= 0 ? "" : Autocompletes[0].Name;
                Gautocomplete2.Tag = counts >= 2 ? Autocompletes[1].Name : "";
                Gautocomplete3.Tag = counts >= 3 ? Autocompletes[2].Name : "";
                Gautocomplete4.Tag = counts >= 4 ? Autocompletes[3].Name : "";
                Gautocomplete5.Tag = counts >= 5 ? Autocompletes[4].Name : "";
                Gautocomplete6.Tag = counts >= 6 ? Autocompletes[5].Name : "";
            }
            else
            {
                autocomplete1.Text = "";
                autocomplete2.Text = "";
                autocomplete3.Text = "";
                autocomplete4.Text = "";
                autocomplete5.Text = "";
                autocomplete6.Text = "";
                Gautocomplete1.Tag = "";
                Gautocomplete2.Tag = "";
                Gautocomplete3.Tag = "";
                Gautocomplete4.Tag = "";
                Gautocomplete5.Tag = "";
                Gautocomplete6.Tag = "";
            }
        }

        void InputingSentence(object sender, System.Windows.Input.MouseEventArgs e)
        {
            string RealSendKey = ((Grid)sender).Tag?.ToString();

            
            if (wordtemp.Length <= 1)
            {

            } else if(wordtemp.Length >= 2) {
                System.Windows.Forms.Clipboard.SetText("{BACK}");
                Send backspace = new Send("{BACK}", "{BACK}");
                for(int i = 0; i < wordtemp.Length - 1; i++)
                {
                    backspace.Work();
                }
            }

            System.Windows.Forms.Clipboard.SetText(RealSendKey + " ");
            Send sendkeys = new Send(RealSendKey + " ", RealSendKey + " ");
            sendkeys.Work();

            wordtemp = "";
            InputingReset();
            if (CurrentLanguage == Languages.Korean)
            {
                KeymapChange(KoreanChosungKeymap);
            }
            else
            {
                KeymapChange(GetKeymapArray(CurrentLanguage));
            }

            Gautocomplete1.Tag = "";
            Gautocomplete2.Tag = "";
            Gautocomplete3.Tag = "";
            Gautocomplete4.Tag = "";
            Gautocomplete5.Tag = "";
            Gautocomplete6.Tag = "";
            autocomplete1.Text = "";
            autocomplete2.Text = "";
            autocomplete3.Text = "";
            autocomplete4.Text = "";
            autocomplete5.Text = "";
            autocomplete6.Text = "";
            InputingReset(true);
        }

        void InputingChar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (CenterText.Text != "")
            {
                temporcomplete = true;

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
                            wordtemp += " ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    KeymapChange(GetKeymapArray(CurrentLanguage));
                    wordtemp += " ";
                    InputingReset();
                    InputingReset(true);
                    sendkeys.Work();
                }
            }
        }

        void BackSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            temporcomplete = false;

            System.Windows.Forms.Clipboard.SetText("{BACK}");
            Send sendkeys = new Send("{BACK}", "{BACK}");
            sendkeys.Work();
            wordtemp = "";
            CenterText.Text = "←";
            InputingReset();
            if (CurrentLanguage == Languages.Korean)
            {
                KeymapChange(KoreanChosungKeymap);
            }
        }

        void InputingSpace(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(" ");
            Send sendkeys = new Send(" ", " ");
            sendkeys.Work();
            wordtemp = "";
            InputingReset();
            if (CurrentLanguage == Languages.Korean)
            {
                KeymapChange(KoreanChosungKeymap);
            }
        }

        void ChangeLanguage(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentLanguage)
            {
                case Languages.Korean:
                    CurrentLanguage = Languages.English;
                    KeymapChange(GetKeymapArray(CurrentLanguage));
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
                    LauguageChangeText.Text = "English";
                    break;
                default:
                    throw new Exception("wrong language");
            }
            wordtemp = "";
            InputingReset();
        }

        #endregion Keyboard
    }
}

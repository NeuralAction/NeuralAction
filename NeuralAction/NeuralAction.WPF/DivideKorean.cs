using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralAction.WPF
{
    class DivideKorean
    {

        private const string KoreanCho = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        private const string KoreanJung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        private const string KoreanJong = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        private const ushort FirstUnicodeKorean = 0xAC00;
        private const uint LastUnicodeKorean = 0xD79F;

        public static string DivideJaso(string hanChar)
        {

            int ChoSung, JungSung, tempsung = 0, JongSung; 

            string Returning = "";

            ushort temp = 0x0000;

            for (int i = 0; i < hanChar.Length; i++)
            {
                char a = hanChar[i];
                temp = Convert.ToUInt16(a);

                // 캐릭터가 한글이 아닐 경우 처리
                if ((temp < FirstUnicodeKorean) || (temp > LastUnicodeKorean))
                    return "it's not korean";


                int nUniCode = temp - FirstUnicodeKorean;
                ChoSung = nUniCode / (21 * 28);
                nUniCode = nUniCode % (21 * 28);
                JungSung = nUniCode / 28;
                nUniCode = nUniCode % 28;
                JongSung = nUniCode;
                

                if(KoreanJung[JungSung] == 'ㅝ')
                {
                    JungSung = 13;
                    tempsung = 5;
                }

                Returning += "" + KoreanCho[ChoSung] + KoreanJung[JungSung] + KoreanJong[JongSung];

            }
            
            return Returning;
           
        }





    }
}

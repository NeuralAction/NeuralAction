using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predict
{
    public static class KoreanHelper
    {
        static string ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        static string JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        static string JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        static ushort UniCodeHangulBase = 0xAC00;
        static ushort UniCodeHangulLast = 0xD79F;

        public static string DivideJaso(string str)
        {
            int firstChar, lastChar, middleChar;
            string result = "";

            for (int i = 0; i < str.Length; i++)
            {
                ushort Temp = Convert.ToUInt16(str[i]);

                if ((Temp < UniCodeHangulBase) || (Temp > UniCodeHangulLast))
                {
                    result += str[i];
                }
                else
                {
                    lastChar = str[i] - UniCodeHangulBase;

                    firstChar = lastChar / (21 * 28);
                    lastChar = lastChar % (21 * 28);

                    middleChar = lastChar / 28;
                    lastChar = lastChar % 28;

                    if (firstChar >= 0)
                        result += ChoSungTbl[firstChar] + " ";

                    if (middleChar >= 0)
                        result += JungSungTbl[middleChar] + " ";

                    if (lastChar != 0x0000 && lastChar >= 0)
                        result += JongSungTbl[lastChar] + " ";
                }
            }

            return result;
        }
    }

    public class WordSuggestions
    {
        public int UsedCount { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class WordCorrecter
    {
        public int TopCount { get; set; } = 6;

        public string DatasetPath { get; set; }
        DataSet sentence;
        DataTable resultSentence;
        DataColumn dc1 = new DataColumn("Name", typeof(string));
        DataColumn dc2 = new DataColumn("Code", typeof(string));
        DataColumn dc3 = new DataColumn("Count", typeof(int));

        public WordCorrecter(string path)
        {
            Load(path);
        }

        public void Load(string xmlPath)
        {
            DatasetPath = xmlPath;

            sentence = GetData(xmlPath);

            resultSentence = new DataTable();

            resultSentence.Columns.Add(dc1);
            resultSentence.Columns.Add(dc2);
            resultSentence.Columns.Add(dc3);

            resultSentence.ReadXml(DatasetPath);

            foreach (DataRow dRow in sentence.Tables[0].Rows)
            {
                DataRow newRow = resultSentence.NewRow();

                newRow["Name"] = dRow["Name"].ToString();
                newRow["Code"] = dRow["Code"].ToString();
                newRow["Count"] = dRow["Count"].ToString();

                resultSentence.Rows.Add(newRow);
            }
        }

        public void Save()
        {
            resultSentence.TableName = "Data";
            resultSentence.WriteXml(DatasetPath);
            resultSentence.Reset();
            resultSentence.Columns.Add(dc1);
            resultSentence.Columns.Add(dc2);
            resultSentence.Columns.Add(dc3);
            sentence.Reset();
            sentence = GetData(DatasetPath);

            foreach (DataRow dRow in sentence.Tables[0].Rows)
            {
                string s = KoreanHelper.DivideJaso(dRow["Name"].ToString());

                DataRow newRow = resultSentence.NewRow();

                newRow["Name"] = dRow["Name"].ToString();
                newRow["Code"] = dRow["Code"].ToString();
                newRow["Count"] = dRow["Count"].ToString();

                resultSentence.Rows.Add(newRow);
            }
        }

        public WordSuggestions[] Correcting(string input)
        {
            DataTable dts = ConvertDataTable(Select(input));

            if (dts != null)
            {
                int retLen = Math.Min(TopCount, dts.Rows.Count);
                var ret = new WordSuggestions[retLen];
                for (int i = 0; i < retLen; i++)
                {
                    ret[i] = new WordSuggestions()
                    {
                        Index = i,
                        Name = (string)dts.Rows[i]["Name"],
                        UsedCount = (int)dts.Rows[i]["Count"]
                    };
                }

                return ret;
            }
            else
            {
                return null;
            }
        }

        public bool Contains(string key)
        {
            var result = Select(key);
            if(result == null ? false : result.Length > 0)
            {
                var dts = ConvertDataTable(result);
                foreach (DataRow item in dts.Rows)
                {
                    if((string)item["Name"] == key)
                        return true;
                }
            }
            return false;
        }

        public void Used(string input, bool addNew = true)
        {
            if (Contains(input))
            {
                var dRows = Select(input);
                int checker = CheckOverlapDatarow(dRows, input);
                if (checker >= 0)
                {
                    dRows[checker]["Count"] = Int32.Parse(dRows[checker]["Count"].ToString()) + 1;
                }

                Save();
            }
            else if (addNew)
            {
                Add(input);

                Save();
            }
        }

        DataRow[] Select(string key)
        {
            if (key == null)
                return null;

            var dividedParse = KoreanHelper.DivideJaso(key.Trim());
            DataRow[] dRows = resultSentence.Select($"Code LIKE '{dividedParse}' OR Code LIKE '{dividedParse}%'", "Count DESC");
            return dRows;
        }

        void Add(string key)
        {
            Console.Write("Sentences Dataset path is " + DatasetPath + "\n");

            DataRow temp = resultSentence.NewRow();
            temp["Name"] = key;
            temp["Code"] = KoreanHelper.DivideJaso(key);
            temp["Count"] = 0;
            resultSentence.Rows.Add(temp);

            Console.Write(key + " has added in database! (" + DatasetPath + ")\n");
        }

        DataSet GetData(string path)
        {
            DataSet temp = new DataSet();
            try
            {
                temp.ReadXml(path);
            }
            catch (Exception ex)
            {
                Console.Write("ERROR: " + ex.ToString());
            }
            return temp;
        }

        DataTable ConvertDataTable(DataRow[] dRows)
        {
            DataTable returnDataTable;

            if (dRows.Length > 0)
                returnDataTable = dRows[0].Table.Clone();
            else
                return null;

            foreach (DataRow dRow in dRows)
            {
                DataRow row = returnDataTable.NewRow();
                row.ItemArray = ((object[])dRow.ItemArray.Clone());

                returnDataTable.Rows.Add(row);
            }

            return returnDataTable;
        }

        int CheckOverlapDatarow(DataRow[] dRows, string ReadSentence)
        {
            int check = -1;

            for (int i = 0; i < dRows.Length; i++)
            {
                if (dRows[i]["Code"].ToString() == KoreanHelper.DivideJaso(ReadSentence))
                {
                    check = i;
                }
            }
            return check;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConvertSQLToJS
{
    class Program
    {
        static bool isTest = false;
        public static string[] Ids;
        static void Main(string[] args)
        {
            string ruleKey = "";
            string ruleName = "";
            string input;
            read(out input, out ruleKey, out ruleName);
            Ids= input.GetIDsFromString();

            string codeText = string.Empty;
           

            codeText += CreateHeader();
            codeText += CreateImporstAndConsts(ruleKey, ruleName);
            codeText += CreateIDsInitialization(Ids);
            codeText += input.ToLower().createIfCondition();

            codeText += "\n"+"".createError(Ids.First());
            codeText += "};";

         string   dirPath= readDir();
            write(codeText, needToCreateDir: isTest , dirPath: dirPath, dirName: ruleKey);


        }

        private static string readDir(string path="directory.txt")
        {
            return File.ReadAllText(path,Encoding.UTF8);
        }

        static void WriteWithDirectory(string path )
        {
            System.IO.Directory.CreateDirectory(path);
             
        }
        static void WriteWithDirectory_File(string path, string answer)
        {
            System.IO.File.WriteAllText(path+"/main.js", answer);

        }
        static void read( out string input, out string id, out string title,string fileName = "input.txt")
        {

            using (var reader = new StreamReader(fileName, encoding: Encoding.GetEncoding(1251)))
            {
                id = reader.ReadLine();
                title = reader.ReadLine();
                input = reader.ReadToEnd();

            }
            return ;
        }
        static void write(string answer, string fileName = "output.txt", bool needToCreateDir = false, string dirPath="" ,string dirName ="")
        {
            
            File.WriteAllText(fileName, answer + Environment.NewLine, Encoding.UTF8);
            if (needToCreateDir) { WriteWithDirectory(dirPath + "/" + dirName); WriteWithDirectory_File(dirPath + "/" + dirName, answer); }
            return;
        }
        static string CreateHeader(string author = "Gilman M.M", string mail = "gilman.mm@parma.ru")
        {
            return $"/*THIS  CODE WAS GENERATED AUTOMATICALLY  at {DateTime.Now}  By {author}. E-mail {mail}.*/"+"\n";
        }
        static string CreateImporstAndConsts(string ruleKey, string ruleName)
        {
          return  @"import getReqValue from 'storage/FLC/basic/getReqValue';
import isEmpty from 'storage/FLC/extended/isEmpty';
import {
ErrorHelper
}
from 'storage/FLC/system/ErrorHelper';

const ruleKey = "+ruleKey+@";
const ruleName = '" + ruleName + "';\n" +
"export default function main() {\n";
        }
        static string CreateIDsInitialization(string [] ids)
        {
            string Itits = "";
            foreach (var item in ids)
            {
                Itits += CreateIDInitialization(item);
            }
            return Itits;
        }

            static string CreateIDInitialization(string id, string title="")
        {
            
            return $" var reqID{id} = {id};\n" +
                $"var valueID{id} = getReqValue(reqID{id});" +
                $" //{title} \n";
        }


    }

    public static class Extensions
    {
        /// <summary>
        /// ищет числа между # # в строке sql запросв
        /// </summary>
        /// <param name="input"> входная строка</param>
        /// <param name="regexPatthern"> regex шаблон, по умолчанию на поиск чисел между '#' </param>
        /// <returns></returns>
        public static string[] GetIDsFromString(this string input, string regexPatthern=@"#\d+#")
        {
            Regex regex = new Regex(regexPatthern);
             var matches = Regex.Matches(input, regexPatthern)
                       .Cast<Match>()
                       .ToArray().Select(elem=> elem.ToString()).Select(x=>x.Replace("#","")).Distinct().ToArray();
            return matches;
        }
        public static string createIfCondition(this string input)
        {
            string jsInput = input;
            jsInput = JS_Replacer.ReplaceCase(jsInput);
            jsInput = JS_Replacer.ReplaceNot(jsInput);
            jsInput = JS_Replacer.ReplaceOr(jsInput);
            jsInput = JS_Replacer.ReplaceAnd(jsInput);
            jsInput = JS_Replacer.ReplaceEnd(jsInput);
            jsInput = JS_Replacer.ReplaceNvl(jsInput);
            jsInput = JS_Replacer.ReplaceIsNull(jsInput);
            jsInput = JS_Replacer.ReplaceLength(jsInput);
            jsInput = JS_Replacer.ReplaceRegex(jsInput);


            //Оставить последним поскольку метод заменяет все неиспользованные #ID# на valueID, обработчики выше ищут в строках именно #ID#
            jsInput = JS_Replacer.ReplaceDate(jsInput);


            return jsInput;
        }
        public static string createError(this string input, string id1, string id2="")
        {
            string error = $"{{" +
                $" ErrorHelper.addNeedValueError(" +
                $"reqID{id1},"+
                $"0," + 
                $"ruleKey," + 
                $"ruleName," + 
                $"[], " + 
                $"{id2}" +
                  $");" +
                  $"}}";
 


            return error;
        }

    }
    public static class JS_Replacer
    {

        public static string ReplaceCase(string str)
        {
            return str.ToLower().Replace("case when", "if( ");
        }
        public static string ReplaceNot(string str)
        {
            return str.ToLower().Replace("not", " ! ");
        }
        public static string ReplaceAnd(string str)
        {
            return str.ToLower().Replace("and", " && ");
        }
        public static string ReplaceOr(string str)
        {
            return str.ToLower().Replace("or", " || ");
        }
        //
        public static string ReplaceEnd(string str)
        {
            return str.ToUpper().Replace("THEN 1 ELSE 0 END", " ) ").ToLower();
        }
        public static string ReplaceNvl(string str)
        {
            string newStr = str;
            var matches = Regex.Matches(str, @"nvl\(#\d+#, -1\) in \(((\d)*(,)*)+\)")
                .Cast<Match>()
                .ToArray();
            foreach (var match in matches)
            {
                string ID = Regex.Match(match.Value, @"#\d+#").Value.Replace("#", "");
                string IN = match.Value.Split('(').Last().Replace(")","") ;

                newStr = newStr.Replace(match.Value, $"valueIn((valueID{ID}==null? -1:valueID{ID}), [{IN}])");
            }

            return newStr;
        }
        public static string ReplaceDate(string str)
        {
            string newStr = str;
            var matches = Regex.Matches(str, @"#\d+#")
                .Cast<Match>()
                .ToArray();
            foreach (var match in matches)
            {
                string ID = Regex.Match(match.Value, @"#\d+#").Value.Replace("#", "");

                newStr = newStr.Replace(match.Value, $"valueID{ID}");
            }

            return newStr;
        }

            public static string ReplaceIsNull(string str)
        {
            string newStr = str;
            var matches = Regex.Matches(str, @"#\d+# is null")
                .Cast<Match>()
                .ToArray();
             foreach (var match in matches)
             {
                 string ID = Regex.Match(match.Value, @"#\d+#").Value.Replace("#","");

                newStr = newStr.Replace(match.Value, $"isEmpty(valueID{ID})");
             }

            return newStr;
                
              
        }

        /// <summary>
        /// Replace all Regex Sql exp 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceRegex(string str)
        {
            /* REGEXP_LIKE(#71153951#, '^(\d){1}\.(\d){2}\.([A-Z0-9А-Я]){8}\.(\d){6}$')*/

            /*(/{regex}/.test(valueID{}))*/
            string newStr = str;
            var matches = Regex.Matches(str, @"regexp_like\(#\d+#,.*\'\^(\S)+\$\'\)")
                .Cast<Match>()
                .ToArray();
            foreach (var match in matches)
            {
                string regex = match.Value.Substring(match.Value.IndexOf(@"'")).Replace(@"'",@"/").TrimEnd(')');
                string ID = Regex.Match(match.Value, @"#\d+#").Value.Replace("#", "");
                newStr = newStr.Replace(match.Value, $"{regex}.test(valueID{ID})");
            }

            return newStr;
        }
        public static string ReplaceLength(string str)
        {
            string newStr = str;
            var matches = Regex.Matches(str, @"length\(#\d+#\)")
                .Cast<Match>()
                .ToArray();
            foreach (var match in matches)
            {
                string ID = Regex.Match(match.Value, @"#\d+#").Value.Replace("#", "");
                var operation = (newStr.Substring(newStr.IndexOf(match.Value)).Replace(" ","")).Split(')')[2];

                newStr = newStr.Replace(match.Value, $"valueID{ID}.length{operation}");
            }

            return newStr;
         }
    }
}

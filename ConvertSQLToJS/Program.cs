using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConvertSQLToJS
{
    class Program
    {
        public static string[] Ids;
        static void Main(string[] args)
        {
            /*CASE WHEN (not(#71149641# is null)) and (nvl(#71149481#, -1) IN (34208411)) or not((not(#71149641# is null))) THEN 1 ELSE 0 END*/
            string input = "CASE WHEN (not(#71149661# is null)) and (nvl(#71149481#, -1) IN (34208411)) or not((not(#71149661# is null))) THEN 1 ELSE 0 END";
            Ids= input.GetIDsFromString();

            string codeText = string.Empty;
            int ruleKey=0;
            string ruleName="";

            codeText += CreateImporstAndConsts(ruleKey, ruleName);
            codeText += CreateIDsInitialization(Ids);
            codeText += input.ToLower().createIfCondition();

        }
        static string CreateImporstAndConsts(int ruleKey, string ruleName)
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
            jsInput = Replacer.ReplaceCase(jsInput);
            jsInput = Replacer.ReplaceNot(jsInput);
            jsInput = Replacer.ReplaceOr(jsInput);
            jsInput = Replacer.ReplaceAnd(jsInput);
            jsInput = Replacer.ReplaceIsNull(jsInput);


            return jsInput;
        }
    }
    public static class Replacer
    {

        public static string ReplaceCase(string str)
        {
            return str.ToLower().Replace("case when", "if ");
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
        public static string ReplaceIsNull(string str)
        {

            return Regex.Replace(str.ToLower(), @"#\d+# is null", "" );
                
              
        }
    }
}

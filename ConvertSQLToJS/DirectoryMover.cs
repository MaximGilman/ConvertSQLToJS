using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertSQLToJS
{
    public static class DirMover
    {
        //заходим во все папки с номером
        //берем main.js
        //пихаем ее в папку CheckFLCLogic

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mainPath"></param>
        public static void AddDirAndMoveFile(string mainPath)
        {
            ReadAllFiles(mainPath);
        }


        public static void ReadAllFiles(string directoryName)
        {
            //для каждой подгруппы папок
            foreach (string dirName in Directory.GetDirectories(directoryName))
            {
                //для каждой папки с номером правила
                foreach (string ruleDirName in Directory.GetDirectories(dirName))
                {
                    if (long.TryParse(ruleDirName.Split('\\').Last(), out long ruleId))
                    {
                        //зайти //взять //положить
                        var path = Directory.CreateDirectory(ruleDirName + $"/CheckFLCLogic");
                        string fileName = "main.js";
                        Directory.GetFiles(ruleDirName).ToList().ForEach(f => File.Move(ruleDirName+$"\\{fileName}", path.FullName + $"\\{fileName}"));
                    }

                }

            }
        }


        public static void RenameAndMoveFile(string mainPath)
        {
            ReadnNameAllFiles(mainPath);
        }
        public static void ReadnNameAllFiles(string directoryName)
        {
            string fileName = "main.js";

            //для каждой подгруппы папок
            foreach (string dirName in Directory.GetDirectories(directoryName))
            {
                //для каждой папки с номером правила
                foreach (string ruleDirName in Directory.GetDirectories(dirName))
                {
                    
                    if (long.TryParse(ruleDirName.Split('\\').Last(), out long ruleId))
                    {
                        //зайти //взять //положить
                          
                        Directory.GetFiles(ruleDirName).ToList().ForEach(f => File.Copy(ruleDirName + $"\\{fileName}", @"C:\Users\Gilman.MM\source\repos\ConvertSQLToJS\ConvertSQLToJS\bin\Debug\JS_Projects" + $"\\{ruleDirName.Split('\\').Last()}.js"));
                    }
                    Console.WriteLine($"закончил с {ruleDirName}");
                }
                Directory.GetFiles(dirName).ToList().ForEach(f => File.Copy(dirName + $"\\{fileName}", @"C:\Users\Gilman.MM\source\repos\ConvertSQLToJS\ConvertSQLToJS\bin\Debug\JS_Projects" + $"\\{dirName.Split('\\').Last()}.js"));
                Console.WriteLine($"закончил с {dirName}");
            }
        }
    }
}

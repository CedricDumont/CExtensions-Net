using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Helper
{
    public class BaseUnitTest
    {
        public string GetFilePath(string fileNameWithExtension, string forlderName = "input")
        {
            string filePath = this.GetType().AssemblyDirectory() + "\\" + forlderName + "\\" + fileNameWithExtension;

            if (!File.Exists(filePath))
            {
                throw new Exception("The file [" + filePath + "] does not exists. Make sure that you checked 'Copy always' or 'copy Newer' on the item properties");
            }

            return filePath;
        }

        public string GetOutFileContent(string testName, string forlderName = "input")
        {
            string filePath = GetFilePath(testName + "_out.xml", forlderName);

            using (StreamReader reader = new StreamReader(filePath))
            {
                return reader.ReadToEnd();
            }
        }

        public string GetInFileContent(string testName, string forlderName = "input")
        {
            string filePath = GetFilePath(testName + "_in.xml", forlderName);

            using (StreamReader reader = new StreamReader(filePath))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

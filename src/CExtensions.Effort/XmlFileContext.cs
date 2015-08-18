using Effort;
using Effort.DataLoaders;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CExtensions.EntityFramework;
using System.Diagnostics;

namespace CExtensions.Effort
{
    public enum ContextEnum
    {
        Expected,
        Input
    }

    public class XmlFileContext<T> where T : DbContext
    {

        private string _rootPath;

        private string _assemblyName;

        public XmlFileContext(Type typeOfTest)
        {
            _rootPath = typeOfTest.AssemblyDirectory();
            if (!Directory.Exists(_rootPath))
            {
                throw new Exception("The directory with path " + _rootPath + " does not exists");
            }
        }

        public T InputContext(string testName, string folderName = "input")
        {
            return Create(ContextEnum.Input, testName, folderName);
        }

        public T ExpectedContext(string testName, string folderName = "input")
        {
            return Create(ContextEnum.Expected, testName, folderName);
        }

        private T Create(ContextEnum direction, string testName, string folderName = "input")
        {
            string suffix = direction == ContextEnum.Expected ? "_out" : "_in";

            return Create(testName + suffix, folderName);

        }

        public T Create(string testFileName = null, string folderName = "input")
        {
            IDataLoader loader;

            if (testFileName != null)
            {
                string testFullFileName = _rootPath + "\\" + folderName + "\\" + testFileName + ".xml";

                Debug.Write("FULL file Name : " + testFullFileName);

                loader = new CachingDataLoader(new XmlDataLoader(testFullFileName));
            }
            else
            {
                loader = new CsvDataLoader();
            }


            DbConnection connection = DbConnectionFactory.CreateTransient(loader);

            T instance = (T)Activator.CreateInstance(typeof(T), connection);

            if (testFileName != null)
            {
                instance.Database.CreateIfNotExists();
            }

            return instance;
        }



        public T Empty()
        {
            DbConnection connection = DbConnectionFactory.CreateTransient();

            T instance = (T)Activator.CreateInstance(typeof(T), connection);

            instance.Database.CreateIfNotExists();

            return instance;
        }
    }
}

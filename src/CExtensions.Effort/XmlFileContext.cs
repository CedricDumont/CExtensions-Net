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
using Effort.Provider;
using Effort.DataLoaders.Xml;

namespace CExtensions.Effort
{
    public enum ContextEnum
    {
        Expected,
        Input
    }

    //public class ContextFactory<T> where T : DbContext
    //{
    //    public IDictionary<string, XmlFileContext<T>>
    //}

    public class XmlFileContext<T> where T : DbContext
    {

        private string _rootPath;

        public XmlFileContext(Type typeOfTest)
        {
            _rootPath = typeOfTest.AssemblyDirectory();
            if (!Directory.Exists(_rootPath))
            {
                throw new Exception("The directory with path " + _rootPath + " does not exists");
            }
        }

        public T InputContext(string testName, string folderName = "input", bool useTransient = true)
        {
            return Create(ContextEnum.Input, testName, folderName, useTransient);
        }

        public T ExpectedContext(string testName, string folderName = "input", bool useTransient = true)
        {
            return Create(ContextEnum.Expected, testName, folderName, useTransient);
        }


        public T Create(
            ContextEnum direction = ContextEnum.Input, 
            string testName = null, 
            string folderName = "input", 
            bool useTransient = true)
        {

            string suffix = direction == ContextEnum.Expected ? "_out" : "_in";

            string testFileName = testName + suffix;

            T context = null;

            string testFullFileName = null;


            if (testFileName != null)
            {
                testFullFileName = _rootPath + "\\" + folderName + "\\" + testFileName + ".xml";
            }

            if (useTransient)
            {
                context = DbContextFactory<T>.Create(testFullFileName);

            }
            else
            {
                string connName = typeof(T).FullName + suffix;

                context = DbContextFactory<T>.Create(testFullFileName, ConnectionBehaviour.Persistent, connName);

            }

            return context;
        }

        //public T Create(ContextEnum direction = ContextEnum.Input, string testName = null, string folderName = "input", bool useTransient = true)
        //{
        //    string suffix = direction == ContextEnum.Expected ? "_out" : "_in";

        //    string testFileName = testName + suffix;

        //    IDataLoader loader;

        //    if (testFileName != null)
        //    {
        //        string testFullFileName = _rootPath + "\\" + folderName + "\\" + testFileName + ".xml";

        //        //Debug.Write("FULL file Name : " + testFullFileName);

        //        loader = new CachingDataLoader(new XmlDataLoader(testFullFileName));
        //    }
        //    else
        //    {
        //        loader = new CsvDataLoader();
        //    }

        //    if (useTransient)
        //    {
        //        currentCOnnection = (EffortConnection)DbConnectionFactory.CreateTransient(loader);
        //        instance = (T)Activator.CreateInstance(typeof(T), currentCOnnection);
        //    }
        //    else
        //    {
        //        if (direction == ContextEnum.Input)
        //        {
        //            if (currentCOnnection_input == null)
        //            {
        //                currentCOnnection_input = (EffortConnection)DbConnectionFactory.CreatePersistent(direction.ToString(), loader);
        //            }
        //            else
        //            {
        //                currentCOnnection_input.LoadData(loader);
        //            }
        //            instance = (T)Activator.CreateInstance(typeof(T), currentCOnnection_input);
        //        }
        //        else
        //        {
        //            if (currentCOnnection_output == null)
        //            {
        //                currentCOnnection_output = (EffortConnection)DbConnectionFactory.CreatePersistent(direction.ToString(), loader);
        //            }
        //            else
        //            {
        //                currentCOnnection_output.LoadData(loader);
        //            }
        //            instance = (T)Activator.CreateInstance(typeof(T), currentCOnnection_output);
        //        }
        //    }



        //    if (testFileName != null)
        //    {
        //        instance.Database.CreateIfNotExists();
        //    }

        //    //_effortDBManager = currentCOnnection.DbManager;

        //    return instance;
        //}


        public T Empty()
        {
            DbConnection connection = DbConnectionFactory.CreateTransient();

            T instance = (T)Activator.CreateInstance(typeof(T), connection);

            instance.Database.CreateIfNotExists();

            return instance;
        }
    }
}

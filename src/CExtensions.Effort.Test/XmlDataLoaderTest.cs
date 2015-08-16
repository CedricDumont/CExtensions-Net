using CExtensions.Effort.SampleApp;
using Effort.DataLoaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Effort;
using Shouldly;

namespace CExtensions.Effort.Test
{
    public class XmlDataLoaderTest
    {
        [Fact]
        public void ShouldCreateDatabaseFromXml_Transient()
        {
             // create the test file
            string fileName = this.GetType().AssemblyDirectory() + "\\input\\test1_in.xml";

            IDataLoader loader = new CachingDataLoader(new XmlDataLoader(fileName));

            using (SampleContext ctx = new SampleContext(DbConnectionFactory.CreateTransient(loader)))
            {
                ctx.Authors.ToList().Count.ShouldBe(3);
                ctx.Authors.Where(a => a.Experience > 100).Count().ShouldBe(2);
            }
        }

         [Fact]
        public void ShouldCreateDatabaseFromXml_Persistent()
        {
            // create the test file
            string fileName = this.GetType().AssemblyDirectory() + "\\input\\test1_in.xml";

            IDataLoader loader = new CachingDataLoader(new XmlDataLoader(fileName));

            using (SampleContext ctx = new SampleContext(DbConnectionFactory.CreatePersistent("myConn", loader)))
            {
                //ensure the author does not exist
                ctx.Authors.Where(a => a.FirstName == "FromTest").Count().ShouldBe(0);

                //create the new author
                Author author = new Author();
                author.FirstName = "FromTest";
                ctx.Authors.Add(author);
                ctx.SaveChanges();
            }

             //create a new context with same connection id
            using (SampleContext ctx = new SampleContext(DbConnectionFactory.CreatePersistent("myConn")))
            {
                ctx.Authors.Where(a => a.FirstName == "FromTest").Count().ShouldBe(1);
            }
        }
    }
}

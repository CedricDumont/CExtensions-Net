using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using CExtensions.Effort.SampleApp;
using CExtensions.EntityFramework;
using CExtensions.Effort;
using System.Threading.Tasks;
using System.Linq;

namespace CExtensions.EntityFramework.Test
{
    public class DBContextExtensionsTest
    {

        [Fact]
        public void ShouldReturnAllDbsSets()
        {
            using (SampleContext context = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var result = context.DbSets();

                result.Count().ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldReturnAllDbsSetForEntity()
        {
            using (SampleContext context = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var result = context.Set("Author");

                result.ElementType.Name.ShouldBe("Author");
            }
        }

        [Fact]
        public async Task ShouldCompareEmptyContext()
        {
            using (SampleContext emptyContext1 = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                using (SampleContext emptyContext2 = new XmlFileContext<SampleContext>(this.GetType()).Empty())
                {
                    var comparisonResult = await emptyContext1.CompareTo(emptyContext2);

                    comparisonResult.AreEqual.ShouldBe(true, comparisonResult.ToString());
                }

            }
        }

        [Fact]
        public void ShouldGetMappedTableAndEntityName()
        {
            using (SampleContext emptyContext1 = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var TableName = emptyContext1.MappedTable("Author");
                TableName.ShouldBe("AUTHOR");

                var EntityName = emptyContext1.MappedEntity("AUTHOR");
                EntityName.ShouldBe("Author");
            }
        }

        [Fact]
        public void ShouldGetMappedColumnAndPropertyName()
        {
            using (SampleContext emptyContext1 = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var columnName = emptyContext1.MappedColumnName("Author", "Id");
                columnName.ShouldBe("AUT_ID");

                columnName = emptyContext1.MappedColumnName("Author", "FirstName");
                columnName.ShouldBe("AUT_FIRSTNAME");

                var propertyName = emptyContext1.MappedPropertyName("AUTHOR", "AUT_FIRSTNAME");
                propertyName.ShouldBe("FirstName");
            }
        }

        [Fact]
        public void ShouldGetMappings()
        {
            using (SampleContext emptyContext = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var entityMapping = emptyContext.GetMappings<Author>();

                entityMapping.TableName.ShouldBe("AUTHOR");
                entityMapping.EntityName.ShouldBe("Author");
                entityMapping.PropertiesMapping.Count().ShouldBe(4);

                entityMapping.MappedColumn("FirstName").ShouldBe("AUT_FIRSTNAME");
                entityMapping.MappedProperty("AUT_FIRSTNAME").ShouldBe("FirstName");

                entityMapping.MappedColumn("Id").ShouldBe("AUT_ID");
                entityMapping.MappedProperty("AUT_ID").ShouldBe("Id");


                //from table name
                entityMapping = emptyContext.GetMappings("AUTHOR");

                entityMapping.TableName.ShouldBe("AUTHOR");
                entityMapping.EntityName.ShouldBe("Author");
                entityMapping.PropertiesMapping.Count().ShouldBe(4);
            }
        }
    }
}

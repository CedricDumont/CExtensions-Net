using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using CExtensions.Test.Model;
using CExtensions.EntityFramework;
using CExtensions.Effort;
using System.Threading.Tasks;
using System.Linq;
using Effort;
using Effort.DataLoaders.Xml;
using System.Data.Entity;
using System.Diagnostics;
using System.Data.Entity.Core.Objects;
using CloneExtensions;
using Newtonsoft.Json;
using CExtensions.Xml;

namespace CExtensions.EntityFramework.Test
{
    public class DBContextDumpExtensionsTest
    {
        private const String connectionName = "SampleContextConn";

        [Theory(DisplayName = "OriginalShouldBeSameAsInput")]
        [InlineData("1_one_author.xml", ContextDataEnum.Local, 1)]
        [InlineData("1_one_author.xml", ContextDataEnum.Relations, 1)]
        [InlineData("1_one_author.xml", ContextDataEnum.All, 1)]
        [InlineData("2_one_author_one_post_two_comment.xml", ContextDataEnum.Local, 2)]
        [InlineData("2_one_author_one_post_two_comment.xml", ContextDataEnum.Relations, 2)]
        [InlineData("2_one_author_one_post_two_comment.xml", ContextDataEnum.All, 2)]
        public async Task OriginalShouldBeSameAsInput(string fileName, ContextDataEnum contextData, decimal autId)
        {
            string filepath = this.GetType().AssemblyDirectory() + "\\input\\"+ fileName;
            
            string xml_result_ori = null;
            string xml_result_actual = null;
            XmlComparisonResult result = null;
            XmlComparisonUtils xmlUtils = new XmlComparisonUtils();

            using (SampleContext sampleContext = DbContextFactory.CreateFromPersistent<SampleContext>(connectionName, filepath))
            {
                using (DbContext originalTracker = OriginalDbContextTracker.Instance.AddTracker(sampleContext))
                {
                    //just load the author
                    var autor = sampleContext.Authors.FirstOrDefault(aut => aut.Id == autId);
                    autor.ShouldNotBe(null, $"author with id {autId} test {fileName}" );

                    xml_result_actual = await sampleContext.AsXmlAsync(contextData);
                    xml_result_ori = await originalTracker.AsXmlAsync(contextData);

                    result = xmlUtils.CompareXml(xml_result_actual, xml_result_ori);
                    result.AreEqual.ShouldBe(true, result.ToString());

                }

            }
       }

        public async Task OriginalShouldBeSameAsInputTEMOP(string fileName)
        {

            string filepath = this.GetType().AssemblyDirectory() + "\\input\\" + fileName;

            string xml_result_ori = null;
            string xml_result_actual = null;

            using (SampleContext sampleContext = DbContextFactory.CreateFromPersistent<SampleContext>(connectionName, filepath))
            {
                using (DbContext originalTracker = OriginalDbContextTracker.Instance.AddTracker(sampleContext))
                {
                    // contexttemp.AsObjectContext().ObjectMaterialized += DBContextDumpExtensionsTest_ObjectMaterialized;
                    //load one comment
                    Post post = sampleContext.Posts.Find((Int64)3);

                    post.Body = "Changed : " + post.Body;

                    sampleContext.SaveChanges();

                    xml_result_actual = await sampleContext.AsXmlAsync(ContextDataEnum.All);
                    xml_result_ori = await originalTracker.AsXmlAsync(ContextDataEnum.All);

                    var result = await sampleContext.CompareTo(originalTracker);

                    result.AreEqual.ShouldBe(true, result.ToString());
                }

            }
        }
    }
}

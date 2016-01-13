using System;
using Xunit;
using Shouldly;
using CExtensions.Test.Model;
using CExtensions.Effort;
using System.Threading.Tasks;
using System.Linq;
using Effort;
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
            string filepath = this.GetType().AssemblyDirectory() + "\\input\\" + fileName;

            string xml_result_ori = null;
            string xml_result_actual = null;
            XmlComparisonResult result = null;
            XmlComparisonUtils xmlUtils = new XmlComparisonUtils();

            using (SampleContext sampleContext = DbContextFactory<SampleContext>.Create(filepath, ConnectionBehaviour.Persistent, connectionName))
            {
                sampleContext.StartRecordingOriginalValues();

                //just load the author
                var autor = sampleContext.Authors.FirstOrDefault(aut => aut.Id == autId);
                autor.ShouldNotBe(null, $"author with id {autId} test {fileName}");

                xml_result_actual = await sampleContext.AsXmlAsync(contextData);
                xml_result_ori = await sampleContext.GetRecordedContext().AsXmlAsync(contextData);

                result = xmlUtils.CompareXml(xml_result_actual, xml_result_ori);
                result.AreEqual.ShouldBe(true, result.ToString());

                sampleContext.StopRecordingOriginalValues();
            }
        }

        [Theory(DisplayName = "ShouldRecordOriginalValues")]
        [InlineData("test1")]
        public async Task ShouldRecordOriginalValues(string test)
        {
            var contextFactory = new XmlFileContext<SampleContext>(this.GetType());

            using (SampleContext sampleContext = contextFactory.InputContext(test, "original-values-tests", false).StartRecordingOriginalValues())
            {
                Post post = sampleContext.Posts.Find((Int64)1);
                post.Body = "Changed : " + post.Body;
                sampleContext.SaveChanges();

                var recordedContext = sampleContext.GetRecordedContext();

                sampleContext.PauseRecordingOriginalValues();

                var result = await sampleContext.CompareTo(recordedContext);

                result.AreEqual.ShouldBe(false);
                result.Differences.Count.ShouldBe(1);
                result.Differences[0].ToString().ShouldBe("Post.Body Should be [Original Body] but was [Changed : Original Body] - object with Post_Id : 1");

                sampleContext.StopRecordingOriginalValues();

            }
        }
    }
}

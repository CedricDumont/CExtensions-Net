using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CExtensions.EntityFramework;
using System.Threading.Tasks;
using CExtensions;
using Shouldly;
using Xunit;
using CExtensions.Effort;
using CExtensions.Test.Model;

namespace CExtensions.EntityFramework
{
    public class DbContextObjectComparerTest
    {


        [Fact]
        public async Task ShouldReturnTrueForSameContextComparison()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx1 = xmlFileCtx.InputContext("test1");
            SampleContext ctx2 = xmlFileCtx.InputContext("test1");

            var result = await ctx1.CompareTo(ctx2);

            result.AreEqual.ShouldBe(true);

        }

        [Fact]
        public async Task ShouldReturnFalseForContextWithMissingEntryinExpected()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            XmlFileContext<SampleContext> xmlFileExpectedCtx = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx1 = xmlFileCtx.InputContext("test1");
            SampleContext ctx2 = xmlFileExpectedCtx.ExpectedContext("test1");

            var result = await ctx1.CompareTo(ctx2);

            result.AreEqual.ShouldBe(false);
        }

        [Fact]
        public async Task ShouldReturnFalseForContextWithSameCollectionCountButDifferentIds()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            XmlFileContext<SampleContext> xmlFileExpectedCtx = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx1 = xmlFileCtx.InputContext("test3");
            SampleContext ctx2 = xmlFileExpectedCtx.ExpectedContext("test3");

            var result = await ctx1.CompareTo(ctx2);

            result.AreEqual.ShouldBe(false);
            result.Differences.Count.ShouldBe(1);
            result.Differences[0].ToString().ShouldBe("Author was null - object with AUT_ID : 2 (we couldn't find an actual object with expected id  : 2 - this can be caused because the id is auto generated. You could adapt the ids of the expected object)");

        }

        [Fact]
        public async Task ShouldBeFalseWhenEntityStateAreDifferent()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());
            XmlFileContext<SampleContext> xmlFileExpectedCtx = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx1 = xmlFileCtx.InputContext("test4");
            SampleContext ctx2 = xmlFileExpectedCtx.ExpectedContext("test4");
            SampleContext ctx3 = xmlFileExpectedCtx.ExpectedContext("test4b");

            var auth = ctx1.Authors.Find((Int64)1);
            auth.FirstName = "cedric";
            
            var result = await ctx1.CompareTo(ctx2);
            result.AreEqual.ShouldBe(false, result.ToString());

            ctx1.SaveChanges();
            result = await ctx1.CompareTo(ctx3);
            result.AreEqual.ShouldBe(true, result.ToString());

        }


        [Fact]
        public async Task ShouldReturnFalseForContextWithEntryContainingDifferentValues()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());
            XmlFileContext<SampleContext> xmlFileExpectedCtx = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx1 = xmlFileCtx.InputContext("test2");
            SampleContext ctx2 = xmlFileExpectedCtx.ExpectedContext("test2");

            var result = await ctx1.CompareTo(ctx2);

            result.AreEqual.ShouldBe(false);
            result.Differences.Count.ShouldBe(2);

            result.Differences[0].ObjectId.ShouldBe("2");
            result.Differences[0].ObjectName.ShouldBe("Post");
            result.Differences[0].PropertyName.ShouldBe("Subject");
            result.Differences[0].ActualPropertyContent.ShouldBe("First Text");
            result.Differences[0].ExpectedPropertyContent.ShouldBe("First Text Modified");
            result.Differences[0].ToString().ShouldBe("Post.Subject Should be [First Text Modified] but was [First Text] - object with Post_Id : 2");

            result.Differences[1].ObjectId.ShouldBe("2");
            result.Differences[1].ObjectName.ShouldBe("Post");
            result.Differences[1].PropertyName.ShouldBe("Body");
            result.Differences[1].ActualPropertyContent.ShouldBe("Some Text here");
            result.Differences[1].ExpectedPropertyContent.ShouldBe("Some Other Text here");
        }
    }
}

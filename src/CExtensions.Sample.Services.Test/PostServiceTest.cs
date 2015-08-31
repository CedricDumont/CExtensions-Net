using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using System.Threading.Tasks;
using CExtensions.EntityFramework;
using CExtensions.Effort;
using System.Linq;

namespace CExtensions.Sample.Services.Test
{
    public class PostServiceTest
    {

        [Fact(DisplayName="Should return all posts")]
        public void Should_Return_All_Posts()
        {
            using (SampleContext ctx = GetInputContext("test1"))
            {
                using (PostService svc = new PostService(ctx))
                {
                    var allPosts = svc.GetAllPost();

                    allPosts.Count().ShouldBe(3);
                }
            }
        }

        [Fact(DisplayName="Should Delete Post")]
        public async Task Should_Delete_Post()
        {
            using (SampleContext ctx = GetInputContext("test1"))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.DeletPostWithId(1);

                    var result  = await ctx.CompareTo(GetExpectedContext("test1"));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        #region Helper methods
        private SampleContext GetInputContext(string testname)
        {
            SampleContext ctx = new XmlFileContext<SampleContext>(this.GetType()).InputContext(testname);

            return ctx;
        }

        private SampleContext GetExpectedContext(string testname)
        {
            SampleContext ctx = new XmlFileContext<SampleContext>(this.GetType()).ExpectedContext(testname);

            return ctx;
        }
        #endregion
    }
}

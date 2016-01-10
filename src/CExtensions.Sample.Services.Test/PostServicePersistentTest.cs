using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CExtensions.Sample.Dal;
using CExtensions.Effort;
using System.Threading.Tasks;
using Xunit;
using CExtensions.EntityFramework;
using Shouldly;

namespace CExtensions.Sample.Services.Test
{
    public class PostServicePersistentTest
    {

        [Theory(DisplayName = "Creating Post Using persistent configuration")]
        [InlineData("test01", 1, "sampleSubject", "sampleBody")]
        [InlineData("test02", 2, "sampleSubject2", "sampleBody2")]
        [InlineData("test03", 2, "sampleSubject2", "sampleBody2")]
        [InlineData("test04", 2, "sampleSubject2", "sampleBody2")]
        public async Task Should_CreateNewPost(string test, Int64 actorId, string postsubject, string postBody)
        {
            using (SampleContext ctx = GetInputContext(test))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.CreateNewPost(actorId, postsubject, postBody);

                    var result = await ctx.CompareTo(GetExpectedContext(test));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        [Theory(DisplayName = "Delete Post Using persistent configuration")]
        [InlineData("test20", 1)]
        [InlineData("test21", 3)]
        public async Task Should_DeletePost(string test, Int64 postId)
        {
            using (SampleContext ctx = GetInputContext(test))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.DeletPostWithId(postId);

                    var result = await ctx.CompareTo(GetExpectedContext(test));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        [Fact]
        public async Task Should_CreateTwoPostAndDeleteOne()
        {
            string test = "test31";

            using (SampleContext ctx = GetInputContext(test))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.CreateNewPost(3, "NewPost Created1", "NewPost Created1");
                    svc.DeletPostWithId(3);
                    svc.CreateNewPost(2, "NewPost Created2", "NewPost Created2");

                    var result = await ctx.CompareTo(GetExpectedContext(test));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        [Fact]
        public async Task Should_DeleteAnAuthor()
        {
            string test = "test32";

            using (SampleContext ctx = GetInputContext(test))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.DeleteAuthor(3);

                    var result = await ctx.CompareTo(GetExpectedContext(test));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }


        [Theory(DisplayName = "Delete Author by Id Using persistent configuration")]
        [InlineData("test41", 24)]
        [InlineData("test42", 2)]
        public async Task ShouldDeleteAuthor(string test, Int64 autId)
        {
            using (SampleContext ctx = GetInputContext(test))
            {
                using (PostService svc = new PostService(ctx))
                {
                    svc.DeleteAuthor(autId);

                    var result = await ctx.CompareTo(GetExpectedContext(test));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        private SampleContext GetInputContext(string testname)
        {
            XmlFileContext<SampleContext> factory = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx = factory.InputContext(testname, "persistent_tests_input", false);

            return ctx;
        }

        private SampleContext GetExpectedContext(string testname)
        {
            XmlFileContext<SampleContext> factory = new XmlFileContext<SampleContext>(this.GetType());

            SampleContext ctx = factory.ExpectedContext(testname, "persistent_tests_input", false);

            return ctx;
        }
    }
}

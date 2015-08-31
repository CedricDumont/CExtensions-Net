using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using System.Threading.Tasks;
using CExtensions.EntityFramework;

namespace CExtensions.Effort.SampleApp.Test
{
    public class ServiceTest
    {

        [Fact(DisplayName="Should return all posts")]
        public void Should_Return_All_Posts()
        {
            using (SampleContext ctx = GetInputContext("test1"))
            {
                using (Service svc = new Service(ctx))
                {
                    var allPosts = svc.GetAllPost();

                    allPosts.Count.ShouldBe(3);
                }
            }
        }

        [Fact(DisplayName="Should Delete Post")]
        public async Task Should_Delete_Post()
        {
            using (SampleContext ctx = GetInputContext("test1"))
            {
                using (Service svc = new Service(ctx))
                {
                    svc.DeletPostWithId(1);

                    var result  = await ctx.CompareTo(GetExpectedContext("test1"));

                    result.AreEqual.ShouldBe(true, result.ToString());
                }
            }
        }

        [Fact(DisplayName="Should Add Post an Author")]
        public async Task Should_Add_Post_And_Author()
        {
            using (SampleContext ctx = GetInputContext("test2"))
            {
                using (Service svc = new Service(ctx))
                {
                    Post p = new Post();
                    Author a = new Author() { FirstName = "test" };
                    p.Author = a;
                    ctx.Posts.Add(p);
                    
                    ctx.SaveChanges();

                    var result = await ctx.CompareTo(GetExpectedContext("test2"));


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

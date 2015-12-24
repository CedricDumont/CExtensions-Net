using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using System.Threading.Tasks;
using CExtensions.EntityFramework;
using CExtensions.Effort;
using System.Linq;
using System.Diagnostics;
using CExtensions.Sample.Dal;
using CExtensions.SqlCE;

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

        [Theory(DisplayName ="Creating Post")]
        [InlineData("test2", 1, "sampleSubject", "sampleBody")]
        [InlineData("test3", 2, "sampleSubject2", "sampleBody2")]
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

       [Fact]
        public void BENCHMARK()
        {
            int ITERATION_COUNT = 1;
         

            Stopwatch transientWatch = new Stopwatch();
            Stopwatch doWorkTransient = new Stopwatch();
            Stopwatch doWorkNonTransient = new Stopwatch();
            Stopwatch doWorkSqlCe = new Stopwatch();
            useTransient = true;
            transientWatch.Start();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                SampleContext inputContext = GetInputContext("test2");
                SampleContext outputContext = GetExpectedContext("test2");
                var task = DoWork(inputContext, outputContext, doWorkTransient);
                task.Wait();
            }
            transientWatch.Stop();

            string test_in = this.GetType().AssemblyDirectory() + "\\input\\test2_in.xml";
            string test_out = this.GetType().AssemblyDirectory() + "\\input\\test2_out.xml";

            SqlCEExtensions.ResteWatch();
            Stopwatch sqlCeWatch = new Stopwatch();
            sqlCeWatch.Start();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                SampleContext inputContext = test_in.ToDbContext<SampleContext>("in");
                SampleContext outputContext = test_out.ToDbContext<SampleContext>("out");
                var task = DoWork(inputContext, outputContext, doWorkSqlCe);
                task.Wait();
            }
            sqlCeWatch.Stop();
            SqlCEExtensions.StopAllWatch();

            XmlFileContext<SampleContext>.currentCOnnection = null;
            useTransient = false;
            Stopwatch nontransientWatch = new Stopwatch();
            nontransientWatch.Start();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                SampleContext inputContext = GetInputContext("test2");
                SampleContext outputContext = GetExpectedContext("test2");
                var task = DoWork(inputContext, outputContext, doWorkNonTransient);
                task.Wait();
            }
            nontransientWatch.Stop();


            Debug.WriteLine("total inner time = " + SqlCEExtensions.overAllWatch.ElapsedMilliseconds);
            Debug.WriteLine("insert time = " + SqlCEExtensions.insertWatch.ElapsedMilliseconds);
            Debug.WriteLine("delete time = " + SqlCEExtensions.deleteWatch.ElapsedMilliseconds);
            Debug.WriteLine("ds time = " + SqlCEExtensions.dsWathc.ElapsedMilliseconds);
            Debug.WriteLine("activator time = " + SqlCEExtensions.ActivatorWatch.ElapsedMilliseconds);
            Debug.WriteLine("createDb time = " + SqlCEExtensions.DbCreationWatch.ElapsedMilliseconds);

            Debug.WriteLine("totalTime sqlce dowork = " + doWorkSqlCe.ElapsedMilliseconds);
            Debug.WriteLine("totalTime transient dowork = " + doWorkTransient.ElapsedMilliseconds);
            Debug.WriteLine("totalTime non transient  dowork= " + doWorkNonTransient.ElapsedMilliseconds);

            Debug.WriteLine("totalTime sqlce time = " + sqlCeWatch.ElapsedMilliseconds);
            Debug.WriteLine("totalTime transient = " + transientWatch.ElapsedMilliseconds);
            Debug.WriteLine("totalTime non transient = " + nontransientWatch.ElapsedMilliseconds);
        }

        private static async Task DoWork(SampleContext inputContext, SampleContext outputContext, Stopwatch doworkWatch)
        {
            doworkWatch.Start();
            using (inputContext)
            {
                using (outputContext)
                {
                    using (PostService svc = new PostService(inputContext))
                    {
                        svc.CreateNewPost(1, "sampleSubject", "sampleBody");
                        //svc.DeletPostWithId(2);
                        //svc.CreateNewPost(1, "sampleSubject", "sampleBody");

                        var allPosts = svc.GetAllPost().ToList();

                        allPosts.Count.ShouldBe(2);

                        var result = await inputContext.CompareTo(outputContext);

                        result.AreEqual.ShouldBe(true, result.ToString());
                    }
                }
            }
            doworkWatch.Stop();
        }

        XmlFileContext<SampleContext> _ctxFactory = null;


        private XmlFileContext<SampleContext> CtxFactory
        {
            get
            {
                if(_ctxFactory == null)
                {
                    _ctxFactory = new XmlFileContext<SampleContext>(this.GetType());
                }

                return _ctxFactory;
            }
        }

        bool useTransient = true;
        #region Helper methods
        private SampleContext GetInputContext(string testname)
        {
            SampleContext ctx = CtxFactory.InputContext(testname, "input", useTransient);

            return ctx;
        }

        private SampleContext GetExpectedContext(string testname)
        {
            SampleContext ctx = CtxFactory.ExpectedContext(testname, "input", useTransient);

            return ctx;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CExtensions.Sample.Model;
using AutoMapper;
using CExtensions.Sample.Dal;
using CExtensions.Sample.Dto;

namespace CExtensions.Sample.Services
{
    public class PostService : BaseService,  IDisposable
    {
        private SampleContext myContext;

        public PostService(SampleContext ctx):base()
        {
            myContext = ctx;
        }

        public void Dispose()
        {
            myContext.Dispose();
        }

        public IEnumerable<PostDto> GetAllPost()
        {
            IList<Post> result =  myContext.Posts.ToList();

            return Mapper.Map<IList<PostDto>>(result);
        }


        public void DeletPostWithId(Int64 id)
        {
            var post = myContext.Posts.Find(id);

            if(post == null)
            {
                throw new Exception("Post was not found");
            }

            myContext.Posts.Remove(post);
            myContext.SaveChanges();
        }

        public void CreateNewPost(Int64 autid, String postSubject, String postBody)
        {
            var post = myContext.Posts.Create<Post>();
            post.AutId = autid;
            post.Subject = postSubject;
            post.Body = postBody;
            myContext.Posts.Add(post);
            myContext.SaveChanges();
        }
    }

}

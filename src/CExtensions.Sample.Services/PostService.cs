using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CExtensions.Sample.Model;
using AutoMapper;
using CExtensions.Sample.Services.Dto;

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


        public void DeletPostWithId(decimal id)
        {
            var post = myContext.Posts.Find(id);

            if(post == null)
            {
                throw new Exception("Post was not found");
            }

            myContext.Posts.Remove(post);
            myContext.SaveChanges();
        }
    }

}

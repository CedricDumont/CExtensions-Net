using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Effort.SampleApp
{
    public class Service : IDisposable
    {
        private SampleContext myContext;

        public Service(SampleContext ctx)
        {
            myContext = ctx;
        }

        public void Dispose()
        {
            myContext.Dispose();
        }

        public List<Post> GetAllPost()
        {
            return myContext.Posts.ToList();
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

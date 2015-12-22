using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Sample.Dto
{
    public class PostDto
    {
        public decimal Id { get; set; } // Post_Id (Primary key)
        public string Subject { get; set; } // Subject
        public string Body { get; set; } // Body
        public DateTime? DateCreated { get; set; } // DateCreated
        public DateTime? DateModified { get; set; } // DateModified
    }
}

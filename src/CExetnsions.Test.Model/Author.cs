using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Test.Model
{
    public partial class Author
    {
        public Int64 Id { get; internal set; } // AUT_ID (Primary key)
        public string FirstName { get; set; } // AUT_FIRSTNAME
        public string LastName { get; set; } // AUT_LASTNAME
        public decimal? Experience { get; set; } // AUT_EXPERIENCE

        // Reverse navigation
        public virtual ICollection<Post> Posts { get; set; } // POST.FK_POST_AUTHOR

        public virtual ICollection<Comment> Comments { get; set; } // POST.FK_COMMENT_AUTHOR

        public Author()
        {
            Posts = new List<Post>();

            Comments = new List<Comment>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Test.Model
{
    public class Comment
    {
        public Int64 Id { get; internal set; } // COM_ID (Primary key)

        public String Body { get; set; } //COM_Body

        public virtual Author Author { get; set; } // FK_COMMENT_AUTHOR

        public virtual Post Post { get; set; } // FK_COMMENT_POST

        public Int64? AutId { get; internal set; }

        public Int64 PostId { get; internal set; }
    }
}

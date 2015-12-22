using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CExtensions.Sample.Dto
{
    public class AuthorDto
    {
        public decimal Id { get; internal set; } // AUT_ID (Primary key)
        public string FirstName { get; set; } // AUT_FIRSTNAME
        public string LastName { get; set; } // AUT_LASTNAME
        public decimal? Experience { get; set; } // AUT_EXPERIENCE
    }
}

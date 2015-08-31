using System;
namespace CExtensions.Sample.Interfaces
{
    public interface IPost
    {
        IAuthor Author { get; set; }
        string Body { get; set; }
        DateTime? DateCreated { get; set; }
        DateTime? DateModified { get; set; }
        decimal Id { get; set; }
        string Subject { get; set; }
    }
}

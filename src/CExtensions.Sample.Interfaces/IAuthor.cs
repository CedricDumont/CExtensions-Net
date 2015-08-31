using System;
namespace CExtensions.Sample.Interfaces
{
    public interface IAuthor
    {
        decimal? Experience { get; set; }
        string FirstName { get; set; }
        decimal Id { get; }
        string LastName { get; set; }
    }
}

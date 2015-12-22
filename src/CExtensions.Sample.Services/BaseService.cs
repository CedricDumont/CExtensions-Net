using AutoMapper;
using CExtensions.Sample.Model;
using CExtensions.Sample.Dto;

namespace CExtensions.Sample.Services
{
    public abstract class BaseService
    {
        public BaseService()
        {
            //configure the mapper
            Mapper.CreateMap<Post, PostDto>();
            Mapper.CreateMap<Author, AuthorDto>();
        }
    }
}

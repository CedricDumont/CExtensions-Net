using AutoMapper;
using CExtensions.Sample.Model;
using CExtensions.Sample.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

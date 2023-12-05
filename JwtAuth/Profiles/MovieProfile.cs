using AutoMapper;
using JwtAuth.Data.Dtos;
using JwtAuth.Models;

namespace JwtAuth.Profiles
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<CreateMovieDto, Movie>();
            CreateMap<UpdateMovieDto, Movie>();
            CreateMap<Movie, UpdateMovieDto>();
            CreateMap<Movie, ReadMovieDto>();
        }
    }
}

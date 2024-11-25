using AutoMapper;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Profilers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterModel, RegisterDto>().ReverseMap();
            CreateMap<LoginModel, LoginDto>().ReverseMap();
            CreateMap<ApplicationUser,UserDto>().ReverseMap();
            CreateMap<FavoriteArticleDto,UserFavoriteArticle>().ReverseMap();
            //CreateMap<Comment,AddCommentDto>().ReverseMap();
            //CreateMap<UpdateCommentDto, Comment>()
            //    .ForMember(dest => dest.Content , opt => opt.MapFrom(src => src.Content))
            //    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)) 
            //    .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
            //    .ForMember(dest => dest.User, opt => opt.Ignore())  
            //    .ForMember(dest => dest.ArticleId, opt => opt.Ignore());
        }
    }
}

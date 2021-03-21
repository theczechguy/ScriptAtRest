using AutoMapper;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Models.Users;
using ScriptAtRestServer.Models.Scripts;

namespace WebApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<ScriptModel, Script>();
            CreateMap<Script, ScriptModel>();
        }
    }
}
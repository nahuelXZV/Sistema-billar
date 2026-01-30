using AutoMapper;
using Domain.DTOs.Inventory;
using Domain.DTOs.Security;
using Domain.DTOs.Security.Request;
using Domain.Entities.Inventory;
using Domain.Entities.Security;

namespace Application.Mapping;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        #region Entity To DTO
        CreateMap<Usuario, RequestRegisterDTO>();
        CreateMap<Usuario, UsuarioDTO>()
         .ForMember(dest => dest.Password, opt => opt.Ignore());

        CreateMap<Perfil, PerfilDTO>();
        CreateMap<PerfilAcceso, PerfilAccesoDTO>();
        CreateMap<Acceso, AccesoDTO>();
        CreateMap<Modulo, ModuloDTO>();

        CreateMap<Almacen, AlmacenDTO>();

        #endregion

        #region  DTO To Entity
        CreateMap<RequestRegisterDTO, Usuario>();
        CreateMap<UsuarioDTO, Usuario>();
        CreateMap<PerfilDTO, Perfil>();
        CreateMap<PerfilAccesoDTO, PerfilAcceso>();
        CreateMap<AccesoDTO, Acceso>();
        CreateMap<ModuloDTO, Modulo>();
        CreateMap<AlmacenDTO, Almacen>();
        #endregion

    }
}

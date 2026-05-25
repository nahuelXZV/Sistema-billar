using System.Net.Http.Headers;
using AutoMapper;
using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.DTOs.Security;
using Domain.DTOs.Security.Request;
using Domain.Entities.Configuration;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
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
        CreateMap<Categoria, CategoriaDTO>();
        CreateMap<UnidadMedida, UnidadMedidaDTO>();
        CreateMap<Producto, ProductoDTO>();
        CreateMap<ProductoCompuesto, ProductoCompuestoDTO>();
        CreateMap<Inventario, InventarioDTO>();
        CreateMap<Lote, LoteDTO>();
        CreateMap<TransaccionInventario, TransaccionInventarioDTO>();
        CreateMap<TransaccionInventarioDetalle, TransaccionInventarioDetalleDTO>();
        CreateMap<ListaPrecios, ListaPrecioDTO>();
        CreateMap<ListaPreciosDetalle, ListaPrecioDetalleDTO>();

        CreateMap<Mesa, MesaDTO>();
        CreateMap<TipoMesa, TipoMesaDTO>();
        CreateMap<Vendedor, VendedorDTO>();


        #endregion

        #region  DTO To Entity
        CreateMap<RequestRegisterDTO, Usuario>();
        CreateMap<UsuarioDTO, Usuario>();
        CreateMap<PerfilDTO, Perfil>();
        CreateMap<PerfilAccesoDTO, PerfilAcceso>();
        CreateMap<AccesoDTO, Acceso>();
        CreateMap<ModuloDTO, Modulo>();
        CreateMap<AlmacenDTO, Almacen>();
        CreateMap<CategoriaDTO, Categoria>();
        CreateMap<UnidadMedidaDTO, UnidadMedida>();
        CreateMap<ProductoCompuestoDTO, ProductoCompuesto>();
        CreateMap<ProductoDTO, Producto>();
        CreateMap<InventarioDTO, Inventario>();
        CreateMap<LoteDTO, Lote>();
        CreateMap<TransaccionInventarioDTO, TransaccionInventario>();
        CreateMap<TransaccionInventarioDetalleDTO, TransaccionInventarioDetalle>();
        CreateMap<ListaPrecioDTO, ListaPrecios>();
        CreateMap<ListaPrecioDetalleDTO, ListaPreciosDetalle>();

        CreateMap<MesaDTO, Mesa>();
        CreateMap<TipoMesaDTO, TipoMesa>();
        CreateMap<VendedorDTO, Vendedor>();
        #endregion

    }
}

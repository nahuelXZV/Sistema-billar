using AutoMapper;
using Domain.DTOs.Contact;
using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.DTOs.Security;
using Domain.DTOs.Security.Request;
using Domain.Entities.Contact;
using Domain.Entities.Configuration;
using Domain.Entities.Inventory;
using Domain.Entities.Security;
using Domain.Entities.Sales;

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

        CreateMap<Cliente, ClienteDTO>();
        CreateMap<Mesa, MesaDTO>();
        CreateMap<TipoMesa, TipoMesaDTO>();
        CreateMap<Vendedor, VendedorDTO>()
            .ForMember(dest => dest.UsuarioDTO, opt => opt.MapFrom(src => src.Usuario))
            .ForMember(dest => dest.ListaPrecioDTO, opt => opt.MapFrom(src => src.ListaPrecio));
        CreateMap<VendedorAlmacenes, VendedorAlmacenDTO>();
        CreateMap<OrdenVenta, OrdenVentaDTO>();
        CreateMap<OrdenVentaDetalle, OrdenVentaDetalleDTO>();
        CreateMap<Venta, VentaDTO>();
        CreateMap<VentaDetalle, VentaDetalleDTO>();
        CreateMap<PagoVenta, PagoVentaDTO>()
            .ForMember(dest => dest.Venta, opt => opt.Ignore());
        CreateMap<UsoMesa, UsoMesaDTO>();
        CreateMap<MetodoPago, MetodoPagoDTO>();


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

        CreateMap<ClienteDTO, Cliente>();
        CreateMap<MesaDTO, Mesa>();
        CreateMap<TipoMesaDTO, TipoMesa>();
        CreateMap<VendedorDTO, Vendedor>()
            .ForMember(dest => dest.Usuario, opt => opt.Ignore())
            .ForMember(dest => dest.ListaPrecio, opt => opt.Ignore())
            .ForMember(dest => dest.ListaAlmacenes, opt => opt.Ignore());
        CreateMap<VendedorAlmacenDTO, VendedorAlmacenes>();
        CreateMap<OrdenVentaDTO, OrdenVenta>()
            .ForMember(dest => dest.ListaDetalles, opt => opt.Ignore())
            .ForMember(dest => dest.ListaUsoMesas, opt => opt.Ignore());
        CreateMap<OrdenVentaDetalleDTO, OrdenVentaDetalle>();
        CreateMap<VentaDTO, Venta>()
            .ForMember(dest => dest.IdempotencyKey, opt => opt.Ignore())
            .ForMember(dest => dest.OrdenVenta, opt => opt.Ignore())
            .ForMember(dest => dest.Cliente, opt => opt.Ignore())
            .ForMember(dest => dest.Vendedor, opt => opt.Ignore())
            .ForMember(dest => dest.ListaDetalles, opt => opt.Ignore())
            .ForMember(dest => dest.ListaPagos, opt => opt.Ignore());
        CreateMap<VentaDetalleDTO, VentaDetalle>()
            .ForMember(dest => dest.Venta, opt => opt.Ignore())
            .ForMember(dest => dest.OrdenVentaDetalle, opt => opt.Ignore())
            .ForMember(dest => dest.Producto, opt => opt.Ignore());
        CreateMap<PagoVentaDTO, PagoVenta>()
            .ForMember(dest => dest.Venta, opt => opt.Ignore())
            .ForMember(dest => dest.MetodoPago, opt => opt.Ignore());
        CreateMap<UsoMesaDTO, UsoMesa>();
        CreateMap<MetodoPagoDTO, MetodoPago>();
        #endregion

    }
}

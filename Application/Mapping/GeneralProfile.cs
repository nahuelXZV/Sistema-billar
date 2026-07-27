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
        CreateMap<ProductoConversion, ProductoConversionDTO>();
        CreateMap<ProductoCompuesto, ProductoCompuestoDTO>();
        CreateMap<Inventario, InventarioDTO>();
        CreateMap<Lote, LoteDTO>();
        CreateMap<TransaccionInventario, TransaccionInventarioDTO>();
        CreateMap<TransaccionInventarioDetalle, TransaccionInventarioDetalleDTO>();
        CreateMap<TraspasoInventario, TraspasoInventarioDTO>();
        CreateMap<TraspasoInventarioDetalle, TraspasoInventarioDetalleDTO>();
        CreateMap<ListaPrecios, ListaPrecioDTO>();
        CreateMap<ListaPreciosDetalle, ListaPrecioDetalleDTO>()
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src =>
                src.ProductoConversion != null && src.ProductoConversion.Producto != null
                    ? src.ProductoConversion.Producto.Nombre
                    : string.Empty))
            .ForMember(dest => dest.NombreUnidadMedida, opt => opt.MapFrom(src =>
                src.ProductoConversion != null && src.ProductoConversion.UnidadMedida != null
                    ? src.ProductoConversion.UnidadMedida.Nombre
                    : string.Empty))
            .ForMember(dest => dest.AbreviaturaUnidadMedida, opt => opt.MapFrom(src =>
                src.ProductoConversion != null && src.ProductoConversion.UnidadMedida != null
                    ? src.ProductoConversion.UnidadMedida.Abreviatura
                    : string.Empty))
            .ForMember(dest => dest.FactorConversion, opt => opt.MapFrom(src =>
                src.ProductoConversion != null
                    ? src.ProductoConversion.FactorConversion
                    : 0));

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
        CreateMap<TurnoCaja, TurnoCajaDTO>();
        CreateMap<TurnoCajaDetalle, TurnoCajaDetalleDTO>();


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
        CreateMap<ProductoConversionDTO, ProductoConversion>()
            .ForMember(dest => dest.Producto, opt => opt.Ignore())
            .ForMember(dest => dest.UnidadMedida, opt => opt.Ignore());
        CreateMap<ProductoCompuestoDTO, ProductoCompuesto>();
        CreateMap<ProductoDTO, Producto>();
        CreateMap<InventarioDTO, Inventario>();
        CreateMap<LoteDTO, Lote>();
        CreateMap<TransaccionInventarioDTO, TransaccionInventario>();
        CreateMap<TransaccionInventarioDetalleDTO, TransaccionInventarioDetalle>();
        CreateMap<TraspasoInventarioDTO, TraspasoInventario>()
            .ForMember(dest => dest.AlmacenOrigen, opt => opt.Ignore())
            .ForMember(dest => dest.AlmacenDestino, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore())
            .ForMember(dest => dest.Detalles, opt => opt.Ignore());
        CreateMap<TraspasoInventarioDetalleDTO, TraspasoInventarioDetalle>()
            .ForMember(dest => dest.TraspasoInventario, opt => opt.Ignore())
            .ForMember(dest => dest.Producto, opt => opt.Ignore())
            .ForMember(dest => dest.Lote, opt => opt.Ignore());
        CreateMap<ListaPrecioDTO, ListaPrecios>()
            .ForMember(dest => dest.ListaDetalles, opt => opt.Ignore());
        CreateMap<ListaPrecioDetalleDTO, ListaPreciosDetalle>()
            .ForMember(dest => dest.ProductoConversion, opt => opt.Ignore());

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
            .ForMember(dest => dest.IdTurnoCaja, opt => opt.Ignore())
            .ForMember(dest => dest.OrdenVenta, opt => opt.Ignore())
            .ForMember(dest => dest.TurnoCaja, opt => opt.Ignore())
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
        CreateMap<TurnoCajaDTO, TurnoCaja>()
            .ForMember(dest => dest.Vendedor, opt => opt.Ignore())
            .ForMember(dest => dest.Detalles, opt => opt.Ignore());
        CreateMap<TurnoCajaDetalleDTO, TurnoCajaDetalle>()
            .ForMember(dest => dest.TurnoCaja, opt => opt.Ignore())
            .ForMember(dest => dest.MetodoPago, opt => opt.Ignore());
        #endregion

    }
}

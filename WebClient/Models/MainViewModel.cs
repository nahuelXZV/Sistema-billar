using Domain.DTOs.Security;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using WebClient.Common;
using WebClient.Configs;
using WebClient.Extensions;

namespace WebClient.Models;

public interface IMainViewModel
{
    void Initialize(HttpContext context, AdminConfig adminConfig);
}

public class ItemMenu
{
    public MenuItemId Id { get; set; }
    public string Text { get; set; }
    public string IconCss { get; set; }
    public string Url { get; set; }
    public bool Separator { get; set; }
    public object Data { get; set; }
    public MenuItemActionType ActionType { get; set; } = MenuItemActionType.GET;
    public bool Confirm { get; set; }
    public string ConfirmMessage { get; set; }
}

public class MessageModel
{
    public MessageType Type { get; set; }
    public string Message { get; set; }
}

public class MainViewModel : IMainViewModel
{
    public bool SesionUsuarioValida { get; private set; }
    public long IdUsuarioLoggedIn { get; set; }
    public long IdPerfil { get; set; }
    public bool EsSuperAdministrador { get; set; }
    public string NombreUsuarioLoggedIn { get; set; }
    public string ApellidoUsuarioLoggedIn { get; set; }
    public string NombreCompletoUsuarioLoggedIn { get; set; }
    public string FotoUsuarioLoggedIn { get; set; }
    public string CorreoLoggedIn { get; set; }
    public AdminConfig Configuraciones { get; set; }
    public List<ModuloDTO> ModulosMenu { get; set; }
    public bool IncluirBlazorComponents { get; set; } = false;
    public List<MessageModel> Messages { get; set; }
    public List<ItemMenu> ListaItemsMenu { get; set; } = new();

    public MainViewModel() { }

    public MainViewModel(HttpContext context)
    {
        CargarTempMessages(context);
        SesionUsuarioValida = CargarDatosUsuarioLoggedIn(context.User);
        CargarAccesos(context);
    }

    public virtual void Initialize(HttpContext context, AdminConfig adminConfig)
    {
        Configuraciones = adminConfig;
        CargarTempMessages(context);
        SesionUsuarioValida = CargarDatosUsuarioLoggedIn(context.User);
        CargarAccesos(context);
    }

    private void CargarTempMessages(HttpContext context)
    {
        var factory = context?.RequestServices?.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = factory?.GetTempData(context);

        Messages = tempData.Get<List<MessageModel>>("Messages") ?? new List<MessageModel>();
    }

    private bool CargarDatosUsuarioLoggedIn(ClaimsPrincipal userClaims)
    {
        LimpiarDatosUsuarioLoggedIn();

        if (userClaims?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (!long.TryParse(userClaims.GetClaimValue(Constantes.ClaimTypes.UsuarioId), out var idUsuario))
        {
            return false;
        }

        if (!long.TryParse(userClaims.GetClaimValue(Constantes.ClaimTypes.PerfilId), out var idPerfil))
        {
            return false;
        }

        IdUsuarioLoggedIn = idUsuario;
        IdPerfil = idPerfil;
        EsSuperAdministrador = bool.TryParse(
            userClaims.GetClaimValue(Constantes.ClaimTypes.EsSuperAdmin),
            out var esSuperAdministrador) && esSuperAdministrador;
        NombreUsuarioLoggedIn = userClaims.GetClaimValue(Constantes.ClaimTypes.NombreUsuario);
        ApellidoUsuarioLoggedIn = userClaims.GetClaimValue(Constantes.ClaimTypes.ApellidoUsuario);
        NombreCompletoUsuarioLoggedIn = userClaims.GetClaimValue(Constantes.ClaimTypes.NombreCompleto);
        CorreoLoggedIn = userClaims.GetClaimValue(Constantes.ClaimTypes.Correo);
        FotoUsuarioLoggedIn = "";
        return true;
    }

    private void LimpiarDatosUsuarioLoggedIn()
    {
        IdUsuarioLoggedIn = 0;
        IdPerfil = 0;
        EsSuperAdministrador = false;
        NombreUsuarioLoggedIn = string.Empty;
        ApellidoUsuarioLoggedIn = string.Empty;
        NombreCompletoUsuarioLoggedIn = string.Empty;
        CorreoLoggedIn = string.Empty;
        FotoUsuarioLoggedIn = string.Empty;
    }

    private void CargarAccesos(HttpContext context)
    {
        List<PerfilAccesoDTO> listaAccesos = context.Session.Get<List<PerfilAccesoDTO>>(Constantes.ClaimTypes.ListaAccesos) ?? new List<PerfilAccesoDTO>();
        var accesos = listaAccesos.Select(x => x.Acceso).ToList();

        ModulosMenu = accesos
            .GroupBy(a => a.Modulo.Id)
            .Select(g => new ModuloDTO
            {
                Id = g.First().Modulo.Id,
                Nombre = g.First().Modulo.Nombre,
                Icono = g.First().Modulo.Icono,
                Secuencia = g.First().Modulo.Secuencia,
                ListaAccesos = g
                    .OrderBy(a => a.Secuencia)
                    .Select(a => new AccesoDTO
                    {
                        Id = a.Id,
                        Nombre = a.Nombre,
                        Secuencia = a.Secuencia,
                        Controlador = a.Controlador,
                        Vista = a.Vista,
                        Url = a.Url,
                        Icono = a.Icono,
                        Descripcion = a.Descripcion,
                        ModuloId = a.ModuloId
                    }).ToList()
            })
            .OrderBy(m => m.Secuencia)
            .ToList();
    }
}

using System.Text;
using System.Net;
using Domain.Constants;
using Domain.Extensions;
using Domain.Common;
using Domain.DTOs.Shared;
using Domain.Interfaces.Shared;
using WebClient.Exceptions;

namespace WebClient.Services.Implementacion;

public class AppBaseServices
{
    protected readonly string _serviceBaseUrl;
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;

    public AppBaseServices(
        string serviceBaseUrl,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger logger)
    {

        _logger = logger;
        _serviceBaseUrl = serviceBaseUrl;
        _httpClient = httpClientFactory.CreateClient(Constantes.HttpClientNames.ApiRest);
    }

    protected string AplicarFiltro(FilterDTO? filtro)
    {
        if (filtro == null) return "";

        var queryParams = new List<string>();

        if (filtro.Limit > 0)
            queryParams.Add($"limit={filtro.Limit}");

        if (filtro.Offset > 0)
            queryParams.Add($"offset={filtro.Offset}");

        if (!string.IsNullOrWhiteSpace(filtro.Search))
            queryParams.Add($"search={Uri.EscapeDataString(filtro.Search)}");

        if (queryParams.Count == 1) return "?" + queryParams.First();

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }

    protected async Task<TResult> GetAsync<TResult>(string uri = "") where TResult : new()
    {
        var response = await _httpClient.GetAsync($"{_serviceBaseUrl}/{uri}");

        var cinemaResponse = await ProcessResponse<Response<TResult>>(response);
        ValidateResponse(cinemaResponse);
        return cinemaResponse.Data;
    }

    protected async Task<TResult> PostAsync<TResult>(object content = default!) where TResult : new()
    {
        return await PostAsync<TResult>("", content);
    }

    protected async Task<TResult> PostAsync<TResult>(string uri = "", object content = default!) where TResult : new()
    {
        var body = new StringContent(content.Serialize(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_serviceBaseUrl}/{uri}", body);

        var cinemaResponse = await ProcessResponse<Response<TResult>>(response);
        ValidateResponse(cinemaResponse);
        return cinemaResponse.Data;
    }

    protected async Task<TResult> PostMultipartAsync<TResult>(string uri, MultipartFormDataContent content) where TResult : new()
    {
        var response = await _httpClient.PostAsync($"{_serviceBaseUrl}/{uri}", content);

        var cinemaResponse = await ProcessResponse<Response<TResult>>(response);
        ValidateResponse(cinemaResponse);
        return cinemaResponse.Data;
    }

    protected async Task<TResult> PutAsync<TResult>(object content = default!) where TResult : new()
    {
        return await PutAsync<TResult>("", content);
    }

    protected async Task<TResult> PutAsync<TResult>(string uri = "", object content = default!) where TResult : new()
    {
        var body = new StringContent(content.Serialize(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{_serviceBaseUrl}/{uri}", body);

        var cinemaResponse = await ProcessResponse<Response<TResult>>(response);
        ValidateResponse(cinemaResponse);
        return cinemaResponse.Data;
    }

    protected async Task<TResult> DeleteAsync<TResult>(string uri = "") where TResult : new()
    {
        var response = await _httpClient.DeleteAsync($"{_serviceBaseUrl}/{uri}");

        var cinemaResponse = await ProcessResponse<Response<TResult>>(response);
        ValidateResponse(cinemaResponse);
        return cinemaResponse.Data;
    }

    private void ValidateResponse<TResult>(Response<TResult> response)
    {
        if (response == null)
        {
            throw new InvalidOperationException("La API no devolvió una respuesta válida.");
        }

        if (response.Succeded) return;

        var clientMessage = response.Errors?.ClientMessage;
        if (string.IsNullOrWhiteSpace(clientMessage))
        {
            clientMessage = !string.IsNullOrWhiteSpace(response.ClientMessage)
                ? response.ClientMessage
                : response.Message;
        }

        _logger.LogError(
            "La API devolvió una respuesta fallida. Mensaje: {Message}. Diagnóstico: {DiagnosticMessage}",
            clientMessage,
            response.Errors?.DiagnosticMessage);

        throw new ApiResponseException(
            string.IsNullOrWhiteSpace(clientMessage)
                ? "La operación no pudo completarse."
                : clientMessage,
            response.Errors);
    }

    private async Task<TResult> ProcessResponse<TResult>(HttpResponseMessage response) where TResult : new()
    {
        string jsonResponse = null;
        try
        {
            jsonResponse = await response.Content.ReadAsStringAsync();

            if (jsonResponse == "" && (int)response.StatusCode != StatusCodes.Status200OK || response.Content.Headers.ContentType.MediaType != "application/json")
            {
                jsonResponse = "";
                //throw new Exception($"Ocurrió un error en la solicitud con el siguiente código: {response.StatusCode}");
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new Exception($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: No se encontró el recurso para la url solicitada, {response.RequestMessage.RequestUri}");
                    case HttpStatusCode.BadRequest:
                        throw new Exception($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: Hay un Error en la Solicitud realizada");
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: La solicitud no tiene permiso al recurso solicitado");
                    case HttpStatusCode.Forbidden:
                        throw new Exception($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: La solicitud no tiene acceso al contenido");
                    case HttpStatusCode.RequestTimeout:
                        throw new Exception($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: Se expiró el tiempo de espera para la solicitud");
                    case HttpStatusCode.ServiceUnavailable:
                        throw new Exception($"Status: {(int)response.StatusCode}, Status Text: {response.StatusCode}, Message: El servicio no esta disponible");
                    default:
                        throw new Exception($"Ocurrió un error en la solicitud con el siguiente código: {response.StatusCode}");
                }
            }

            return jsonResponse.Deserialize<TResult>();
        }
        catch (Exception ex)
        {
            if (string.IsNullOrWhiteSpace(jsonResponse)) throw;
            var type = typeof(TResult);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Response<>))
            {
                var apiResponse = jsonResponse.Deserialize<Response<object>>();
                var responseResult = new TResult();

                if (responseResult is IResponse typedResponse)
                {
                    typedResponse.Succeded = apiResponse.Succeded;
                    typedResponse.Message = apiResponse.Message;
                    typedResponse.ClientMessage = apiResponse.ClientMessage;
                    typedResponse.Errors = apiResponse.Errors;
                    return responseResult;
                }
            }

            var error = $"{(int)response.StatusCode} {response.ReasonPhrase} {response.RequestMessage.RequestUri}";
            _logger.LogError($"Ocurrio un error al obtener el cuerpo de la respuesta: {error}", ex);
            return new TResult();
        }
    }
}

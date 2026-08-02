using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace WebClient.Common.Http;

/// <summary>
/// Creates HttpClient instances whose outermost delegating handler is resolved
/// from the current application scope (HTTP request or Blazor circuit).
/// </summary>
public sealed class ScopeAwareHttpClientFactory : IHttpClientFactory
{
    private readonly IServiceProvider _scopeServiceProvider;
    private readonly IHttpMessageHandlerFactory _httpMessageHandlerFactory;
    private readonly IOptionsMonitor<HttpClientFactoryOptions> _httpClientOptions;
    private readonly IOptionsMonitor<ScopeAwareHttpClientFactoryOptions> _scopeAwareOptions;

    public ScopeAwareHttpClientFactory(
        IServiceProvider scopeServiceProvider,
        IHttpMessageHandlerFactory httpMessageHandlerFactory,
        IOptionsMonitor<HttpClientFactoryOptions> httpClientOptions,
        IOptionsMonitor<ScopeAwareHttpClientFactoryOptions> scopeAwareOptions)
    {
        _scopeServiceProvider = scopeServiceProvider;
        _httpMessageHandlerFactory = httpMessageHandlerFactory;
        _httpClientOptions = httpClientOptions;
        _scopeAwareOptions = scopeAwareOptions;
    }

    public HttpClient CreateClient(string name)
    {
        var handler = _httpMessageHandlerFactory.CreateHandler(name);
        var handlerType = _scopeAwareOptions.Get(name).HttpHandlerType;

        if (handlerType is not null)
        {
            if (!typeof(DelegatingHandler).IsAssignableFrom(handlerType))
            {
                throw new InvalidOperationException(
                    $"El handler {handlerType.Name} debe heredar de DelegatingHandler.");
            }

            var scopeAwareHandler =
                (DelegatingHandler)_scopeServiceProvider.GetRequiredService(handlerType);

            if (scopeAwareHandler.InnerHandler is not null)
            {
                throw new InvalidOperationException(
                    $"El handler {handlerType.Name} debe registrarse como Transient.");
            }

            scopeAwareHandler.InnerHandler = handler;
            handler = scopeAwareHandler;
        }

        var client = new HttpClient(handler);
        var options = _httpClientOptions.Get(name);

        foreach (var configureClient in options.HttpClientActions)
        {
            configureClient(client);
        }

        return client;
    }
}

public sealed class ScopeAwareHttpClientFactoryOptions
{
    public Type? HttpHandlerType { get; set; }
}

public static class ScopeAwareHttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddScopeAwareHttpHandler<THandler>(
        this IHttpClientBuilder builder)
        where THandler : DelegatingHandler
    {
        builder.Services.TryAddTransient<THandler>();

        if (!builder.Services.Any(
                descriptor => descriptor.ImplementationType == typeof(ScopeAwareHttpClientFactory)))
        {
            builder.Services.AddTransient<IHttpClientFactory, ScopeAwareHttpClientFactory>();
        }

        builder.Services.Configure<ScopeAwareHttpClientFactoryOptions>(
            builder.Name,
            options => options.HttpHandlerType = typeof(THandler));

        return builder;
    }
}

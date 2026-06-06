using System.Net.Sockets;
using System.Net;

namespace WebClient.Extensions;

public static class HttpRequestExtensions
{
    public static string GetLocalIpAddress()
    {
        string hostName = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);
        string ip = addresses.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "1.1.1.1";
        return ip;
    }

    public static string GetClientIpAddress(this HttpRequest request)
    {
        string ip = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "1.1.1.1";

        if (!string.IsNullOrWhiteSpace(ip)) ip = ip.Split(",")[0];

        if (string.IsNullOrWhiteSpace(ip))
        {
            var ipAddress = request.HttpContext.Connection.RemoteIpAddress;
            if (ipAddress != null)
            {
                ip = !IPAddress.IsLoopback(ipAddress) ? ipAddress.MapToIPv4().ToString() : GetLocalIpAddress();
            }
        }

        if (string.IsNullOrWhiteSpace(ip)) ip = request.Headers["REMOTE_ADDR"].FirstOrDefault() ?? "1.1.1.1";

        if (ip.Contains(":")) ip = ip.Split(":")[0];

        return ip;
    }
}

using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace CrowdControl.Api;

// Prints links to the dashboard once the Api has started. If the Api listens on all
// network interfaces (the "lan" launch profile), a link per LAN address is printed too.
public static class StartupLinks
{
    // Virtual network interfaces (Docker, WSL, Hyper-V, VPN) that others on the network cannot reach
    private static readonly string[] VirtualPrefixes = { "docker", "br-", "veth", "virbr", "vEthernet", "WSL", "tun", "tap" };

    public static void PrintOnStarted(WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            ICollection<string> addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses;

            List<string> links = new List<string>();
            foreach (string address in addresses)
            {
                Uri uri = new Uri(address.Replace("://*", "://0.0.0.0").Replace("://+", "://0.0.0.0"));
                if (uri.Host == "0.0.0.0" || uri.Host == "[::]")
                {
                    links.Add($"Dashboard (denne maskine): {uri.Scheme}://localhost:{uri.Port}");
                    foreach (string ip in LanAddresses())
                        links.Add($"Dashboard (LAN):           {uri.Scheme}://{ip}:{uri.Port}");
                }
                else
                {
                    links.Add($"Dashboard (denne maskine): {address}");
                }
            }

            app.Logger.LogInformation("\n\n    {Links}\n", string.Join("\n    ", links));
        });
    }

    private static IEnumerable<string> LanAddresses()
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up
                || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback
                || VirtualPrefixes.Any(prefix => nic.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (UnicastIPAddressInformation address in nic.GetIPProperties().UnicastAddresses)
            {
                if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                    yield return address.Address.ToString();
            }
        }
    }
}

using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace CrowdControl.Api;

// Printer links til dashboardet, når Api'et er startet. Lytter Api'et på alle
// netkort (launch-profilen "lan"), printes også et link pr. LAN-adresse.
public static class StartupLinks
{
    // Virtuelle netkort (Docker, WSL, Hyper-V, VPN), som andre på netværket ikke kan nå
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

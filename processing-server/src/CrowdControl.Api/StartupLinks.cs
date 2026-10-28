using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace CrowdControl.Api;

// Prints the Api's address once it has started. If the Api listens on all network interfaces
// (the "lan" launch profile), an address per LAN IP is printed too. The dashboard's dev server
// cannot print links of its own, so the dashboard's links (same host names, port 5111) are
// printed here as well, including the one that runs the dashboard on fake data.
public static class StartupLinks
{
    // Virtual network interfaces (Docker, WSL, Hyper-V, VPN) that others on the network cannot reach
    private static readonly string[] VirtualPrefixes = { "docker", "br-", "veth", "virbr", "vEthernet", "WSL", "tun", "tap" };

    // See dashboard/src/CrowdControl.Dashboard/Properties/launchSettings.json and Program.cs
    private const int DashboardPort = 5111;
    private const string FakeFeedQuery = "?feed=fake";

    public static void PrintOnStarted(WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            ICollection<string> addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses;

            List<string> apiLinks = new List<string>();
            List<string> dashboardLinks = new List<string>();

            void Add(string where, string scheme, string host, int port)
            {
                apiLinks.Add($"{$"API ({where}):",-26} {scheme}://{host}:{port}");
                dashboardLinks.Add($"{$"Dashboard ({where}):",-26} http://{host}:{DashboardPort}");
            }

            foreach (string address in addresses)
            {
                Uri uri = new Uri(address.Replace("://*", "://0.0.0.0").Replace("://+", "://0.0.0.0"));
                if (uri.Host == "0.0.0.0" || uri.Host == "[::]")
                {
                    Add("this machine", uri.Scheme, "localhost", uri.Port);
                    foreach (string ip in LanAddresses())
                        Add("LAN", uri.Scheme, ip, uri.Port);
                }
                else
                {
                    Add("this machine", uri.Scheme, uri.Host, uri.Port);
                }
            }

            List<string> links = apiLinks.Distinct().Concat(dashboardLinks.Distinct()).ToList();
            links.Add($"{"Dashboard with fake data:",-26} http://localhost:{DashboardPort}/{FakeFeedQuery}   (no Api needed)");

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

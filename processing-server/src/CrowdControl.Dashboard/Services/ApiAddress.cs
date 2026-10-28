namespace CrowdControl.Dashboard.Services;

// Adressen på Api'et (REST + SignalR). Læses fra "ApiBaseUrl" i wwwroot/appsettings.json,
// hvor {host} erstattes med det værtsnavn, dashboardet er åbnet på. Så virker samme
// konfiguration både på localhost og fra en anden maskine på LAN'et.
public record ApiAddress(Uri BaseUrl)
{
    public static ApiAddress From(string? template, Uri dashboardAddress)
    {
        if (string.IsNullOrWhiteSpace(template))
            return new ApiAddress(dashboardAddress);

        return new ApiAddress(new Uri(template.Replace("{host}", dashboardAddress.Host)));
    }

    public Uri Resolve(string path) => new Uri(BaseUrl, path);
}

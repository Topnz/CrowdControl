namespace CrowdControl.Dashboard.Adapters;

// The address of the Api (REST + SignalR). Read from "ApiBaseUrl" in wwwroot/appsettings.json,
// where {host} is replaced with the host name the dashboard was opened on. That way the same
// configuration works both on localhost and from another machine on the LAN.
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

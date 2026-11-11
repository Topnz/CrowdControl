using System.Net.Http.Json;
using CrowdControl.Dashboard.Core.Contracts;
using CrowdControl.Dashboard.Core.Ports;
using Microsoft.AspNetCore.SignalR.Client;

namespace CrowdControl.Dashboard.Adapters;

public class CreateEventFeed : ICreateEventFeed
{
    private readonly ApiAddress _api;
    private readonly HttpClient _httpClient;

    public CreateEventFeed(ApiAddress api, HttpClient httpClient)
    {
        _api = api;
        _httpClient = httpClient;
    }

    public async void CreateEvent(CreateEventRequestDto createEventRequestDto, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Got new event");
        var response = await _httpClient.PostAsJsonAsync(
            _api.Resolve("/events"),
            createEventRequestDto,
            cancellationToken
        );
        
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);
    }
}

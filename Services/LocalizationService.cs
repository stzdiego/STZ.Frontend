using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using STZ.Shared.Bases;
using STZ.Shared.Entities;

namespace STZ.Frontend.Services;

public class LocalizationService
{
    private readonly ILanguageService _languageService;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _resources = new();
    public Guid CultureId { get; private set; }
    public event Action? OnCultureChanged;

    public LocalizationService(ILanguageService languageService, IConfiguration configuration)
    {
        _languageService = languageService;
        _configuration = configuration;
    }

    public Guid GetDefaultCultureIdAsync()
    {
        return _configuration.GetValue<Guid>("Parameters:DefaultGuidCulture");
    }

    public async Task LoadResourcesAsync(Guid cultureId)
    {
        var response = await _languageService.GetResourcesAsync(cultureId);
        
        _resources = response.Resources.ToDictionary(r => r.Code, r => r.Text);
        CultureId = cultureId;
        OnCultureChanged?.Invoke();
    }

    public string Get(string key)
    {
        return _resources.TryGetValue(key, out var value) ? value : key;
    }
    
    public async Task SetCultureAsync(Guid newCultureId)
    {
        if (newCultureId != CultureId)
        {
            await LoadResourcesAsync(newCultureId);
        }
    }
}
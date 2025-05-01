using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace STZ.Frontend.Services;

public class LocalizationService
{
    private readonly ICultureService _cultureService;
    private readonly IJSRuntime _jsRuntime;
    private Dictionary<string, string> _resources = new();
    private const int ExpirationHours = 12;
    private const string ResourceKeyFormat = "resources-{0}";
    private const string ResourceDateKeyFormat = "resources-{0}-date";

    public LocalizationService(ICultureService cultureService, IJSRuntime jsRuntime)
    {
        _cultureService = cultureService ?? throw new ArgumentNullException(nameof(cultureService));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task LoadResourcesAsync(Guid cultureId, bool jsSafeContext = false)
    {
        try
        {
            if (cultureId == Guid.Empty)
                throw new ArgumentException("El ID de la cultura no puede ser vacío.", nameof(cultureId));

            try
            {
                var resourceKey = string.Format(ResourceKeyFormat, cultureId);
                var resourceDateKey = string.Format(ResourceDateKeyFormat, cultureId);

                var storedResourcesJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", resourceKey);
                var storedDateJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", resourceDateKey);

                Dictionary<string, string>? storedResources = null;
                DateTime? storedDate = null;

                if (!string.IsNullOrEmpty(storedResourcesJson))
                    storedResources = JsonSerializer.Deserialize<Dictionary<string, string>>(storedResourcesJson);

                if (!string.IsNullOrEmpty(storedDateJson))
                    storedDate = JsonSerializer.Deserialize<DateTime>(storedDateJson);

                if (storedResources != null && storedDate != null &&
                    storedDate.Value.AddHours(ExpirationHours) > DateTime.UtcNow)
                {
                    _resources = storedResources;
                    return;
                }
            }
            catch (JsonException)
            {
                throw new Exception(
                    "Error al deserializar los recursos de localización desde el almacenamiento local.");
            }
            catch (JSException)
            {
                throw new Exception("Error al acceder al almacenamiento local.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los recursos de localización desde el almacenamiento local.", ex);
            }

            var response = await _cultureService.GetResourcesAsync(cultureId);

            if (response is null)
                throw new Exception($"No se pudo obtener los recursos de localización para la cultura seleccionada.");

            _resources = response.Resources.ToDictionary(r => r.Code, r => r.Text);

            var resourceKeyToSave = string.Format(ResourceKeyFormat, cultureId.ToString());
            var resourceDateKeyToSave = string.Format(ResourceDateKeyFormat, response.CultureCode);

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", resourceKeyToSave,
                JsonSerializer.Serialize(_resources));
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", resourceDateKeyToSave,
                JsonSerializer.Serialize(response.Date));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public string GetString(string key) => _resources.GetValueOrDefault(key, key);
}
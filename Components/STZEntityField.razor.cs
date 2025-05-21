using Microsoft.AspNetCore.Components;
using MudBlazor;
using STZ.Frontend.Dialogs;
using STZ.Shared.Bases;

namespace STZ.Frontend.Components;

public partial class STZEntityField<TItem> : ComponentBase
    where TItem : class, new()
{
    [Inject] public required ServiceBase<TItem> EntityService { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    [Parameter] public string PropertyName { get; set; }
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public TItem? SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem> SelectedItemChanged { get; set; }
    [Parameter] public Type? AddComponentType { get; set; }
    [Parameter] public bool DeleteButton { get; set; }

    private List<TItem> _items = new();
    private string _label;
    private MudAutocomplete<TItem> _autocompleteRef;
    private bool _suppressSearch = false;

    protected async override Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await OnRefresh();

        _label = Localization.Get(Label ?? "");
        Localization.OnCultureChanged += HandleCultureChanged;
    }

    private void HandleCultureChanged()
    {
        _label = Localization.Get(Label ?? "");
        InvokeAsync(StateHasChanged);
    }

    private Task<IEnumerable<TItem>> SearchItems(string? value, CancellationToken cancellationToken)
    {
        if (_suppressSearch)
        {
            _suppressSearch = false;
            return Task.FromResult(Enumerable.Empty<TItem>());
        }
        
        if (string.IsNullOrWhiteSpace(value))
            return Task.FromResult(_items.AsEnumerable());

        return Task.FromResult(_items.Where(item =>
            item.GetType().GetProperty(PropertyName)?.GetValue(item, null)?.ToString()
                ?.Contains(value, StringComparison.OrdinalIgnoreCase) == true));
    }
    
    private async Task OnSelectedItemChanged(TItem value)
    {
        SelectedItem = value;
        await SelectedItemChanged.InvokeAsync(value);
        
        // Cierra el menú y quita el foco
        _suppressSearch = true;
        await Task.Delay(100);
        await _autocompleteRef.CloseMenuAsync();
        await _autocompleteRef.BlurAsync();
    }

    private async Task OnAdd()
    {
        if (AddComponentType == null)
        {
            ShowError("No se ha definido el componente correspondiente.");
            return;
        }

        var item = new TItem();
        await ShowDialog("Adición", AddComponentType, new Dictionary<string, object> { { "Item", item } },
            async result =>
            {
                if (!result.Canceled)
                {
                    var newItem = (TItem)result.Data;
                    await EntityService.AddAsync(newItem);
                    Snackbar.Add("Registro agregado", Severity.Success);
                    await OnRefresh();
                }
            });
    }

    private async Task OnDelete()
    {
        var parameters = new DialogParameters<DeleteDialog>
        {
            { x => x.ContentText, Localization.Get("General.Confirmation") },
            { x => x.ButtonText, Localization.Get("General.Delete") },
            { x => x.Color, Color.Error }
        };
        
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar", parameters, options);
        var result = await dialog.Result;
        
        if (result?.Canceled == true)
        {
            Snackbar.Add(Localization.Get("General.Deleted.Canceled"), Severity.Info);
        }
        else
        {
            await EntityService.DeleteAsync(SelectedItem!.GetType().GetProperty("Id")?.GetValue(SelectedItem));
            SelectedItem = null;
            await SelectedItemChanged.InvokeAsync(null);
            await OnRefresh();
            Snackbar.Add(Localization.Get("General.RegisterDeleted"), Severity.Success);
        }
    }

    private async Task OnRefresh()
    {
        _items = await EntityService.GetAllAsync();

        if (SelectedItem != null)
        {
            SelectedItem = _items.FirstOrDefault(item =>
                item.GetType().GetProperty("Id")?.GetValue(item)?.Equals(
                    SelectedItem.GetType().GetProperty("Id")?.GetValue(SelectedItem)) == true);
        }
    }

    private async Task ShowDialog(string title, Type componentType, Dictionary<string, object> parameters,
        Func<DialogResult, Task>? onResult = null)
    {
        var dialogParameters = new DialogParameters
        {
            ["ComponentType"] = componentType,
            ["ComponentParameters"] = parameters
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<DataGridTemplateDialog>(title, dialogParameters, options);
        var result = await dialog.Result;

        if (onResult != null)
        {
            await onResult(result);
        }
    }

    private void ShowError(string message)
    {
        Snackbar.Add(message, Severity.Error);
    }

    public void Dispose()
    {
        Snackbar.Dispose();
        Localization.OnCultureChanged -= HandleCultureChanged;
    }
}
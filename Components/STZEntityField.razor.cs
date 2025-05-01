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
    [Parameter] public TItem SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem> SelectedItemChanged { get; set; }
    [Parameter] public Type? AddComponentType { get; set; }

    private List<TItem> _items = [];

    protected async override Task OnInitializedAsync()
    {
        await OnRefresh();
        await base.OnInitializedAsync();
    }
    
    private async Task OnSelectedItemChanged(TItem value)
    {
        SelectedItem = value;
        await SelectedItemChanged.InvokeAsync(value);
    }

    private async Task OnAdd()
    {
        if (AddComponentType == null)
        {
            ShowError("No se ha definido el componente correspondiente.");
            return;
        }

        var item = new TItem();
        await ShowDialog("Adición", AddComponentType, new Dictionary<string, object> { { "Item", item } }, async result =>
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

    private async Task OnRefresh()
    {
        _items = await EntityService.GetAllAsync();
    }
    
    private async Task ShowDialog(string title, Type componentType, Dictionary<string, object> parameters, Func<DialogResult, Task>? onResult = null)
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
}
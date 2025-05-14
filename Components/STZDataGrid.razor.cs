using Microsoft.AspNetCore.Components;
using MudBlazor;
using STZ.Frontend.Dialogs;
using STZ.Shared.Bases;

namespace STZ.Frontend.Components;

public partial class STZDataGrid<TItem, TId> : ComponentBase
    where TItem : class, IEntity<TId>, new()
{
    [Inject] public required ServiceBase<TItem> EntityService { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    [Parameter] public string Feature { get; set; }
    [Parameter] public string Title { get; set; } = "Listado";
    [Parameter] public bool ShowToolbarTitle { get; set; } = true;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public bool ShowDeleteAction { get; set; } = true;
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public RenderFragment Columns { get; set; } = default!;
    [Parameter] public Type? AddComponentType { get; set; }
    [Parameter] public Type? EditComponentType { get; set; }
    [Parameter] public Type? DetailComponentType { get; set; }

    private MudDataGrid<TItem> _dataGrid = default!;
    private TItem SelectedItem { get; set; } = default!;
    private string? _searchString;
    private string _textPlaceHolder = string.Empty;

    protected override Task OnInitializedAsync()
    {
        _textPlaceHolder = Localization.Get("General.Search");
        Localization.OnCultureChanged += HandleCultureChanged;
        return base.OnInitializedAsync();
    }
    
    private void HandleCultureChanged()
    {
        _textPlaceHolder = Localization.Get("General.Search");
        InvokeAsync(StateHasChanged);
    }

    private async Task<GridData<TItem>> ServerDataFunc(GridState<TItem> state)
    {
        try
        {
            if (EntityService == null)
                throw new InvalidOperationException("CultureService is not initialized.");

            return await EntityService.LoadServerData(state, _searchString);
        }
        catch (TaskCanceledException)
        {
            return new GridData<TItem>
            {
                Items = new List<TItem>(),
                TotalItems = 0
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Task OnSearch(string text)
    {
        _searchString = text;
        return _dataGrid.ReloadServerData();
    }

    private async Task OnDelete(CellContext<TItem?> context)
    {
        if (context.Item == null)
            return;

        var parameters = new DialogParameters<DeleteDialog>
        {
            { x => x.ContentText, "¿Está seguro de eliminar el registro?" },
            { x => x.ButtonText, "Eliminar" },
            { x => x.Color, Color.Error }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar", parameters, options);
        var result = await dialog.Result;

        if (result?.Canceled == true)
        {
            Snackbar.Add("Eliminación cancelada", Severity.Info);
        }
        else
        {
            await EntityService.DeleteAsync(context.Item.Id);
            await _dataGrid.ReloadServerData();
            Snackbar.Add("Registro eliminado", Severity.Success);
        }
    }

    private async Task ShowAddDialog()
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
                await _dataGrid.ReloadServerData();
            }
        });
    }

    private async Task ShowEditDialog(TItem item)
    {
        if (EditComponentType == null)
        {
            ShowError("No se ha definido el componente correspondiente.");
            return;
        }

        await ShowDialog("Edición", EditComponentType, new Dictionary<string, object> { { "Item", item } }, async result =>
        {
            if (!result.Canceled)
            {
                var updatedItem = (TItem?)result.Data;
                await EntityService.UpdateAsync(item.Id.ToString(), updatedItem);
                Snackbar.Add("Registro actualizado", Severity.Success);
                await _dataGrid.ReloadServerData();
            }
        });
    }

    private async Task ShowDetailDialog(TItem item)
    {
        if (DetailComponentType == null)
        {
            ShowError("No se ha definido el componente correspondiente.");
            return;
        }

        await ShowDialog("Detalle", DetailComponentType, new Dictionary<string, object> { { "Item", item } });
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
    
    public void Dispose()
    {
        Localization.OnCultureChanged -= HandleCultureChanged;
    }
}
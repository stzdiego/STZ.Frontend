namespace STZ.Frontend.Services;

public class LoaderService
{
    public string Message { get; private set; } = string.Empty;
    
    public event Action<string> OnShow;
    public event Action OnHide;

    public void Show(string message = "")
    {
        Message = message;
        OnShow?.Invoke(message);
    }
    
    public void Hide() => OnHide?.Invoke();
}
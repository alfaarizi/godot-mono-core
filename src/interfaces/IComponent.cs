public interface IComponent
{
    bool IsEnabled { get; set; }
    void SetEnabled(bool isEnabled);
}
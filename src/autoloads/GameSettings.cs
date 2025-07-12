using Godot;

public partial class GameSettings : Node
{
    private static readonly Vector2I MaxResolution = new(1920, 1080);

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            RenderingServer.SetDefaultClearColor(Colors.Black);

            var window = GetWindow();
            window.ContentScaleMode = Window.ContentScaleModeEnum.Viewport;
            window.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
            window.ContentScaleSize = MaxResolution;
        }
    }
}
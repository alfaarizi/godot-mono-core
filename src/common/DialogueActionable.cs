using Godot;
using DialogueManagerRuntime;

public partial class DialogueActionable : Actionable
{
    [Export] public Resource? DialogueResource { get; set; }
    [Export] public string DialogueStart { get; set; } = "start";

    public override void _Ready()
    {
        base._Ready();
        ActionRequested += OnActionRequested;
    }

    public void OnActionRequested(Node2D actor)
    {
        if (actor.IsInGroup("dialogue_actor"))
            _ = DialogueManager.ShowDialogueBalloon(DialogueResource, DialogueStart);
    }
}
using DialogueManagerRuntime;
using Godot;

public partial class DialogueActionable : Actionable
{
    [Export] public Resource? DialogueResource { get; set; }
    [Export] public string DialogueStart { get; set; } = "start";
    private (InputComponent? input, MovementComponent? movement) _components;

    public void OnActionRequested(Node2D actor)
    {
        if (!actor.IsInGroup("dialogue_actor")) return;

        _components = (
           actor.GetNodeOrNull<InputComponent>("%InputComponent"),
           actor.GetNodeOrNull<MovementComponent>("%MovementComponent")
        );

        _components.input?.SetEnabled(false);
        _components.movement?.SetEnabled(false);

        _ = DialogueManager.ShowDialogueBalloon(DialogueResource, DialogueStart);
    }

    public override void _Ready()
    {
        base._Ready();
        ActionRequested += OnActionRequested;
        DialogueManager.DialogueEnded += OnDialogueEnded;
    }

    public override void _ExitTree()
    {
        DialogueManager.DialogueEnded -= OnDialogueEnded;
        base._ExitTree();
    }

    private void OnDialogueEnded(Resource dialogueResource)
    {
        if (dialogueResource != DialogueResource) return;

        _components.input?.SetEnabled(true);
        _components.movement?.SetEnabled(true);

        _components = (null, null);
    }
}
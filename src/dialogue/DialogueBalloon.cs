using Godot;
using Godot.Collections;

namespace DialogueManagerRuntime
{
#nullable disable
    public partial class DialogueBalloon : CanvasLayer
    {
        [Export] public string NextAction { get; set; } = "ui_accept";
        [Export] public string SkipAction { get; set; } = "ui_cancel";


        private Control balloon;
        private RichTextLabel characterLabel;
        private RichTextLabel dialogueLabel;
        private VBoxContainer responsesMenu;

        private Resource resource;
        private Array<Variant> temporaryGameStates = new();
        private bool isWaitingForInput;
        private bool willHideBalloon;

        private DialogueLine dialogueLine;
        private DialogueLine DialogueLine
        {
            get => dialogueLine;
            set
            {
                if (value == null)
                {
                    QueueFree();
                    return;
                }

                dialogueLine = value;
                ApplyDialogueLine();
            }
        }

        private Timer MutationCooldown = new();


        public override void _Ready()
        {
            balloon = GetNode<Control>("%Balloon");
            characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
            responsesMenu = GetNode<VBoxContainer>("%ResponsesMenu");

            balloon.Hide();

            balloon.GuiInput += (@event) =>
            {
                if ((bool)dialogueLabel.Get("is_typing"))
                {
                    bool mouseWasClicked = @event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left && @event.IsPressed();
                    bool skipButtonWasPressed = @event.IsActionPressed(SkipAction);
                    if (mouseWasClicked || skipButtonWasPressed)
                    {
                        GetViewport().SetInputAsHandled();
                        _ = dialogueLabel.Call("skip_typing");
                        return;
                    }
                }

                if (!isWaitingForInput) return;
                if (dialogueLine.Responses.Count > 0) return;

                GetViewport().SetInputAsHandled();

                if (@event is InputEventMouseButton && @event.IsPressed() && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
                {
                    Next(dialogueLine.NextId);
                }
                else if (@event.IsActionPressed(NextAction) && GetViewport().GuiGetFocusOwner() == balloon)
                {
                    Next(dialogueLine.NextId);
                }
            };

            if (string.IsNullOrEmpty((string)responsesMenu.Get("next_action")))
            {
                responsesMenu.Set("next_action", NextAction);
            }
            _ = responsesMenu.Connect("response_selected", Callable.From((DialogueResponse response) =>
            {
                Next(response.NextId);
            }));


            // Hide the balloon when a mutation is running
            MutationCooldown.Timeout += () =>
            {
                if (willHideBalloon)
                {
                    willHideBalloon = false;
                    balloon.Hide();
                }
            };
            AddChild(MutationCooldown);

            DialogueManager.Mutated += OnMutated;
        }


        public override void _ExitTree()
        {
            DialogueManager.Mutated -= OnMutated;
        }


        public override void _UnhandledInput(InputEvent @event)
        {
            // Only the balloon is allowed to handle input while it's showing
            GetViewport().SetInputAsHandled();
        }


        public override async void _Notification(int what)
        {
            // Detect a change of locale and update the current dialogue line to show the new language
            if (what == NotificationTranslationChanged && IsInstanceValid(dialogueLabel))
            {
                float visibleRatio = dialogueLabel.VisibleRatio;
                DialogueLine = await DialogueManager.GetNextDialogueLine(resource, DialogueLine.Id, temporaryGameStates);
                if (visibleRatio < 1.0f)
                {
                    _ = dialogueLabel.Call("skip_typing");
                }
            }
        }


        public async void Start(Resource dialogueResource, string title, Array<Variant> extraGameStates = null)
        {
            temporaryGameStates = new Array<Variant> { this } + (extraGameStates ?? new Array<Variant>());
            isWaitingForInput = false;
            resource = dialogueResource;

            DialogueLine = await DialogueManager.GetNextDialogueLine(resource, title, temporaryGameStates);
        }


        public async void Next(string nextId)
        {
            DialogueLine = await DialogueManager.GetNextDialogueLine(resource, nextId, temporaryGameStates);
        }


        #region Helpers


        private async void ApplyDialogueLine()
        {
            MutationCooldown.Stop();

            isWaitingForInput = false;
            balloon.FocusMode = Control.FocusModeEnum.All;
            balloon.GrabFocus();

            // Set up the character name
            characterLabel.Visible = !string.IsNullOrEmpty(dialogueLine.Character);
            characterLabel.Text = Tr(dialogueLine.Character, "dialogue");

            // Set up the dialogue
            dialogueLabel.Hide();
            dialogueLabel.Set("dialogue_line", dialogueLine);

            // Set up the responses
            responsesMenu.Hide();
            responsesMenu.Set("responses", dialogueLine.Responses);

            // Type out the text
            balloon.Show();
            willHideBalloon = false;
            dialogueLabel.Show();
            if (!string.IsNullOrEmpty(dialogueLine.Text))
            {
                _ = dialogueLabel.Call("type_out");
                _ = await ToSignal(dialogueLabel, "finished_typing");
            }

            // Wait for input
            if (dialogueLine.Responses.Count > 0)
            {
                balloon.FocusMode = Control.FocusModeEnum.None;
                responsesMenu.Show();
            }
            else if (!string.IsNullOrEmpty(dialogueLine.Time))
            {
                if (!float.TryParse(dialogueLine.Time, out float time))
                {
                    time = dialogueLine.Text.Length * 0.02f;
                }
                _ = await ToSignal(GetTree().CreateTimer(time), "timeout");
                Next(dialogueLine.NextId);
            }
            else
            {
                isWaitingForInput = true;
                balloon.FocusMode = Control.FocusModeEnum.All;
                balloon.GrabFocus();
            }
        }


        #endregion


        #region signals


        private void OnMutated(Dictionary _mutation)
        {
            isWaitingForInput = false;
            willHideBalloon = true;
            MutationCooldown.Start(0.1f);
        }


        #endregion
    }
#nullable enable
}
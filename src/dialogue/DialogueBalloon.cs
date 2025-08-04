using Godot;
using Godot.Collections;

namespace DialogueManagerRuntime
{
#nullable disable
    public partial class DialogueBalloon : CanvasLayer
    {
        [Export] public string NextAction { get; set; } = "ui_interact";
        [Export] public string SkipAction { get; set; } = "ui_skip";
        [Export] public AudioStream VoiceAudio { get; set; }
        [Export] public int MaxCharactersPerLine { get; set; } = 74;


        private Control balloon;
        private RichTextLabel characterLabel;
        private RichTextLabel dialogueLabel;
        private VBoxContainer responsesMenu;
        private ScrollContainer scrollContainer;
        private AnimationPlayer animationPlayer;
        private AudioStreamPlayer audioStreamPlayer;

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
                    if (IsInstanceValid(portraits[0]))
                        portraits[0]?.QueueFree();
                    if (IsInstanceValid(portraits[1]))
                        portraits[1]?.QueueFree();
                    QueueFree();
                    return;
                }

                dialogueLine = value;
                ApplyDialogueLine();
            }
        }

        private Timer MutationCooldown = new();

        private Marker2D[] portraitPositions = new Marker2D[2];

        private BasePortrait[] portraits = new BasePortrait[2];

        public override void _Ready()
        {
            balloon = GetNode<Control>("%Balloon");
            characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
            responsesMenu = GetNode<VBoxContainer>("%ResponsesMenu");
            scrollContainer = GetNode<ScrollContainer>("%ScrollContainer");
            animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
            audioStreamPlayer = GetNode<AudioStreamPlayer>("%AudioStreamPlayer");

            portraitPositions[0] = GetNode<Marker2D>("%LeftPortraitPosition");
            portraitPositions[1] = GetNode<Marker2D>("%RightPortraitPosition");

            balloon.Hide();
            portraitPositions[0].Hide();
            portraitPositions[1].Hide();

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

            _ = dialogueLabel.Connect("spoke", Callable.From((string letter, int letterIndex, float speed) =>
            {
                if (letter == " " || letter == "." || letterIndex % 2 == 0) return;

                if (VoiceAudio != null)
                {
                    audioStreamPlayer.Stream = VoiceAudio;
                    audioStreamPlayer.PitchScale = (float)GD.RandRange(0.8, 1.2);
                    audioStreamPlayer.Play();
                }

                if (letterIndex % MaxCharactersPerLine == 0)
                    ScrollToFollow();
            }));

            // Hide the balloon when a mutation is running
            MutationCooldown.Timeout += () =>
            {
                if (willHideBalloon)
                {
                    willHideBalloon = false;
                    balloon.Hide();
                    portraitPositions[0].Hide();
                    portraitPositions[1].Hide();
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

            if (!string.IsNullOrEmpty(dialogueLine.Character))
                UpdatePortraits(dialogueLine.Character, dialogueLine.Tags);

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

        private async void UpdatePortraits(string character, Array<string> tags)
        {
            int side = tags.Contains("right") ? 1 : 0; // default to left

            string expectedSceneFilePath = $"res://src/dialogue/portraits/{character}Portrait.tscn";
            bool hasCharacterChanged = !IsInstanceValid(portraits[side]) || portraits[side].SceneFilePath != expectedSceneFilePath;

            if (hasCharacterChanged)
            {
                // Hide existing portrait, if any
                if (IsInstanceValid(portraits[side]))
                {
                    string hideAnimation = side == 0 ? "hide_left_portrait" : "hide_right_portrait";
                    animationPlayer.Play(hideAnimation);
                    _ = await ToSignal(animationPlayer, AnimationMixer.SignalName.AnimationFinished);

                    portraits[side].QueueFree();
                    portraits[side] = null;
                }

                // Load and show new portrait
                if (ResourceLoader.Exists(expectedSceneFilePath))
                {
                    var scene = GD.Load<PackedScene>(expectedSceneFilePath);
                    if (scene?.Instantiate() is BasePortrait newPortrait)
                    {
                        portraits[side] = newPortrait;
                        portraitPositions[side].AddChild(newPortrait);
                        portraitPositions[side].Show();

                        if (side == 1)
                            newPortrait.Scale = newPortrait.Scale with { X = -1 };

                        string showAnimation = side == 0 ? "show_left_portrait" : "show_right_portrait";
                        animationPlayer.Play(showAnimation);
                        _ = await ToSignal(animationPlayer, AnimationMixer.SignalName.AnimationFinished);
                    }
                }
                else
                {
                    GD.PrintErr($"Portrait file not found. Expected: {expectedSceneFilePath}");
                }
            }

            if (IsInstanceValid(portraits[side]))
                _ = portraits[side].Call("Emote", tags);
        }

        private void ScrollToFollow()
        {
            float visibleHeight = dialogueLabel.Size.Y * dialogueLabel.VisibleRatio;
            if (visibleHeight > scrollContainer.Size.Y)
                scrollContainer.ScrollVertical = (int)(scrollContainer.GetVScrollBar().MaxValue * dialogueLabel.VisibleRatio);
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
using Godot;
using Godot.Collections;

public partial class BasePortrait : Node2D
{
    private AnimationPlayer? animationPlayer;

    public override void _Ready()
        => animationPlayer = GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");

    public void Emote(Array<string> emotions)
    {
        foreach (var emotion in emotions)
            if (animationPlayer?.HasAnimation(emotion) == true) animationPlayer?.Play(emotion);
    }

}
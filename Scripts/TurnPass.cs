using Godot;
using System;

public class TurnPass : Control
{
    AnimationPlayer animationPlayer;
    Label label;
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
        label = GetNode<Label>("%Label");
    }

    public async void Animate(string turn)
    {
        label.Text = turn;
        animationPlayer.Play("slide_in");
        await ToSignal(animationPlayer, "animation_finished");
        animationPlayer.Play("slide_out");
        await ToSignal(animationPlayer, "animation_finished");
        Visible = false;
    }


}

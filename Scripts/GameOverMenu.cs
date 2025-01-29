using Godot;
using System;

public class GameOverMenu : Control
{
    Label winnerNameLabel;

    public override void _Ready()
    {
        winnerNameLabel = GetNode<Label>("%WinnerName");
    }

    public void SetWinnerName(string name)
    {
        winnerNameLabel.Text = $"{name} checkers wins!";
    }

    public void OnRestartTriggered()
    {
        GetTree().ReloadCurrentScene();
    }
}

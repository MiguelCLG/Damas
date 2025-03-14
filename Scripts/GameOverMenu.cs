using Godot;

public class GameOverMenu : Control
{
    Label winnerNameLabel;
    TextureRect winnerTextureRect;

    public override void _Ready()
    {
        winnerNameLabel = GetNode<Label>("%WinnerName");
        winnerTextureRect = GetNode<TextureRect>("%WinnerChecker");
    }

    public void SetWinnerName(BoardColors name)
    {
        if (name == GameState.currentGameColor)
            winnerNameLabel.Text = "_win_";
        else
            winnerNameLabel.Text = "_lose_";
        if (name == BoardColors.Black)
            winnerTextureRect.Texture = GD.Load<Texture>("res://Assets/In Game Assets/crowd1.webp");
        else
            winnerTextureRect.Texture = GD.Load<Texture>("res://Assets/In Game Assets/crowd2.webp");
    }

    public void OnRestartTriggered()
    {
        GetNode<SceneLoaders>("/root/SceneLoaders").NextScene();
    }
}

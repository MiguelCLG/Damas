using System.Diagnostics;
using Godot;

public class GameOverMenu : Control
{
    [Export] Texture[] GameOverImage;
    Label Label1; // gray Que Tal // hide if win
    Label Label2; // white Voce Ganhou / tentar novamente
    Label Label3; // green Winnings // hide if lose

    Button PlayAgainButton;

    TextureRect gameOverImage;

    public override void _Ready()
    {
        Label1 = GetNode<Label>("%Title");
        Label2 = GetNode<Label>("%WinnerName");
        Label3 = GetNode<Label>("%Winnings");
        PlayAgainButton = GetNode<Button>("%InicioButton");
        Label3.Visible = false;
        gameOverImage = GetNode<TextureRect>("%GameOverImage");
    }

    public void SetWinnerName(BoardColors name, float winnings = 0)
    {
        if (name == GameState.currentGameColor)
        {
            GD.Print("CHEGOU AQUI");
            var currencyInfo = Utils.GetCurrencySymbol(GameState.Currency);
            if (currencyInfo.Position == Utils.SymbolPosition.Left)
                Label3.Text = $" {currencyInfo.Symbol}{winnings}";
            else
                Label3.Text = $" {winnings}{currencyInfo.Symbol}";
            Label2.Text = "_winner_title_";
            Label3.Visible = true;
            Label1.Visible = false;
            PlayAgainButton.Text = "_restart_button_";
            gameOverImage.Texture = GameOverImage[0];
        }
        else
        {
            Label1.Visible = true;
            Label1.Text = "_loser_title_";
            Label2.Text = "_lose_";
            Label3.Visible = false;
            PlayAgainButton.Text = "_restart_button_lose_";
            gameOverImage.Texture = GameOverImage[1];
        }
    }

    public void OnRestartTriggered()
    {
        GetNode<SceneLoaders>("/root/SceneLoaders").NextScene();
    }
}

using Godot;

public class GameOverMenu : Control
{
    Label winnerNameLabel;
    Label title;
    TextureRect winnerTextureRect;

    public override void _Ready()
    {
        winnerNameLabel = GetNode<Label>("%WinnerName");
        title = GetNode<Label>("%Title");
        winnerTextureRect = GetNode<TextureRect>("%WinnerChecker");
    }

    public void SetWinnerName(BoardColors name, float winnings = 0)
    {
        if (name == GameState.currentGameColor)
        {
            var currencyInfo = Utils.GetCurrencySymbol(GameState.Currency);
            if (currencyInfo.Position == Utils.SymbolPosition.Left)
                winnerNameLabel.Text = TranslationServer.Translate("_win_") + $" {currencyInfo.Symbol}{winnings}";
            else
                winnerNameLabel.Text = TranslationServer.Translate("_win_") + $" {winnings}{currencyInfo.Symbol}";
            title.Text = "_winner_title_";
        }
        else
        {
            title.Text = "_loser_title_";
            winnerNameLabel.Text = "_lose_";
        }
        if (name == BoardColors.Black)
            winnerTextureRect.Texture = GD.Load<Texture>("res://Assets/in_game_assets/crowd1.webp");
        else
            winnerTextureRect.Texture = GD.Load<Texture>("res://Assets/in_game_assets/crowd2.webp");
    }

    public void OnRestartTriggered()
    {
        GetNode<SceneLoaders>("/root/SceneLoaders").NextScene();
    }
}

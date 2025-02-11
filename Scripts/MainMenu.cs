using Godot;
using System;

public partial class MainMenu : Control
{
  private SceneLoaders sceneLoaders; // autoload to handle transitions and switching scenes
  private Label PlayerName;
  private Label PlayerMoney;

  public override void _Ready()
  {
    Utils.ViewportBaseX = GetViewportRect().Size.x;
    Utils.ViewportBaseY = GetViewportRect().Size.y;
    sceneLoaders = GetNode<SceneLoaders>("/root/SceneLoaders");
    PlayerName = GetNode<Label>("%PlayerName");
    PlayerMoney = GetNode<Label>("%PlayerMoney");
  }

  public void SetPlayerName(string newPlayerName)
  {
    PlayerName.Text = newPlayerName;
  }
  public void SetPlayerMoney(string newPlayerMoney)
  {
    PlayerMoney.Text = newPlayerMoney;
  }
  public void OnStartGame()
  {
    EventRegistry.GetEventPublisher("JoinQueue").RaiseEvent(this);
    //sceneLoaders.NextScene();
  }
}

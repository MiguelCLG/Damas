using Godot;
using System;

public partial class MainMenu : Control
{
  SceneLoaders sceneLoaders; // autoload to handle transitions and switching scenes

  public override void _Ready()
  {
    Utils.ViewportBaseX = GetViewportRect().Size.x;
    Utils.ViewportBaseY = GetViewportRect().Size.y;
    sceneLoaders = GetNode<SceneLoaders>("/root/SceneLoaders");
  }

  public void OnStartGame()
  {
    sceneLoaders.NextScene();
  }
}

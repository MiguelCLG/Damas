using System;
using System.Linq;
using Godot;
using Godot.Collections;

// Autoload script that will add a node automatically to the scene tree and will allow the changing of the scene independently of the scene we are in
public partial class SceneLoaders : Node
{
  [Export] Array<string> scenePaths = new Array<string>();
  AnimationPlayer transitionAnim;
  string currentScenePath;

  string[] enterAnim = { "fade_in" };
  string[] exitAnim = { "fade_out" };
  public override void _Ready()
  {
    // Add scenes to build order
    // basically we are creating a build order for the scenes
    // so that we can use NextScene() and PreviousScene()
    scenePaths.Add("Scenes/main-menu.tscn");
    scenePaths.Add("Scenes/main.tscn");
    currentScenePath = scenePaths[0];

    //Loading the scene transition UI into the scene
    CanvasLayer cv = new CanvasLayer();
    var transitionScene = GD.Load<PackedScene>("res://Scenes/scene_transition.tscn");
    var transition = transitionScene.Instance<Control>();
    transitionAnim = transition.GetNode<AnimationPlayer>("Panel/AnimationPlayer");
    cv.AddChild(transition);
    cv.Layer = 2; // If we need to change more layers, we need to figure out a system for it, at the moment it's here for the transition to be on top of the game
    AddChild(cv);

    // the animation player has a signal called animation_finished which helps time the transitions and changing of the scene.
    Action<string> CallTransition = (string animationName) => OnAnimationFinished(animationName);
    transitionAnim.Connect("animation_finished", this, nameof(OnAnimationFinished));
    transitionAnim.Play("fade_in");
  }

  public void OnAnimationFinished(string animationName)
  {
    if (exitAnim.Contains(animationName))
    {
      GetTree().ChangeScene(currentScenePath);
      transitionAnim.Play("fade_in");
    }
  }
  public void OnStartGame()
  {
    transitionAnim.Play("fade_out");
    currentScenePath = "Scenes/main.tscn";
  }
  public void NextScene()
  {
    transitionAnim.Play("fade_out");
    var index = scenePaths.IndexOf(currentScenePath);
    currentScenePath = scenePaths[index > scenePaths.Count - 1 ? 0 : index + 1];
  }
  public void PreviousScene()
  {
    transitionAnim.Play("fade_out");
    var index = scenePaths.IndexOf(currentScenePath);
    currentScenePath = scenePaths[index < 1 ? scenePaths.Count - 1 : index - 1];
  }
  public void SelectScene(string scenePath)
  {
    transitionAnim.Play("fade_out");
    currentScenePath = scenePath;
  }

  public void PlayAnimation(string animationName)
  {
    transitionAnim.Play(animationName);
  }

  public void OnQuitGame()
  {
    GetTree().Quit();
  }
}

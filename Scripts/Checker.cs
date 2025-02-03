using Godot;
using Godot.Collections;

public partial class Checker : Control
{
  [Export] public BoardColors Color { get; set; }
  [Export] public Vector2 BoardPosition { get; set; } = new Vector2(4, 3); // The position on the board (goes from v(0,0) to v(7,7)
  [Export] public Array<Vector2> MovementSpaces { get; set; } = new Array<Vector2>(); // Tracker array for possible movement spaces

  TextureButton texture;

  public override void _Ready()
  {
    texture = GetNode<TextureButton>("%Texture");
  }

  public void SetCheckerColor(BoardColors color)
  {
    Color = color;
    texture.TextureNormal = color == BoardColors.White ? GD.Load<Texture>("res://Assets/white-checker.png") : GD.Load<Texture>("res://Assets/black-checker.png");
  }

  // Selecting the checker will change its texture so we can visually see it is selected
  public void SelectChecker()
  {
    texture.TextureNormal = GD.Load<Texture>("res://Assets/checker-selected.png");
  }

  public void ChangeTexture(Texture newTexture)
  {
    texture.TextureNormal = newTexture;
  }

  public void ChangeTexture(string texturePath)
  {
    texture.TextureNormal = GD.Load<Texture>(texturePath);
  }
  public void UnselectChecker()
  {
    SetCheckerColor(Color);
  }

  // There are no animations for movement as of now, so we just change the position
  public void Move(Vector2 newPosition, Vector2 newBoardPosition)
  {
    BoardPosition = newBoardPosition;
    RectPosition = newPosition;
  }
  public void OnCheckerClicked()
  {
    EventRegistry.GetEventPublisher("CheckerClicked").RaiseEvent(this); // Raise the event
  }



}

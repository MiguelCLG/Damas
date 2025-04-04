using Godot;

public partial class Tile : Control
{
  [Export] public Vector2 TilePosition { get; set; }
  [Export] private BoardColors TileColor { get; set; }

  [Export] StyleBoxFlat yellowBackground;
  [Export] StyleBoxFlat whiteBackground;
  [Export] StyleBoxFlat purpleBackground;
  [Export] StyleBoxFlat blackBackground;
  [Export] StyleBoxFlat redBackground;
  [Export] StyleBoxFlat greenBackground;
  Button DefaultTileButton;
  Button PreviousMovementTileButton;
  Button CanMoveTileButton;
  Button MovementTileButton;
  Button CaptureTileButton;

  public override void _Ready()
  {
    DefaultTileButton = GetNode<Button>("%DefaultTile");
    PreviousMovementTileButton = GetNode<Button>("%PreviousMovementTile");
    CanMoveTileButton = GetNode<Button>("%CanMoveTile");
    MovementTileButton = GetNode<Button>("%MovementTile");
    CaptureTileButton = GetNode<Button>("%CaptureTile");
  }

  public void SetTileColor(Button button, BoardColors color)
  {
    TileColor = color;
    MultipleStyleBox(button, color == BoardColors.White ? whiteBackground : blackBackground);
  }

  public void MultipleStyleBox(Button button, StyleBoxFlat background)
  {
    button.AddStyleboxOverride("normal", background);
    button.AddStyleboxOverride("hover", background);
    button.AddStyleboxOverride("pressed", background);
    button.AddStyleboxOverride("disabled", background);

  }
  public void Select(bool isCapture)
  {
    if (isCapture)
    {
      MultipleStyleBox(CaptureTileButton, redBackground);
    }
    else
    {
      MultipleStyleBox(MovementTileButton, yellowBackground);
    }
  }
  public void Unselect()
  {
    SetTileColor(DefaultTileButton, TileColor);
  }

  public void SelectAsMovement()
  {
    MultipleStyleBox(PreviousMovementTileButton, purpleBackground);
    GetTree().CreateTimer(2f).Connect("timeout", this, nameof(Unselect)); // leave it for 2 seconds and return to the proper thingy
  }

  public void SelectAsCanMove()
  {
    MultipleStyleBox(CanMoveTileButton, greenBackground);
  }

  public void SelectAsCanCapture()
  {
    MultipleStyleBox(CaptureTileButton, redBackground);
  }


  // Event sent so that the turn base system knows which tile was clicked
  public void OnTileClicked()
  {
    EventRegistry.GetEventPublisher("TileClicked").RaiseEvent(this);
  }

}

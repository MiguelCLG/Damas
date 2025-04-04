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


    DefaultTileButton.Visible = true;
    PreviousMovementTileButton.Visible = false;
    CanMoveTileButton.Visible = false;
    MovementTileButton.Visible = false;
    CaptureTileButton.Visible = false;

    SetTileColor(TileColor);
    MultipleStyleBox(CanMoveTileButton, greenBackground);
    MultipleStyleBox(MovementTileButton, yellowBackground);
    MultipleStyleBox(CaptureTileButton, redBackground);
    MultipleStyleBox(PreviousMovementTileButton, purpleBackground);
  }

  public void SetTileColor(BoardColors color)
  {
    TileColor = color;
    MultipleStyleBox(DefaultTileButton, color == BoardColors.White ? whiteBackground : blackBackground);
  }

  public void MultipleStyleBox(Button button, StyleBoxFlat background)
  {
    button.AddStyleboxOverride("normal", background);
    button.AddStyleboxOverride("hover", background);
    button.AddStyleboxOverride("pressed", background);
    button.AddStyleboxOverride("focus", background);
    button.AddStyleboxOverride("disabled", background);

  }
  public void Select(bool isCapture)
  {
    if (isCapture)
    {
      CaptureTileButton.Visible = true;
    }
    else
    {
      MovementTileButton.Visible = true;
    }
  }

  public void Unselect()
  {
    UnselectAsCanMove();
    UnselectAsCanCapture();
    UnselectMovement();
  }

  public void SelectAsPreviousMovement()
  {
    PreviousMovementTileButton.Visible = true;
    GetTree().CreateTimer(2f).Connect("timeout", this, nameof(UnselectPreviousMovement)); // leave it for 2 seconds and return to the proper thingy
  }

  public void SelectAsCanMove()
  {
    CanMoveTileButton.Visible = true;
  }

  public void SelectAsCanCapture()
  {
    CaptureTileButton.Visible = true;
  }

  public void UnselectAsCanMove()
  {
    CanMoveTileButton.Visible = false;
  }
  public void UnselectAsCanCapture()
  {
    CaptureTileButton.Visible = false;
  }
  public void UnselectMovement()
  {
    MovementTileButton.Visible = false;
  }
  public void UnselectPreviousMovement()
  {
    PreviousMovementTileButton.Visible = false;
  }

  // Event sent so that the turn base system knows which tile was clicked
  public void OnTileClicked()
  {
    EventRegistry.GetEventPublisher("TileClicked").RaiseEvent(this);
  }

}

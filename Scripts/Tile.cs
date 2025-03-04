using Godot;

public partial class Tile : Control
{
  [Export] public Vector2 TilePosition { get; set; }
  [Export] private BoardColors TileColor { get; set; }
  [Export] private Texture WhiteTileTexture { get; set; }
  [Export] private Texture BlackTileTexture { get; set; }

  TextureButton texture;

  public override void _Ready()
  {
    texture = GetNode<TextureButton>("%Texture");
  }

  public void SetTileColor(BoardColors color)
  {
    TileColor = color;
    texture.TextureNormal = color == BoardColors.White ? WhiteTileTexture : BlackTileTexture;
  }

  public void Select(bool isCapture)
  {
    if (isCapture)
      texture.TextureNormal = GD.Load<Texture>("res://Assets/capture-tile.png");
    else
      texture.TextureNormal = GD.Load<Texture>("res://Assets/available-tile.png");
  }
  public void Unselect()
  {
    SetTileColor(TileColor);
  }

  public void SelectAsMovement()
  {
    texture.TextureNormal = GD.Load<Texture>("res://Assets/movement-tile.webp");
  }

  // Event sent so that the turn base system knows which tile was clicked
  public void OnTileClicked()
  {
    EventRegistry.GetEventPublisher("TileClicked").RaiseEvent(this);
  }

}

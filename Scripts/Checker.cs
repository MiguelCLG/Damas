using Godot;
using Godot.Collections;
using static Utils;
public partial class Checker : Control
{
  private string id;
  [Export] public BoardColors Color { get; set; }
  [Export] public Vector2 BoardPosition { get; set; } = new Vector2(4, 3); // The position on the board (goes from v(0,0) to v(7,7)
  [Export] public Array<Vector2> MovementSpaces { get; set; } = new Array<Vector2>(); // Tracker array for possible movement spaces
  [Export] private Texture WhiteCheckerTexture { get; set; }
  [Export] private Texture WhiteKingTexture { get; set; }
  [Export] private Texture BlackCheckerTexture { get; set; }
  [Export] private Texture BlackKingTexture { get; set; }
  private Panel selectedPanel;
  private Panel requiredPanel;
  private AnimationPlayer animationPlayer;
  public bool isKing { get; set; } = false;
  private TextureButton texture;

  public override void _Ready()
  {
    texture = GetNode<TextureButton>("%Texture");
    selectedPanel = GetNode<Panel>("%Selected");
    requiredPanel = GetNode<Panel>("%Required");
    animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
    selectedPanel.Visible = false;
    requiredPanel.Visible = false;
    RectMinSize = new Vector2(CheckerSize, CheckerSize);
    texture.RectMinSize = new Vector2(CheckerSize, CheckerSize);
  }
  public string GetId() { return id; }
  public void SetId(string id) { this.id = id; }
  public void SetCheckerColor(BoardColors color)
  {
    Color = color;
    if (isKing)
      texture.TextureNormal = color == BoardColors.White ? WhiteKingTexture : BlackKingTexture;
    else
      texture.TextureNormal = color == BoardColors.White ? WhiteCheckerTexture : BlackCheckerTexture;
    selectedPanel.Visible = false;
    requiredPanel.Visible = false;
    animationPlayer.Stop();
  }
  // Selecting the checker will change its texture so we can visually see it is selected
  public void SelectChecker()
  {
    selectedPanel.Visible = true;
    animationPlayer.Play("selected_glow");
  }

  public void ChangeTexture(Texture newTexture)
  {
    texture.TextureNormal = newTexture;
  }

  public void ChangeTexture(string texturePath)
  {
    texture.TextureNormal = GD.Load<Texture>(texturePath);
  }
  public void SelectCapture()
  {
    requiredPanel.Visible = true;
    animationPlayer.Play("required_glow");
  }
  public void UnselectChecker()
  {
    SetCheckerColor(Color);
  }

  public void SetKing()
  {
    isKing = true;
    texture.TextureNormal = Color == BoardColors.White ? WhiteKingTexture : BlackKingTexture;
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

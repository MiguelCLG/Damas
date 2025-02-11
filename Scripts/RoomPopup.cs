using Godot;
using System;

public class RoomPopup : Panel
{
    [Export] private StyleBoxTexture NotReadyButtonBackground;
    [Export] private StyleBoxTexture ReadyButtonBackground;
    private Label PlayerName;
    private Label OponentName;
    private TextureRect PlayerTexture;
    private TextureRect OpponentTexture;
    private Button ReadyButton;
    private TextureRect BidTextureRect;
    private Label BidValue;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayerName = GetNode<Label>("%PlayerNameLabel");
        OponentName = GetNode<Label>("%OponentNameLabel");
        BidTextureRect = GetNode<TextureRect>("%BidTexture");
        PlayerTexture = GetNode<TextureRect>("%PlayerTexture");
        OpponentTexture = GetNode<TextureRect>("%OpponentTexture");
        ReadyButton = GetNode<Button>("%PlayerReadyButton");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //
    //  }
}

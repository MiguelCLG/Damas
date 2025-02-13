using Godot;
using Godot.Collections;
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
    private PanelContainer RoomContainer;
    private PanelContainer WaitingForDataContainer;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayerName = GetNode<Label>("%PlayerNameLabel");
        OponentName = GetNode<Label>("%OponentNameLabel");
        BidTextureRect = GetNode<TextureRect>("%BidTexture");
        PlayerTexture = GetNode<TextureRect>("%PlayerTexture");
        OpponentTexture = GetNode<TextureRect>("%OpponentTexture");
        ReadyButton = GetNode<Button>("%PlayerReadyButton");
        RoomContainer = GetNode<PanelContainer>("%RoomContainer");
        WaitingForDataContainer = GetNode<PanelContainer>("%WaitingForDataContainer");

        EventSubscriber.SubscribeToEvent("OnDataReceived", OnDataReceived);

        WaitingForDataContainer.Visible = true;
        RoomContainer.Visible = false;
    }

    public void OnReadyButtonPressed(bool buttonPressed)
    {
        ReadyButton.AddStyleboxOverride("normal", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);
        ReadyButton.AddStyleboxOverride("hover", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);
        ReadyButton.AddStyleboxOverride("pressed", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);
    }
    public void SetPlayerName(string name)
    {
        PlayerName.Text = name;
    }

    public void SetOponentName(string name)
    {
        OponentName.Text = name;
    }

    public void SetPlayerTexture(Texture texture)
    {
        PlayerTexture.Texture = texture;
    }

    public void SetOponentTexture(Texture texture)
    {
        OpponentTexture.Texture = texture;
    }

    public void SetBidTexture(Texture texture)
    {
        BidTextureRect.Texture = texture;
    }

    public void SetBidValue(string value)
    {
        BidValue.Text = value;
    }

    public void OnDataReceived(object sender, object args)
    {
        if (args is Dictionary dict)
            GD.Print(dict);
        WaitingForDataContainer.Visible = false;
        RoomContainer.Visible = true;
    }

    public void OnClose()
    {
        Visible = false;
        EventRegistry.GetEventPublisher("OnDisconnectFromLobby").RaiseEvent(this);
    }
}

using Godot;

public class RoomPopup : Panel
{
    [Export] private StyleBoxTexture NotReadyButtonBackground;
    [Export] private StyleBoxTexture ReadyButtonBackground;
    [Export] private Texture WhiteCheckersTexture;
    [Export] private Texture BlackCheckersTexture;
    private Label PlayerName;
    private Label OponentName;
    private TextureRect PlayerTexture;
    private TextureRect OpponentTexture;
    private Button ReadyButton;
    private Button OpponentReadyButton;
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
        BidValue = GetNode<Label>("%BidValue");
        PlayerTexture = GetNode<TextureRect>("%PlayerTexture");
        OpponentTexture = GetNode<TextureRect>("%OpponentTexture");
        ReadyButton = GetNode<Button>("%PlayerReadyButton");
        OpponentReadyButton = GetNode<Button>("%OpponentReadyButton");
        RoomContainer = GetNode<PanelContainer>("%RoomContainer");
        WaitingForDataContainer = GetNode<PanelContainer>("%WaitingForDataContainer");

        EventSubscriber.SubscribeToEvent("OnDataReceived", OnDataReceived);
        EventSubscriber.SubscribeToEvent("OnPairedReceived", OnPairedReceived);
        EventSubscriber.SubscribeToEvent("OnOpponentReadyReceived", OnOpponentReadyReceived);

        WaitingForDataContainer.Visible = true;
        RoomContainer.Visible = false;
    }

    public void OnReadyButtonPressed(bool buttonPressed)
    {
        ReadyButton.AddStyleboxOverride("normal", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);
        ReadyButton.AddStyleboxOverride("hover", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);
        ReadyButton.AddStyleboxOverride("pressed", buttonPressed ? ReadyButtonBackground : NotReadyButtonBackground);

        EventRegistry.GetEventPublisher("OnReadyButtonPressed").RaiseEvent(buttonPressed);

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

    public void OnPairedReceived(object sender, object args)
    {
        if (args is PairedValue pairedValue)
        {
            GameState.currentGameColor = pairedValue.color == 0 ? BoardColors.Black : BoardColors.White;
            GameState.opponentName = pairedValue.opponent;
            GameState.room_id = pairedValue.room_id;

            SetPlayerName(GameState.playerName);
            SetOponentName(GameState.opponentName);
            SetPlayerTexture(GameState.currentGameColor == BoardColors.White ? WhiteCheckersTexture : BlackCheckersTexture);
            SetOponentTexture(GameState.currentGameColor == BoardColors.White ? BlackCheckersTexture : WhiteCheckersTexture);
            SetBidValue(GameState.betValue.ToString());
        }
        WaitingForDataContainer.Visible = false;
        RoomContainer.Visible = true;
    }
    public void OnOpponentReadyReceived(object sender, object args)
    {
        if (args is OpponentReady opponentReady)
            OpponentReadyButton.AddStyleboxOverride("disabled", opponentReady.is_ready ? ReadyButtonBackground : NotReadyButtonBackground);
    }
    public void OnDataReceived(object sender, object args)
    {
        if (args is LobbyInfo lobbyInfo)
        {
            SetPlayerName(lobbyInfo.name);
            SetBidValue(lobbyInfo.selected_bid.ToString());
            SetPlayerTexture(WhiteCheckersTexture);
        }
        WaitingForDataContainer.Visible = false;
        RoomContainer.Visible = true;
    }

    public void OnClose()
    {
        WaitingForDataContainer.Visible = true;
        RoomContainer.Visible = false;
        Visible = false;
        EventRegistry.GetEventPublisher("OnDisconnectFromLobby").RaiseEvent(this);
    }

    public override void _ExitTree()
    {
        EventSubscriber.UnsubscribeFromEvent("OnDataReceived", OnDataReceived);
        EventSubscriber.UnsubscribeFromEvent("OnPairedReceived", OnPairedReceived);
        EventSubscriber.UnsubscribeFromEvent("OnOpponentReadyReceived", OnOpponentReadyReceived);
    }
}

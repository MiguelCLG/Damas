using System;
using Godot;

public class RoomPopup : Panel
{
	[Export] private StyleBoxTexture NotReadyButtonBackground;
	[Export] private StyleBoxTexture ReadyButtonBackground;
	[Export] private Texture WhiteCheckersTexture;
	[Export] private Texture BlackCheckersTexture;
	[Export] AudioOptionsResource clickSound;
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

		SubscribeToEvents();

		WaitingForDataContainer.Visible = true;
		RoomContainer.Visible = false;
	}

	public void SetWaitingContainerVisible(bool isVisible)
	{
		ChangeReadyButton(false);
		OpponentReadyButton.AddStyleboxOverride("disabled", NotReadyButtonBackground);
		ReadyButton.SetPressedNoSignal(false);
		OpponentReadyButton.SetPressedNoSignal(false);
		WaitingForDataContainer.Visible = isVisible;
		RoomContainer.Visible = !isVisible;
	}
	public void OnReadyButtonPressed(bool buttonPressed)
	{
		ChangeReadyButton(buttonPressed);

		EventRegistry.GetEventPublisher("OnReadyButtonPressed").RaiseEvent(buttonPressed);

	}

	public void ChangeReadyButton(bool isReady)
	{
		ReadyButton.AddStyleboxOverride("normal", isReady ? ReadyButtonBackground : NotReadyButtonBackground);
		ReadyButton.AddStyleboxOverride("hover", isReady ? ReadyButtonBackground : NotReadyButtonBackground);
		ReadyButton.AddStyleboxOverride("pressed", isReady ? ReadyButtonBackground : NotReadyButtonBackground);
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
			GameState.currentGameColor = pairedValue.color == 0 ? BoardColors.White : BoardColors.Black;
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
		GetNode<AudioManager>("/root/AudioManager")?.Play(clickSound, this);
		OpponentReadyButton.AddStyleboxOverride("disabled", NotReadyButtonBackground);

		if (WaitingForDataContainer.Visible == true || RoomContainer.Visible == false)
		{
			Visible = false;
			ChangeReadyButton(false); // return ready button to false
			EventRegistry.GetEventPublisher("OnDisconnectFromQueue").RaiseEvent(this);
		}
		else
		{
			ChangeReadyButton(false); // return ready button to false
			WaitingForDataContainer.Visible = true;
			RoomContainer.Visible = false;
			Visible = false;
			if (ReadyButton.Pressed)
				EventRegistry.GetEventPublisher("OnReadyButtonPressed").RaiseEvent(false);
			EventRegistry.GetEventPublisher("OnDisconnectFromLobby").RaiseEvent(this);
		}
		ReadyButton.SetPressedNoSignal(false);
		OpponentReadyButton.SetPressedNoSignal(false);

	}

	private void SubscribeToEvents()
	{
		EventSubscriber.SubscribeToEvent("OnDataReceived", OnDataReceived);
		EventSubscriber.SubscribeToEvent("OnPairedReceived", OnPairedReceived);
		EventSubscriber.SubscribeToEvent("OnOpponentReadyReceived", OnOpponentReadyReceived);
	}
	public override void _ExitTree()
	{
		EventSubscriber.UnsubscribeFromEvent("OnDataReceived", OnDataReceived);
		EventSubscriber.UnsubscribeFromEvent("OnPairedReceived", OnPairedReceived);
		EventSubscriber.UnsubscribeFromEvent("OnOpponentReadyReceived", OnOpponentReadyReceived);
	}
}

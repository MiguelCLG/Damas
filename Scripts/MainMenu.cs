using System;
using Godot;
using Godot.Collections;
using static GameState;
using static Utils;
public partial class MainMenu : Control
{
	[Export] Array<Texture> bidTextures;
	[Export] AudioOptionsResource clickSound;
	[Export] AudioOptionsResource music;
	TextureRect BidTexture;
	private SceneLoaders sceneLoaders; // autoload to handle transitions and switching scenes
	private Label PlayerName;
	private Label PlayerMoney;
	private Label WaitingQueueLabel;
	private RoomPopup roomPopup;
	private Panel LoadingMenu;
	private Panel OptionsMenu;
	private AudioManager audioManager;
	// Panel roomList;


	public override void _Ready()
	{
		GetTree().Paused = false;
		ViewportBaseX = GetViewportRect().Size.x;
		ViewportBaseY = GetViewportRect().Size.y;
		sceneLoaders = GetNode<SceneLoaders>("/root/SceneLoaders");
		roomPopup = GetNode<RoomPopup>("%RoomPopup");

		LoadingMenu = GetNode<Panel>("%LoadingMenu");
		OptionsMenu = GetNode<Panel>("%OptionsMenu");

		audioManager = GetNode<AudioManager>("/root/AudioManager");
		audioManager?.Play(music, this);
		//  roomList = GetNode<Panel>("%RoomList");
		PlayerName = GetNode<Label>("%PlayerName");
		PlayerMoney = GetNode<Label>("%PlayerMoney");
		WaitingQueueLabel = GetNode<Label>("%WaitingQueueLabel");
		BidTexture = GetNode<TextureRect>("%BidTexture");

		SubscribeToEvents();

		if (playerInfo != null) // in case the player is already connected
		{
			SetPlayerName(playerInfo.player_name);
			SetPlayerMoney(playerInfo.money.ToString());
			OnConnectionStart();
		}
	}

	private void PlayerConnected(object sender, object args)
	{
		if (args is PlayerInfo playerInfo)
		{
			SetPlayerName(playerInfo.player_name);
			SetPlayerMoney(playerInfo.money.ToString());
			OnConnectionStart();
		}
	}

	private void OnGameStarting(object sender, object args)
	{
		if (args is GameStartMessage message)
		{
			initialBoard = message.Board;
			MaxTimer = message.max_timer;
			foreach (var gamePlayer in message.GamePlayers)
			{
				if (playerName == gamePlayer.name)
				{
					player = gamePlayer;
					currentGameColor = gamePlayer.color == "b" ? BoardColors.Black : BoardColors.White;
				}
				else
				{
					opponentName = gamePlayer.name;
				}
			}
		}
		LoadNextScene();

	}

	private void LoadNextScene()
	{
		sceneLoaders.NextScene();
	}

	public void OnArrowClicked(int direction)
	{ // 0 - left; 1 - right
		audioManager?.Play(clickSound, this);
		if (direction == 0)
		{
			var currentTexture = BidTexture.Texture;
			var index = bidTextures.IndexOf(currentTexture);
			if (index == 0) index = bidTextures.Count;
			BidTexture.Texture = bidTextures[index - 1];
			betValue = availableBidValues[index - 1];
		}
		else
		{
			var currentTexture = BidTexture.Texture;
			var index = bidTextures.IndexOf(currentTexture);
			if (index == bidTextures.Count - 1) index = -1;
			BidTexture.Texture = bidTextures[index + 1];
			betValue = availableBidValues[index + 1];
		}
	}
	public void SetPlayerName(string newPlayerName)
	{
		PlayerName.Text = newPlayerName;
	}
	public void SetPlayerMoney(string newPlayerMoney)
	{
		PlayerMoney.Text = newPlayerMoney;
	}

	public void SetWaitingQueue(object sender, object args)
	{
		if (args is string numberOfPlayers)
			WaitingQueueLabel.Text = $"{numberOfPlayers} jogadores na fila.";
	}
	public void ShowRoom(object sender, object args)
	{
		roomPopup.SetBidTexture(BidTexture.Texture);
		roomPopup.Visible = true;
	}
	public void HideRoom()
	{
		roomPopup.Visible = false;
	}

	public void SetWaitingContainerVisible(object sender, object args)
	{
		if (args is bool isVisible)
			roomPopup.SetWaitingContainerVisible(isVisible);
	}

	public async void OnConnectionStart()
	{
		AnimationPlayer loadingAnimationPlayer = LoadingMenu.GetNode<AnimationPlayer>("%LoadingAnimationPlayer");
		loadingAnimationPlayer.Play("fade_out");
		await ToSignal(loadingAnimationPlayer, "animation_finished");
		LoadingMenu.Visible = false;
		LoadingMenu.Modulate = new Color(1, 1, 1, 1);
	}

	public void OnStartGame()
	{
		audioManager?.Play(clickSound, this);
		EventRegistry.GetEventPublisher("OnJoinRoom").RaiseEvent(betValue);
	}

	public void OnOptionsMenuButtonPressed()
	{
		OptionsMenu.Visible = true;
	}

	private void SubscribeToEvents()
	{
		EventSubscriber.SubscribeToEvent("SetWaitingContainerVisible", SetWaitingContainerVisible);
		EventSubscriber.SubscribeToEvent("OnGameStarting", OnGameStarting);
		EventSubscriber.SubscribeToEvent("SetWaitingQueue", SetWaitingQueue);
		EventSubscriber.SubscribeToEvent("ShowRoom", ShowRoom);
		EventSubscriber.SubscribeToEvent("PlayerConnected", PlayerConnected);
		EventSubscriber.SubscribeToEvent("OnGameReconnect", OnGameStarting);
	}

	public override void _ExitTree()
	{
		audioManager?.StopSound(this);
		EventSubscriber.UnsubscribeFromEvent("OnGameReconnect", OnGameStarting);
		EventSubscriber.UnsubscribeFromEvent("SetWaitingContainerVisible", SetWaitingContainerVisible);
		EventSubscriber.UnsubscribeFromEvent("PlayerConnected", PlayerConnected);
		EventSubscriber.UnsubscribeFromEvent("ShowRoom", ShowRoom);
		EventSubscriber.UnsubscribeFromEvent("SetWaitingQueue", SetWaitingQueue);
		EventSubscriber.UnsubscribeFromEvent("OnGameStarting", OnGameStarting);
	}
}





/* public void ShowRoomList()
  {
	roomList.Visible = true;
  }
  public void HideRoomList()
  {
	roomList.Visible = false;
  } */
/* public void OnSalasClick()
{
  EventRegistry.GetEventPublisher("OnRoomList").RaiseEvent(this);
} */

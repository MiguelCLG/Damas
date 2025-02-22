using System;
using Godot;
using Godot.Collections;
using static GameState;
using static Utils;
public partial class MainMenu : Control
{
  [Export] Array<Texture> bidTextures;
  TextureRect BidTexture;
  private SceneLoaders sceneLoaders; // autoload to handle transitions and switching scenes
  private Label PlayerName;
  private Label PlayerMoney;
  private RichTextLabel WaitingQueueLabel;
  RoomPopup roomPopup;
  // Panel roomList;


  public override void _Ready()
  {
    ViewportBaseX = GetViewportRect().Size.x;
    ViewportBaseY = GetViewportRect().Size.y;
    sceneLoaders = GetNode<SceneLoaders>("/root/SceneLoaders");
    roomPopup = GetNode<RoomPopup>("%RoomPopup");
    //  roomList = GetNode<Panel>("%RoomList");
    PlayerName = GetNode<Label>("%PlayerName");
    PlayerMoney = GetNode<Label>("%PlayerMoney");
    WaitingQueueLabel = GetNode<RichTextLabel>("%WaitingQueueLabel");
    BidTexture = GetNode<TextureRect>("%BidTexture");

    EventSubscriber.SubscribeToEvent("OnGameStarting", OnGameStarting);
  }

  private void OnGameStarting(object sender, object args)
  {
    if (args is GameStartMessage message)
    {
      initialBoard = message.Board;
      playerName = message.CurrentPlayerID;
      foreach (var player in message.GamePlayers)
      {
        if (player.id == message.CurrentPlayerID)
        {
          playerName = player.name;
          GameState.player = player;
        }
        else
        {
          opponentName = player.name;
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

  public void SetWaitingQueue(string numberOfPlayers)
  {
    WaitingQueueLabel.Text = $"{numberOfPlayers} jogadores na fila.";
  }
  public void ShowRoom()
  {
    roomPopup.SetBidTexture(BidTexture.Texture);
    roomPopup.Visible = true;
  }
  public void HideRoom()
  {
    roomPopup.Visible = false;
  }

  public void OnStartGame()
  {
    EventRegistry.GetEventPublisher("OnJoinRoom").RaiseEvent(betValue);
  }

  public override void _ExitTree()
  {
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

using Godot;
using System;

public partial class MainMenu : Control
{
  private SceneLoaders sceneLoaders; // autoload to handle transitions and switching scenes
  private Label PlayerName;
  private Label PlayerMoney;
  private RichTextLabel WaitingQueueLabel;
  RoomPopup roomPopup;
  Panel roomList;


  public override void _Ready()
  {
    Utils.ViewportBaseX = GetViewportRect().Size.x;
    Utils.ViewportBaseY = GetViewportRect().Size.y;
    sceneLoaders = GetNode<SceneLoaders>("/root/SceneLoaders");
    roomPopup = GetNode<RoomPopup>("%RoomPopup");
    roomList = GetNode<Panel>("%RoomList");
    PlayerName = GetNode<Label>("%PlayerName");
    PlayerMoney = GetNode<Label>("%PlayerMoney");
    WaitingQueueLabel = GetNode<RichTextLabel>("%WaitingQueueLabel");
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
    roomPopup.Visible = true;
  }
  public void HideRoom()
  {
    roomPopup.Visible = false;
  }
  public void ShowRoomList()
  {
    roomList.Visible = true;
  }
  public void HideRoomList()
  {
    roomList.Visible = false;
  }
  public void OnSalasClick()
  {
    EventRegistry.GetEventPublisher("RoomList").RaiseEvent(this);
  }
  public void OnStartGame()
  {
    EventRegistry.GetEventPublisher("JoinQueue").RaiseEvent(this);
    //sceneLoaders.NextScene();
  }
}

using System.Text;
using Godot;
using Godot.Collections;
public partial class MultiplayerPeerConnection : Node
{
  WebSocketClient client;
  MainMenu mainMenu;
  RoomPopup roomPopup;

  public override void _Ready()
  {
    mainMenu = GetNode<MainMenu>("/root/MainMenu");
    roomPopup = mainMenu.GetNode<RoomPopup>("%RoomPopup");
    EventRegistry.RegisterEvent("JoinQueue");
    EventRegistry.RegisterEvent("OnDisconnectFromLobby");
    EventRegistry.RegisterEvent("OnDataReceived");

    client = new WebSocketClient();
    client.Connect("connection_established", this, "OnConnect");
    client.Connect("connection_closed", this, "OnConnectionClosed");
    client.Connect("connection_error", this, "OnConnectionClosed");
    client.Connect("data_received", this, "OnData");
    EventSubscriber.SubscribeToEvent("JoinQueue", OnJoinQueue);
    EventSubscriber.SubscribeToEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);

    var err = client.ConnectToUrl("ws://localhost:8080/ws");
    if (err != Error.Ok)
    {
      GD.Print("Unable To Connect: " + err);
      SetProcess(false);
    }
  }
  public override void _Process(float delta)
  {
    client.Poll();
  }

  public void OnConnectionClosed()
  {
    GD.Print("Connection Closed.");
  }

  public void OnConnect(string protocol)
  {

    GD.Print("Connection established: " + protocol);

  }
  public void OnData()
  {

    string jsonString = client.GetPeer(1).GetPacket().GetStringFromUTF8();
    JSONParseResult parsedObject = JSON.Parse(jsonString);
    GD.Print($"Data Received: {jsonString}");
    if (parsedObject.Result is Dictionary)
    {
      Dictionary dict = (Dictionary)parsedObject.Result;
      string commandStr = (string)dict["command"];
      Commands command = (Commands)System.Enum.Parse(typeof(Commands), commandStr);

      // Create the DataReceived object

      switch (command)
      {
        default: break;
        case Commands.connected:
          Connected(dict);

          break;
        case Commands.join_queue:
          EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(dict);
          break;
        case Commands.leave_queue:
          break;
        case Commands.send_message:
          break;
        case Commands.custom_command:
          break;
        case Commands.paired:
          break;
        case Commands.update_waiting_queue:
          UpdateWaitingQueue(dict);
          break;
        case Commands.move_piece:
          break;
      }
    }
    else if (parsedObject.Result is Array)
    {
      Array array = (Array)parsedObject.Result;
      GD.Print(array[0]); // Access data using index
    }

    void UpdateWaitingQueue(Dictionary dict)
    {
      if (dict.Contains("value") && dict["value"] is Dictionary queueSizeInfo)
      {
        var valueToSend = queueSizeInfo["waiting_queue_size"];
        mainMenu.SetWaitingQueue(valueToSend.ToString());
      }
    }
    ;
  }

  private void Connected(Dictionary dict)
  {
    Dictionary playerInfoDict = null;
    if (dict.Contains("value") && dict["value"] is Dictionary)
    {
      playerInfoDict = (Dictionary)dict["value"];
    }
    PlayerInfo playerInfo = null;
    if (playerInfoDict != null)
    {
      playerInfo = new PlayerInfo
      {
        player_name = (string)playerInfoDict["player_name"],
        money = (float)playerInfoDict["money"]
      };
    }
    mainMenu.SetPlayerName(playerInfo.player_name);
    mainMenu.SetPlayerMoney(playerInfo.money.ToString());
  }

  public void OnDisconnectFromLobby(object sender, object args)
  {
    Dictionary dict = new Dictionary();
    dict["command"] = Commands.leave_queue.ToString();
    string jsonString = JSON.Print(dict);
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);

    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Binary);
    client.GetPeer(1).PutPacket(encodedMessage);

  }
  public void OnJoinQueue(object sender, object args)
  {

    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Text);

    Dictionary dict = new Dictionary();
    dict["command"] = Commands.join_queue.ToString();
    dict["value"] = 2;
    string jsonString = JSON.Print(dict);
    GD.Print($"Sending: {jsonString}");
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);

    client.GetPeer(1).PutPacket(encodedMessage);
    roomPopup.Visible = true;
  }

  public override void _ExitTree()
  {
    client.DisconnectFromHost(200, "Exited the game, ending connection.");
    EventSubscriber.UnsubscribeFromEvent("JoinQueue", OnJoinQueue);
    EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
    EventRegistry.UnregisterEvent("OnDataReceived");
    EventRegistry.UnregisterEvent("OnDisconnectFromLobby");
    EventRegistry.UnregisterEvent("OnDataReceived");
  }
}

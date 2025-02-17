
using Newtonsoft.Json;
using System.Text;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
public partial class MultiplayerPeerConnection : Node
{
  WebSocketClient client;
  MainMenu mainMenu;

  public override void _Ready()
  {
    mainMenu = GetNode<MainMenu>("/root/MainMenu");
    EventRegistry.RegisterEvent("OnJoinRoom");
    EventRegistry.RegisterEvent("OnRoomCheck");
    EventRegistry.RegisterEvent("OnDisconnectFromLobby");
    EventRegistry.RegisterEvent("OnDataReceived");
    EventRegistry.RegisterEvent("OnPairedReceived");

    client = new WebSocketClient();
    client.Connect("connection_established", this, "OnConnect");
    client.Connect("connection_closed", this, "OnConnectionClosed");
    client.Connect("connection_error", this, "OnConnectionClosed");
    client.Connect("data_received", this, "OnData");
    EventSubscriber.SubscribeToEvent("OnJoinRoom", OnJoinRoom);
    EventSubscriber.SubscribeToEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);

    var err = client.ConnectToUrl("ws://localhost:8080/ws?token=token456&sessionid=session2&currency=USD");
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

  public void OnConnectionClosed(bool was_clean_close = false)
  {
    GD.Print("Connection Closed.", was_clean_close);
  }

  public void OnConnect(string protocol)
  {
    GD.Print("Connection established: " + protocol);
  }
  public void OnData()
  {
    string jsonString = client.GetPeer(1).GetPacket().GetStringFromUTF8();

    var parsedObject = JsonConvert.DeserializeObject<DataReceived<JToken>>(jsonString);

    GD.Print($"Data Received: {jsonString}");
    GD.Print($"Parsed Object Received: {parsedObject}");

    try
    {
      switch (parsedObject.command)
      {
        default: break;
        case Commands.connected:
          ConnectedCommandReceived(parsedObject.value.ToObject<PlayerInfo>());
          break;
        case Commands.create_room:
          EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(parsedObject.value.ToObject<LobbyInfo>());
          break;
        case Commands.room_created:
          RoomCreatedCommandReceived(parsedObject.value.ToObject<LobbyInfo>());
          break;
        case Commands.leave_room:
          break;
        case Commands.paired:
          PairCommandReceived(parsedObject.value.ToObject<PairedValue>());
          break;
        case Commands.ready_room:
          break;
        case Commands.join_room:
          break;
        case Commands.game_info:
          break;
        case Commands.queue_confirmation:
          // vem um boolean que diz se conseguiu entrar na fila
          break;
      }
    }
    catch (System.Exception e)
    {
      GD.PrintErr("[Server Command Parser] - " + e.Message);
    }

  }

  private void PairCommandReceived(PairedValue dict)
  {
    EventRegistry.GetEventPublisher("OnPairedReceived").RaiseEvent(dict);
  }

  private void RoomCreatedCommandReceived(LobbyInfo dict)
  {
    {
      EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(dict);
    }
  }

  private void ConnectedCommandReceived(PlayerInfo playerInfo)
  {
    mainMenu.SetPlayerName(playerInfo.player_name);
    mainMenu.SetPlayerMoney(playerInfo.money.ToString());
  }

  public void OnDisconnectFromLobby(object sender, object args)
  {
    if (!client.GetPeer(1).IsConnectedToHost())
    {
      return;
    }
    Dictionary dict = new Dictionary();
    dict["command"] = Commands.leave_room.ToString();
    string jsonString = JSON.Print(dict);
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);

    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Binary);
    client.GetPeer(1).PutPacket(encodedMessage);
  }


  public void OnJoinRoom(object sender, object args)
  {
    mainMenu.HideRoomList();
    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Text);

    Dictionary dict = new Dictionary();
    dict["command"] = Commands.queue.ToString();
    dict["value"] = 1.5f;
    string jsonString = JSON.Print(dict);
    GD.Print($"Sending: {jsonString}");
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);

    client.GetPeer(1).PutPacket(encodedMessage);
    mainMenu.ShowRoom();
  }

  public override void _ExitTree()
  {
    client.DisconnectFromHost(200, "Exited the game, ending connection.");
    EventSubscriber.UnsubscribeFromEvent("OnJoinRoom", OnJoinRoom);
    EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
    EventRegistry.UnregisterEvent("OnPairedReceived");
    EventRegistry.UnregisterEvent("OnRoomCheck");
    EventRegistry.UnregisterEvent("OnDisconnectFromLobby");
    EventRegistry.UnregisterEvent("OnDataReceived");
  }
}





/*

public void OnJoinQueue(object sender, object args)
  {
    if (!client.GetPeer(1).IsConnectedToHost())
    {
      return;
    }
    Dictionary dict = new Dictionary();
    dict["command"] = Commands.join_room.ToString();
    string jsonString = JSON.Print(dict);
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);
    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Binary);
    client.GetPeer(1).PutPacket(encodedMessage);
  }


  public void OnRoomList(object sender, object args)
  {
    mainMenu.HideRoom();
    mainMenu.ShowRoomList();

  }

  private void GetRoomList(Dictionary dict)
  {
    RoomInfoList roomInfoList = JsonConvert.DeserializeObject<RoomInfoList>(dict.ToString());
    EventRegistry.GetEventPublisher("OnRoomCheck").RaiseEvent(roomInfoList);

  }
*/

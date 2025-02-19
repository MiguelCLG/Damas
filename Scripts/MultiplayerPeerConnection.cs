
using Newtonsoft.Json;
using System.Text;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using static GameState;
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
    EventRegistry.RegisterEvent("OnReadyButtonPressed");
    EventRegistry.RegisterEvent("OnOpponentReadyReceived");

    client = new WebSocketClient();
    client.Connect("connection_established", this, "OnConnect");
    client.Connect("connection_closed", this, "OnConnectionClosed");
    client.Connect("connection_error", this, "OnConnectionClosed");
    client.Connect("data_received", this, "OnData");
    EventSubscriber.SubscribeToEvent("OnJoinRoom", OnJoinRoom);
    EventSubscriber.SubscribeToEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
    EventSubscriber.SubscribeToEvent("OnReadyButtonPressed", OnReadyButtonPressed);

    player = new GamePlayer();
    player.token = "token123";
    player.session_id = "session1";
    var err = client.ConnectToUrl($"ws://localhost:8080/ws?token={player.token}&sessionid={player.session_id}&currency=USD");
    //var err = client.ConnectToUrl("ws://localhost:8080/ws?token=token456&sessionid=session2&currency=USD");
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

    try
    {

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
          case Commands.room_created:
            EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(parsedObject.value.ToObject<LobbyInfo>());
            break;
          case Commands.leave_room:
            break;
          case Commands.paired:
            player.color = parsedObject.value.ToObject<PairedValue>().color == 0 ? BoardColors.Black.ToString() : BoardColors.White.ToString();
            EventRegistry.GetEventPublisher("OnPairedReceived").RaiseEvent(parsedObject.value.ToObject<PairedValue>());
            break;
          case Commands.join_room:
            break;
          case Commands.game_info:
            break;
          case Commands.game_starting:
            EventRegistry.GetEventPublisher("OnGameStarting").RaiseEvent(null);
            break;
          case Commands.queue_confirmation:
            // vem um boolean que diz se conseguiu entrar na fila
            break;
          case Commands.message:
            HandleServerMessage(parsedObject.value.ToObject<Message>());
            break;
          case Commands.opponent_ready:
            EventRegistry.GetEventPublisher("OnOpponentReadyReceived").RaiseEvent(parsedObject.value.ToObject<OpponentReady>());
            break;
        }
      }
      catch (Exception e)
      {
        GD.PrintErr("[Server Command Parser] - " + e.Message);
      }
    }
    catch (Exception e)
    {
      GD.PrintErr("[Server Data Parser] - " + e.Message);
    }

  }

  private void HandleServerMessage(Message message)
  {
    switch (message.message_type)
    {
      default: break;
      case MessageType.info:
        GD.Print($"INFO: {message.message}");
        break;
      case MessageType.warning:
        GD.Print($"WARN: {message.message}");
        break;
      case MessageType.error:
        GD.PrintErr($"ERROR: {message.message}");
        break;
    }

  }

  private void ConnectedCommandReceived(PlayerInfo playerInfo)
  {
    player.name = playerInfo.player_name;
    player.id = playerInfo.player_id;
    playerName = playerInfo.player_name;
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
    // mainMenu.HideRoomList();

    SendMessage(Commands.queue, args);
    mainMenu.ShowRoom();
  }

  private void OnReadyButtonPressed(object sender, object args)
  {
    SendMessage(Commands.ready_queue, args);
  }

  private void SendMessage(Commands command, object args)
  {
    Dictionary dict = new Dictionary();
    dict["command"] = command.ToString();
    dict["value"] = args;
    string jsonString = JSON.Print(dict);
    GD.Print($"Sending: {jsonString}");
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);
    client.GetPeer(1).PutPacket(encodedMessage);
  }
  public override void _ExitTree()
  {
    client.DisconnectFromHost(200, "Exited the game, ending connection.");
    EventSubscriber.UnsubscribeFromEvent("OnJoinRoom", OnJoinRoom);
    EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
    EventSubscriber.UnsubscribeFromEvent("OnReadyButtonPressed", OnReadyButtonPressed);
    EventRegistry.UnregisterEvent("OnOpponentReadyReceived");
    EventRegistry.UnregisterEvent("OnReadyButtonPressed");
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

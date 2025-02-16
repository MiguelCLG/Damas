
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;
public partial class MultiplayerPeerConnection : Node
{
  WebSocketClient client;
  MainMenu mainMenu;

  public override void _Ready()
  {
    mainMenu = GetNode<MainMenu>("/root/MainMenu");
    EventRegistry.RegisterEvent("JoinQueue");
    EventRegistry.RegisterEvent("RoomList");
    EventRegistry.RegisterEvent("OnRoomCheck");
    EventRegistry.RegisterEvent("OnDisconnectFromLobby");
    EventRegistry.RegisterEvent("OnDataReceived");

    client = new WebSocketClient();
    client.Connect("connection_established", this, "OnConnect");
    client.Connect("connection_closed", this, "OnConnectionClosed");
    client.Connect("connection_error", this, "OnConnectionClosed");
    client.Connect("data_received", this, "OnData");
    EventSubscriber.SubscribeToEvent("JoinQueue", OnJoinQueue);
    EventSubscriber.SubscribeToEvent("RoomList", OnRoomList);
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
    var cleanJson = Regex.Unescape(jsonString);
    JSONParseResult parsedObject = JSON.Parse(cleanJson.Remove(0, 1).Remove(cleanJson.Length - 1, 1));
    GD.Print($"Data Received: {cleanJson}");

    // what we receive is a dictionary in this format:
    // {
    //   "command": "connected",
    //   "value": {
    //            }
    //  }

    if (parsedObject.Result is Dictionary)
    {
      Dictionary dict = (Dictionary)parsedObject.Result;

      try
      {
        string commandStr = (string)dict["command"];
        Commands command = (Commands)System.Enum.Parse(typeof(Commands), commandStr);


        // Create the DataReceived object

        switch (command)
        {
          default: break;
          case Commands.connected:
            Connected(dict);
            break;
          case Commands.create_room:
            EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(dict);
            break;
          case Commands.room_created:
            RoomCreated(dict);
            break;
          case Commands.leave_queue:
            break;
          case Commands.send_message:
            break;
          case Commands.custom_command:
            break;
          case Commands.move_piece:
            break;
          case Commands.room_info:
            break;
          case Commands.game_info:
            GetRoomList(dict);
            break;
        }
      }
      catch (System.Exception e)
      {
        GD.PrintErr(e.Message);
      }
    }
    else if (parsedObject.Result is Array)
    {
      Array array = (Array)parsedObject.Result;
      GD.Print("hello", array[0]); // Access data using index
    }

  }

  private void RoomCreated(Dictionary dict)
  {
    if (dict.Contains("value") && dict["value"] is Dictionary data)
    {
      LobbyInfo lobbyInformation = new LobbyInfo();

      if (data != null)
      {
        lobbyInformation.id = data["id"].ToString();
        lobbyInformation.name = data["name"].ToString();
        lobbyInformation.currency = data["currency"].ToString();
        lobbyInformation.selected_bid = (float)data["selected_bid"];
      }
      EventRegistry.GetEventPublisher("OnDataReceived").RaiseEvent(lobbyInformation);
    }
  }
  private void GetRoomList(Dictionary dict)
  {
    if (dict.Contains("value") && dict["value"] is Dictionary data)
    {
      RoomInfoList roomInfoList = new RoomInfoList();
      Array<RoomInfo> roomsInformation = new Array<RoomInfo>();
      roomInfoList.players_waiting = (int)data["players_waiting"];
      Array<Dictionary> d = data["room_agregate"] as Array<Dictionary>;
      foreach (var room in d)
      {
        if (room == null)
          continue;
        RoomInfo ri = new RoomInfo();
        ri.agregate_value = (float)room["agregate_value"];
        ri.count = (int)room["count"];
        roomsInformation.Add(ri);
      }
      roomInfoList.room_aggregate = roomsInformation;

      EventRegistry.GetEventPublisher("OnRoomCheck").RaiseEvent(roomInfoList);
    }
  }

  void UpdateWaitingQueue(Dictionary dict)
  {
    if (dict.Contains("value") && dict["value"] is Dictionary queueSizeInfo)
    {
      var valueToSend = queueSizeInfo["waiting_queue_size"];
      mainMenu.SetWaitingQueue(valueToSend.ToString());
    }
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
    if (!client.GetPeer(1).IsConnectedToHost())
    {
      return;
    }
    Dictionary dict = new Dictionary();
    dict["command"] = Commands.leave_queue.ToString();
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

  public void OnJoinQueue(object sender, object args)
  {
    mainMenu.HideRoomList();
    client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Text);

    Dictionary dict = new Dictionary();
    dict["command"] = Commands.create_room.ToString();
    dict["value"] = 2f;
    string jsonString = JSON.Print(dict);
    GD.Print($"Sending: {jsonString}");
    byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);

    client.GetPeer(1).PutPacket(encodedMessage);
    mainMenu.ShowRoom();
  }

  public override void _ExitTree()
  {
    client.DisconnectFromHost(200, "Exited the game, ending connection.");
    EventSubscriber.UnsubscribeFromEvent("JoinQueue", OnJoinQueue);
    EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
    EventSubscriber.UnsubscribeFromEvent("RoomList", OnRoomList);

    EventRegistry.UnregisterEvent("RoomList");
    EventRegistry.UnregisterEvent("OnRoomCheck");
    EventRegistry.UnregisterEvent("OnDisconnectFromLobby");
    EventRegistry.UnregisterEvent("OnDataReceived");
  }
}

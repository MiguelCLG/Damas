using System.Text;
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
    client = new WebSocketClient();
    client.Connect("connection_established", this, "OnConnect");
    client.Connect("connection_closed", this, "OnConnectionClosed");
    client.Connect("connection_error", this, "OnConnectionClosed");
    client.Connect("data_received", this, "OnData");
    EventSubscriber.SubscribeToEvent("JoinQueue", OnJoinQueue);

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
          UpdateWaitingQueue();
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

    void UpdateWaitingQueue()
    {
      GD.Print(" UPDATING QUEUE ");
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
  }
}

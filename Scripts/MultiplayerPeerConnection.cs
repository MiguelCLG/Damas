
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
	Timer reconnectTimer;

	public override void _Ready()
	{
		RegisterEvents();
		client = new WebSocketClient();
		client.Connect("connection_established", this, "OnConnect");
		client.Connect("connection_closed", this, "OnConnectionClosed");
		client.Connect("connection_error", this, "OnConnectionClosed");
		client.Connect("data_received", this, "OnData");
		SubscribeToEvents();

		player = new GamePlayer();

		// Set up the reconnection timer
		reconnectTimer = new Timer();
		reconnectTimer.WaitTime = 5f;
		reconnectTimer.OneShot = false;  // Make it repeat every 5 seconds
		AddChild(reconnectTimer);
		reconnectTimer.Connect("timeout", this, nameof(TryReconnect));

		ConnectToServer();
	}

	private void SendMessageEvent(object sender, object args)
	{
		if (args is MovePieceData data)
		{
			SendMessageComplex(Commands.move_piece, data);
		}
	}

	public override void _Process(float delta)
	{
		client.Poll();
	}

	private void ConnectToServer()
	{
		UrlParamsModel parameters = WebUtils.GetUrlParamsModel();
		if (Enum.TryParse(parameters.Currency, out Utils.Currency currency))
			Currency = currency;
		var port = "";
		//var ip = "staging.retromindgames.pt";
		var ip = "localhost";

		//var err = client.ConnectToUrl($"wss://{ip}{port}/ws?token={parameters.Token}&sessionid={parameters.SessionId}&currency={parameters.Currency}");
		var err = client.ConnectToUrl($"ws://{ip}{port}/ws?token={parameters.Token}&sessionid={parameters.SessionId}&currency={parameters.Currency}");
		//var err = client.ConnectToUrl($"wss://localhost:80/ws?token={parameters.Token}&sessionid={parameters.SessionId}&currency={parameters.Currency}");
		if (err != Error.Ok)
		{
			GD.Print("Unable To Connect: " + err);
			SetProcess(false);
		}
		else
		{
			GD.Print("Connecting to server...");
		}
	}

	public void OnConnectionClosed(bool was_clean_close = false)
	{
		GD.Print("Connection Closed.", was_clean_close);
		GD.Print("Attempting to reconnect in 5 seconds...");
		//TODO: show reconnection popup
		EventRegistry.GetEventPublisher("ToggleReconnectPopup").RaiseEvent(true);
		reconnectTimer.Start();
	}

	private void TryReconnect()
	{
		if (client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
		{
			GD.Print("Reconnecting...");
			ConnectToServer();
		}
	}

	public void OnConnect(string protocol)
	{
		GD.Print("Connection established: " + protocol);
		// Stop the reconnection attempts if successfully connected
		//TODO: hide reconnection popup
		EventRegistry.GetEventPublisher("ToggleReconnectPopup").RaiseEvent(false);
		reconnectTimer.Stop();
	}
	public void OnData()
	{
		string jsonString = client.GetPeer(1).GetPacket().GetStringFromUTF8();

		try
		{

			var parsedObject = JsonConvert.DeserializeObject<DataReceived<JToken>>(jsonString.Trim());
			GD.Print($"Data Received: {jsonString}");

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
						player.color = parsedObject.value.ToObject<PairedValue>().color == 0 ? BoardColors.White.ToString() : BoardColors.Black.ToString();
						EventRegistry.GetEventPublisher("OnPairedReceived").RaiseEvent(parsedObject.value.ToObject<PairedValue>());
						break;
					case Commands.join_room:
						break;
					case Commands.ready_queue:
						break;
					case Commands.game_info:
						GameInfo gameInfo = parsedObject.value.ToObject<GameInfo>();
						AddPlayerCountPerBet(gameInfo.player_count_per_bet_value);
						var playerCountForThisBet = GetPlayerCountForBet(betValue);
						EventRegistry.GetEventPublisher("SetWaitingQueue").RaiseEvent(playerCountForThisBet);
						break;
					case Commands.game_start:
						EventRegistry.GetEventPublisher("OnGameStarting").RaiseEvent(parsedObject.value.ToObject<GameStartMessage>());
						break;
					case Commands.queue_confirmation:
						// vem um boolean que diz se conseguiu entrar na fila
						EventRegistry.GetEventPublisher("OnQueueConfirmation").RaiseEvent(parsedObject.value.ToObject<bool>());
						break;
					case Commands.opponent_left_room:
						EventRegistry.GetEventPublisher("SetWaitingContainerVisible").RaiseEvent(true);
						break;
					case Commands.message:
						HandleServerMessage(parsedObject.value.ToObject<Message>());
						break;
					case Commands.opponent_ready:
						EventRegistry.GetEventPublisher("OnOpponentReadyReceived").RaiseEvent(parsedObject.value.ToObject<OpponentReady>());
						break;
					case Commands.move_piece:
						EventRegistry.GetEventPublisher("OnMovePiece").RaiseEvent(parsedObject.value.ToObject<MovePieceData>());
						break;
					case Commands.invalid_move:
						EventRegistry.GetEventPublisher("OnInvalidMove").RaiseEvent(null);
						break;
					case Commands.game_timer:
						EventRegistry.GetEventPublisher("OnTimerUpdate").RaiseEvent(parsedObject.value.ToObject<GameTimer>());
						break;
					case Commands.balance_update:
						playerInfo.money = (float)parsedObject.value;
						break;
					case Commands.turn_switch:
						EventRegistry.GetEventPublisher("OnTurnSwitch").RaiseEvent(parsedObject.value.ToString());
						break;
					case Commands.game_over:
						EventRegistry.GetEventPublisher("OnGameOver").RaiseEvent(parsedObject.value.ToObject<GameOver>());
						break;
					case Commands.opponent_disconnected_game:
						EventRegistry.GetEventPublisher("OnOpponentDisconnectedGame").RaiseEvent(parsedObject.value.ToString());
						break;
					case Commands.game_reconnect:
						EventRegistry.GetEventPublisher("OnGameReconnect").RaiseEvent(parsedObject.value.ToObject<GameStartMessage>());
						break;
				}
			}
			catch (Exception e)
			{
				GD.PrintErr("[Server Command Parser] - " + e.Message + " - " + jsonString);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("[Server Data Parser] - " + e.Message + " - " + jsonString);
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
		GameState.playerInfo = playerInfo;

		EventRegistry.GetEventPublisher("PlayerConnected").RaiseEvent(playerInfo);
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
		GD.Print("Sending LEAVE_ROOM");

		client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Binary);
		client.GetPeer(1).PutPacket(encodedMessage);
	}
	public void OnDisconnectFromQueue(object sender, object args)
	{
		if (!client.GetPeer(1).IsConnectedToHost())
		{
			return;
		}
		Dictionary dict = new Dictionary();
		dict["command"] = Commands.leave_queue.ToString();
		string jsonString = JSON.Print(dict);
		byte[] encodedMessage = Encoding.ASCII.GetBytes(jsonString);
		GD.Print("Sending LEAVE_QUEUE");
		client.GetPeer(1).SetWriteMode(WebSocketPeer.WriteMode.Binary);
		client.GetPeer(1).PutPacket(encodedMessage);
	}


	public void OnJoinRoom(object sender, object args)
	{
		SendMessage(Commands.queue, args);
		EventRegistry.GetEventPublisher("ShowRoom").RaiseEvent(this);
	}

	private void OnReadyButtonPressed(object sender, object args)
	{
		AudioOptionsResource clickSound = ResourceLoader.Load<AudioOptionsResource>("res://Resources/Sounds/click_hover.tres");
		GetNode<AudioManager>("/root/AudioManager")?.Play(clickSound, this);
		SendMessage(Commands.ready_queue, args);
	}
	private void OnConcede(object sender, object args)
	{
		SendMessage(Commands.leave_game, args);
	}

	private void SendMessage(Commands command, object args)
	{
		Dictionary dict = new Dictionary();
		dict["command"] = command.ToString();
		dict["value"] = args;
		var conversion = JsonConvert.SerializeObject(dict);
		string jsonString = JSON.Print(dict);
		GD.Print($"Sending: {conversion}");
		byte[] encodedMessage = Encoding.ASCII.GetBytes(conversion);
		client.GetPeer(1).PutPacket(encodedMessage);
	}
	private void SendMessageComplex(Commands command, MovePieceData args)
	{
		DataReceived<JToken> dict = new DataReceived<JToken>();
		dict.command = command;
		dict.value = JToken.FromObject(args);
		var novo = new { command = command.ToString(), value = dict.value };
		var conversion = JsonConvert.SerializeObject(novo);
		GD.Print($"Sending: {conversion}");
		byte[] encodedMessage = Encoding.ASCII.GetBytes(conversion);
		client.GetPeer(1).PutPacket(encodedMessage);
	}


	public void RegisterEvents()
	{
		EventRegistry.RegisterEvent("OnJoinRoom");
		EventRegistry.RegisterEvent("OnRoomCheck");
		EventRegistry.RegisterEvent("OnDisconnectFromLobby");
		EventRegistry.RegisterEvent("OnDisconnectFromQueue");
		EventRegistry.RegisterEvent("OnDataReceived");
		EventRegistry.RegisterEvent("OnPairedReceived");
		EventRegistry.RegisterEvent("OnReadyButtonPressed");
		EventRegistry.RegisterEvent("OnOpponentReadyReceived");
		EventRegistry.RegisterEvent("OnGameStarting");
		EventRegistry.RegisterEvent("SendMessage");
		EventRegistry.RegisterEvent("OnMovePiece");
		EventRegistry.RegisterEvent("OnTimerUpdate");
		EventRegistry.RegisterEvent("OnTurnSwitch");
		EventRegistry.RegisterEvent("ShowRoom");
		EventRegistry.RegisterEvent("SetWaitingQueue");
		EventRegistry.RegisterEvent("PlayerConnected");
		EventRegistry.RegisterEvent("OnGameOver");
		EventRegistry.RegisterEvent("SetWaitingContainerVisible");
		EventRegistry.RegisterEvent("OnOpponentDisconnectedGame");
		EventRegistry.RegisterEvent("OnGameReconnect");
		EventRegistry.RegisterEvent("ToggleReconnectPopup");
		EventRegistry.RegisterEvent("OnInvalidMove");
		EventRegistry.RegisterEvent("OnConcede");
		EventRegistry.RegisterEvent("OnQueueConfirmation");
	}

	private void SubscribeToEvents()
	{
		EventSubscriber.SubscribeToEvent("OnJoinRoom", OnJoinRoom);
		EventSubscriber.SubscribeToEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
		EventSubscriber.SubscribeToEvent("OnDisconnectFromQueue", OnDisconnectFromQueue);
		EventSubscriber.SubscribeToEvent("OnReadyButtonPressed", OnReadyButtonPressed);
		EventSubscriber.SubscribeToEvent("SendMessage", SendMessageEvent);
		EventSubscriber.SubscribeToEvent("OnConcede", OnConcede);
	}
	public override void _ExitTree()
	{
		client.DisconnectFromHost(200, "Exited the game, ending connection.");
		EventSubscriber.UnsubscribeFromEvent("OnJoinRoom", OnJoinRoom);
		EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromLobby", OnDisconnectFromLobby);
		EventSubscriber.UnsubscribeFromEvent("OnDisconnectFromQueue", OnDisconnectFromQueue);
		EventSubscriber.UnsubscribeFromEvent("OnReadyButtonPressed", OnReadyButtonPressed);
		EventSubscriber.UnsubscribeFromEvent("SendMessage", SendMessageEvent);
		EventSubscriber.UnsubscribeFromEvent("OnConcede", OnConcede);

		EventRegistry.UnregisterEvent("OnConcede");
		EventRegistry.UnregisterEvent("OnJoinRoom");
		EventRegistry.UnregisterEvent("OnRoomCheck");
		EventRegistry.UnregisterEvent("OnDisconnectFromLobby");
		EventRegistry.UnregisterEvent("OnDisconnectFromQueue");
		EventRegistry.UnregisterEvent("OnDataReceived");
		EventRegistry.UnregisterEvent("OnPairedReceived");
		EventRegistry.UnregisterEvent("OnReadyButtonPressed");
		EventRegistry.UnregisterEvent("OnOpponentReadyReceived");
		EventRegistry.UnregisterEvent("OnGameStarting");
		EventRegistry.UnregisterEvent("SendMessage");
		EventRegistry.UnregisterEvent("OnMovePiece");
		EventRegistry.UnregisterEvent("OnTimerUpdate");
		EventRegistry.UnregisterEvent("OnTurnSwitch");
		EventRegistry.UnregisterEvent("ShowRoom");
		EventRegistry.UnregisterEvent("SetWaitingQueue");
		EventRegistry.UnregisterEvent("PlayerConnected");
		EventRegistry.UnregisterEvent("OnGameOver");
		EventRegistry.UnregisterEvent("SetWaitingContainerVisible");
		EventRegistry.UnregisterEvent("OnOpponentDisconnectedGame");
		EventRegistry.UnregisterEvent("OnGameReconnect");
		EventRegistry.UnregisterEvent("ToggleReconnectPopup");
		EventRegistry.UnregisterEvent("OnInvalidMove");
		EventRegistry.UnregisterEvent("OnQueueConfirmation");
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

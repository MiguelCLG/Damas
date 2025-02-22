using System.Collections;
using System.Collections.Generic;

public enum Commands
{
  connected,
  create_room,
  room_created,
  paired,
  ready_queue,
  join_room,
  leave_room,
  game_info,
  queue,
  queue_confirmation,
  message,
  opponent_ready,
  game_start,
}

public enum MessageType
{
  info,
  warning,
  error
}

public class PlayerInfo
{
  public string player_id;
  public string player_name;
  public float money;
  public string status; // TODO: not doing anything with it at the moment, but the BE sends it
}

public class WaitingQueueSize
{
  public int waiting_queue_size;
}

public class LobbyInfo
{
  public string id;
  public string name;
  public string currency;
  public float selected_bid;
}
public class RoomInfo
{
  public float aggregate_value;
  public int count;
}

public class RoomInfoList
{
  public int players_waiting;

  public List<RoomInfo> room_aggregate;
}

public class PairedValue
{
  public int color; // 0 - black | 1 - white
  public string opponent;
  public string room_id;
}

public class OpponentReady
{
  public bool is_ready;
}

public class Message
{
  public MessageType message_type;
  public string message;
}

public class Piece
{
  public string type;
  public string player_id;
  public string piece_id;
}

public class BoardTile
{
  public string name;
  public Piece piece;
}

public class MovePieceData
{
  public string player_id;  // The player making the move
  public string piece_id;  // Will be given to clientes by the server.
  public string from;      // e.g., "A1"
  public string to;        // e.g., "B2"
  public bool is_capture; // Whether the move captured an opponent's piece
  public bool is_kinged;  // Whether the piece was kinged after the move
}
public class GamePlayer
{
  public string id;
  public string token;
  public string name;
  public string color;
  public string session_id;
}
public class GameStartMessage
{
  public Hashtable Board;
  public string CurrentPlayerID;
  public List<GamePlayer> GamePlayers;
}

public class GameUpdatetMessage
{
  public Hashtable board;
  public string current_player_id;
  public int current_turn;
}

public class GameTimer
{
  public float player_timer;
  public string current_player_id;
}

public class DataReceived<T>
{

  public Commands command;
  public T value;

  public DataReceived(Commands commandReceived, T _value)
  {
    command = commandReceived;
    value = _value;
  }
}

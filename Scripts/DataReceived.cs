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
  opponent_ready
}

public enum MessageType
{
  info,
  warning,
  error
}

public class PlayerInfo
{
  public string player_name;
  public float money;
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

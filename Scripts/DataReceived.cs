using Godot;
using Godot.Collections;

public enum Commands
{
  connected,
  create_room,
  room_created,
  leave_queue,
  send_message,
  custom_command,
  move_piece,
  room_info,
  game_info,
}

public interface IValue
{
}
public class PlayerInfo : IValue
{
  public string player_name;
  public float money;
}

public class WaitingQueueSize : IValue
{
  public int waiting_queue_size;
}

public class LobbyInfo : IValue
{
  public string id;
  public string name;
  public string currency;
  public float selected_bid;
}
public class RoomInfo : IValue
{
  public float agregate_value;
  public int count;
}

public class RoomInfoList : IValue
{
  public int players_waiting;

  public Array<RoomInfo> room_aggregate;
}

public class DataReceived
{

  public Commands command;
  public IValue value;

  public DataReceived(Commands commandReceived, IValue _value = null)
  {
    command = commandReceived;
    value = _value;
  }
}

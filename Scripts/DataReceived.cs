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

public class RoomInfo : IValue
{
  public string id;
  public string name;
  public string currency;
  public float SelectedBid;
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

using Godot.Collections;

public enum Commands
{
  connected,
  join_queue,
  leave_queue,
  send_message,
  custom_command,
  paired,
  update_waiting_queue,
  move_piece,
}
public class PlayerInfo
{
  public string player_name;
  public float money;
}
public class DataReceived
{

  public Commands command;
  public PlayerInfo value;

  public DataReceived(Commands commandReceived, PlayerInfo playerInfo = null)
  {
    command = commandReceived;
    value = playerInfo;
  }
}

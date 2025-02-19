using Godot.Collections;

public static class GameState
{
  public static GamePlayer player;
  public static string playerName;
  public static string opponentName;
  public static BoardColors currentGameColor;
  public static float betValue = 1f;
  public static string room_id;
  public static Array<float> availableBidValues = new Array<float>() { 0.5f, 1, 3, 5, 10, 25, 50, 100 };
}

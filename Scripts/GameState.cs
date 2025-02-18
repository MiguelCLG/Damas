using Godot.Collections;

public static class GameState
{
  public static string playerName;
  public static string opponentName;
  public static BoardColors currentGameColor;
  public static float betValue = 1f;
  public static string room_id;
  public static Array<float> availableBidValues = new Array<float>() { 0.5f, 1, 3, 5, 10, 25, 50, 100 };

  public static int[,] checkersScheme = new int[8, 8]
  {
      { 0, 1, 0, 0, 0, 2, 0, 2 },
      { 1, 0, 1, 0, 0, 0, 2, 0 },
      { 0, 1, 0, 0, 0, 2, 0, 2 },
      { 1, 0, 1, 0, 0, 0, 2, 0 },
      { 0, 1, 0, 0, 0, 2, 0, 2 },
      { 1, 0, 1, 0, 0, 0, 2, 0 },
      { 0, 1, 0, 0, 0, 2, 0, 2 },
      { 1, 0, 1, 0, 0, 0, 2, 0 }
  };
  public static string[,] boardScheme = new string[8, 8]
  {
      { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8" },
      { "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8" },
      { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8" },
      { "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8" },
      { "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" },
      { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8" },
      { "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8" },
      { "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8" },
  };
}

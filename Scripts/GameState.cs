using System.Collections;
using Godot.Collections;

public static class GameState
{
  public static GamePlayer player;
  public static BoardColors currentGameColor;
  public static Hashtable initialBoard = new Hashtable
    {
        { "A1", "W_1" }, { "A2", null }, { "A3", "W_2" }, { "A4", null },{ "A5", "W_3" }, { "A6", null }, { "A7", "W_4" }, { "A8", null },

        { "B1", null }, { "B2", "W_5" }, { "B3", null }, { "B4", "W_6" }, { "B5", null }, { "B6", "W_7" }, { "B7", null }, { "B8", "W_8" },

        { "C1", "W_9" }, { "C2", null }, { "C3", "W_10" }, { "C4", null }, { "C5", "W_11" }, { "C6", null }, { "C7", "W_12" }, { "C8", null },

        { "D1", null }, { "D2", null }, { "D3", null }, { "D4", null }, { "D5", null }, { "D6", null }, { "D7", null }, { "D8", null },

        { "E1", null }, { "E2", null }, { "E3", null }, { "E4", null }, { "E5", null }, { "E6", null }, { "E7", null }, { "E8", null },

        { "F1", null }, { "F2", "B_12" }, { "F3", null }, { "F4", "B_11" }, { "F5", null }, { "F6", "B_10" }, { "F7", null }, { "F8", "B_9" },

        { "G1", "B_8" }, { "G2", null }, { "G3", "B_7" }, { "G4", null }, { "G5", "B_6" }, { "G6", null }, { "G7", "B_5" }, { "G8", null },

        { "H1", null }, { "H2", "B_4" }, { "H3", null }, { "H4", "B_3" }, { "H5", null }, { "H6", "B_2" }, { "H7", null }, { "H8", "B_1" },
    };

  public static string playerName;
  public static string opponentName;
  public static float betValue = 1f;
  public static string room_id;
  public static Array<float> availableBidValues = new Array<float>() { 0.5f, 1f, 3f, 5f, 10f, 25f, 50f, 100f };
}

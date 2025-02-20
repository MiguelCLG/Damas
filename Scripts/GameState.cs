using System.Collections;
using Godot.Collections;

public static class GameState
{
  public static GamePlayer player;
  public static BoardColors currentGameColor;
  public static Hashtable initialBoard = new Hashtable
    {
        { "A1", "W_1" },
        { "B1", null },
        { "C1", "W_2" },
        { "D1", null },
        { "E1", "W_3" },
        { "F1", null },
        { "G1", "W_4" },
        { "H1", null },

        { "A2", null },
        { "B2", "W_5" },
        { "C2", null },
        { "D2", "W_6" },
        { "E2", null },
        { "F2", "W_7" },
        { "G2", null },
        { "H2", "W_8" },

        { "A3", "W9" },
        { "B3", null },
        { "C3", "W_10" },
        { "D3", null },
        { "E3", "W_11" },
        { "F3", null },
        { "G3", "W_12" },
        { "H3", null },

        { "A4", null },
        { "B4", null },
        { "C4", null },
        { "D4", null },
        { "E4", null },
        { "F4", null },
        { "G4", null },
        { "H4", null },

        { "A5", null },
        { "B5", null },
        { "C5", null },
        { "D5", null },
        { "E5", null },
        { "F5", null },
        { "G5", null },
        { "H5", null },

        { "A6", null },
        { "B6", "B_12" },
        { "C6", null },
        { "D6", "B_11" },
        { "E6", null },
        { "F6", "B_10" },
        { "G6", null },
        { "H6", "B_9" },

        { "A7", "B_8" },
        { "B7", null },
        { "C7", "B_7" },
        { "D7", null },
        { "E7", "B_6" },
        { "F7", null },
        { "G7", "B_5" },
        { "H7", null },

        { "A8", null },
        { "B8", "B_4" },
        { "C8", null },
        { "D8", "B_3" },
        { "E8", null },
        { "F8", "B_2" },
        { "G8", null },
        { "H8", "B_1" },
    };

  public static string playerName;
  public static string opponentName;
  public static float betValue = 1f;
  public static string room_id;
  public static Array<float> availableBidValues = new Array<float>() { 0.5f, 1f, 3f, 5f, 10f, 25f, 50f, 100f };
}

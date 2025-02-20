using System.Collections;
using Godot.Collections;

public static class GameState
{
  public static GamePlayer player;
  public static BoardColors currentGameColor;
  public static Hashtable board = new Hashtable
    {
        { "A1", null },
        { "B1", null },
        { "C1", null },
        { "D1", null },
        { "E1", null },
        { "F1", null },
        { "G1", null },
        { "H1", null },

        { "A2", null },
        { "B2", null },
        { "C2", null },
        { "D2", null },
        { "E2", null },
        { "F2", null },
        { "G2", null },
        { "H2", null },

        { "A3", null },
        { "B3", null },
        { "C3", null },
        { "D3", null },
        { "E3", null },
        { "F3", null },
        { "G3", null },
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
        { "B6", null },
        { "C6", null },
        { "D6", null },
        { "E6", null },
        { "F6", null },
        { "G6", null },
        { "H6", null },

        { "A7", null },
        { "B7", null },
        { "C7", null },
        { "D7", null },
        { "E7", null },
        { "F7", null },
        { "G7", null },
        { "H7", null },

        { "A8", null },
        { "B8", null },
        { "C8", null },
        { "D8", null },
        { "E8", null },
        { "F8", null },
        { "G8", null },
        { "H8", null },
    };

  public static string playerName;
  public static string opponentName;
  public static float betValue = 1f;
  public static string room_id;
  public static Array<float> availableBidValues = new Array<float>() { 0.5f, 1f, 3f, 5f, 10f, 25f, 50f, 100f };
}

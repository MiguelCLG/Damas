using System.Collections;
using Godot;
using Godot.Collections;

public static class GameState
{
  public static GamePlayer player;
  public static PlayerInfo playerInfo;
  public static BoardColors currentGameColor;
  public static BoardColors currentInGameTurn = BoardColors.Black;
  public static int MaxTimer = 15;
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
  public static Hashtable RotateBoard(Hashtable board)
  {
    // Define the tile positions for a 180-degree rotation
    string[,] originalKeys = {
        { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8" },
        { "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8" },
        { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8" },
        { "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8" },
        { "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" },
        { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8" },
        { "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8" },
        { "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8" }
    };

    string[,] rotatedKeys = {
        { "H8", "H7", "H6", "H5", "H4", "H3", "H2", "H1" },
        { "G8", "G7", "G6", "G5", "G4", "G3", "G2", "G1" },
        { "F8", "F7", "F6", "F5", "F4", "F3", "F2", "F1" },
        { "E8", "E7", "E6", "E5", "E4", "E3", "E2", "E1" },
        { "D8", "D7", "D6", "D5", "D4", "D3", "D2", "D1" },
        { "C8", "C7", "C6", "C5", "C4", "C3", "C2", "C1" },
        { "B8", "B7", "B6", "B5", "B4", "B3", "B2", "B1" },
        { "A8", "A7", "A6", "A5", "A4", "A3", "A2", "A1" }
    };

    // Create a new hashtable to store the rotated board
    Hashtable rotatedBoard = new Hashtable();

    // Perform the rotation by mapping each original key to its rotated position
    for (int i = 0; i < originalKeys.GetLength(0); i++)
    {
      for (int j = 0; j < originalKeys.GetLength(1); j++)
      {
        string originalKey = originalKeys[i, j];
        string rotatedKey = rotatedKeys[i, j];

        // Transfer the value from the original key to the rotated key
        rotatedBoard[rotatedKey] = board[originalKey];
      }
    }
    return rotatedBoard;
  }
}

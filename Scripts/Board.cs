using Godot;
using Godot.Collections;
using static Utils;
using static GameState;

public partial class Board : Control
{
  [Export] private PackedScene tileScene;
  private Array<Tile> BoardTiles { get; set; }
  private GridContainer BoardContainer { get; set; }
  public bool IsCaptureMove = false;
  public Dictionary<Checker, Tile> CaptureMoves = new Dictionary<Checker, Tile>(); // saves the checker to capture and the target tile for the current selected checker
  public Array<Tile> RegularMoves = new Array<Tile>();
  int direction = -1;

  public override void _Ready()
  {
    BoardTiles = new Array<Tile>();
    BoardContainer = GetNode<GridContainer>("%BoardContainer");
  }

  // Function called from the GameplaySystem when all events have been registered and subscribed (Preventing a race condition)
  public void InitializeBoard()
  {
    if (currentGameColor == BoardColors.White) FillBoardTiles(); else FillBoardTilesReverse();
  }
  public Tile FindTileByName(string name)
  {
    return BoardContainer.GetNode<Tile>(name);
  }

  public bool HasCaptureMove(Checker checker)
  {
    if (checker.isKing)
    {
      /* return KingHasCaptureMove(checker); */
    }
    else
    {
    }
    return CheckerHasCaptureMove(checker);

  }

  private bool CheckerHasCaptureMove(Checker checker)
  {
    CalculateMoves(checker);
    return IsCaptureMove;
  }

  /* private bool KingHasCaptureMove(Checker checker)
  {
    Vector2[] diagonals = { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) };
    var captureMoves = new Array<Vector2>();
    foreach (var diagonal in diagonals)
    {
      var steps = 1;
      while (true)
      {
        Vector2 checkPosition = checker.BoardPosition + diagonal * steps;

        if (!IsWithinBounds(checkPosition) || IsAllyAt(checkPosition, checker.Color, checkersInPlay))
        {
          break;
        }
        if (IsEnemyAt(checkPosition, checker.Color, checkersInPlay))
        {
          // Check the space behind the enemy
          Vector2 behindEnemyPosition = checker.BoardPosition + diagonal * steps + diagonal;
          if (IsWithinBounds(behindEnemyPosition))
          {
            if (IsSpaceEmpty(behindEnemyPosition, checkersInPlay))
            {
              captureMoves.Add(behindEnemyPosition);
            }
          }
        }
        steps++;
      }
    }

    return captureMoves.Count > 0;
  } */

  private bool IsAllyAt(Vector2 checkPosition, BoardColors color, Array<Checker> checkersInPlay)
  {
    foreach (var checker in checkersInPlay)
    {
      if (checker.BoardPosition == checkPosition && checker.Color == color)
      {
        return true;
      }
      else if (checker.BoardPosition == checkPosition && checker.Color != color)
      {
        return false;
      }
    }
    return false;
  }


  // Since we want to prioritize capture moves, we first check for them
  // then we if there aren't any, we check for regular moves
  // this way we can highlight and diferentiate a capture from a regular move
  public void OnCheckerClicked(Checker checker, Array<Checker> checkersInPlay)
  {
    if (checker.isKing)
    {
      /*  KingMovement(checker, checkersInPlay); */
    }
    else
    {
      CheckerMovement(checker);
    }
  }

  private void CheckerMovement(Checker checker)
  {
    CalculateMoves(checker);
    if (IsCaptureMove)
    {
      foreach (var captureMove in CaptureMoves)
      {

        captureMove.Value.Select(true);
      }
    }
    else
    {
      foreach (var regularMove in RegularMoves)
      {
        regularMove.Select(false);
      }
    }
    checker.MovementSpaces = (Array<Tile>)(IsCaptureMove ? CaptureMoves.Values : RegularMoves);
  }

  private void CalculateMoves(Checker checker)
  {
    direction = (checker.Color == BoardColors.White) ? -1 : 1;

    // setting up a diagonals vector so we can check its tiles, basically left or right and up or down (depending on the direction above)
    Vector2[] diagonals = { new Vector2(direction, -1), new Vector2(direction, 1) };
    CaptureMoves = new Dictionary<Checker, Tile>(); // saves the checker to capture and the target tile for the current selected checker
    RegularMoves = new Array<Tile>();

    string tileName = checker.GetParent().Name;
    int row = tileName[0] - 'A';
    int column = int.Parse(tileName[1].ToString());


    foreach (var diagonal in diagonals)
    {
      char checkTileLetter = (char)('A' + (row + Mathf.FloorToInt(diagonal.x)));
      int checkTileNumber = column + Mathf.FloorToInt(diagonal.y);
      string checkTileName = checkTileLetter.ToString() + checkTileNumber.ToString();

      if (checkTileNumber < 1 || checkTileNumber > BoardSize || checkTileLetter < 'A' || checkTileLetter > 'H')
        continue;

      var checkTile = FindTileByName(checkTileName);
      if (checkTile.GetChild(checkTile.GetChildCount() - 1) is Checker c) // There is a checker in the tile
      {
        if (checker.Color != c.Color)
        {
          // it is an enemy checker
          char tileBehindLetter = (char)('A' + (row + Mathf.FloorToInt(diagonal.x * 2)));
          int tileBehindNumber = checkTileNumber + Mathf.FloorToInt(diagonal.y);
          string tileBehindName = tileBehindLetter.ToString() + tileBehindNumber.ToString();

          if (tileBehindNumber < 1 || tileBehindNumber > BoardSize || tileBehindLetter < 'A' || tileBehindLetter > 'H')
            continue;

          var tileBehind = FindTileByName(tileBehindName);

          if (tileBehind.GetChild(tileBehind.GetChildCount() - 1) is Checker) // tile behind is not empty
            continue;

          CaptureMoves.Add(c, tileBehind);
        }
      }
      else
      { // there is no checker in the tile
        RegularMoves.Add(checkTile);
      }
    }
    IsCaptureMove = CaptureMoves.Count > 0;
  }

  /* private void KingMovement(Checker checker, Array<Checker> checkersInPlay)
  {
    // Kings can move in all diagonal directions: up-left, up-right, down-left, down-right
    Vector2[] diagonals = { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) };

    // This time we have 4 directions so this loop will run 4 times
    foreach (var diagonal in diagonals)
    {
      Vector2 currentPosition = checker.BoardPosition;

      while (true) // we want to loop through all the available spaces until we are out of bounds, so that we can select the space we want to go
      {
        currentPosition += diagonal;

        if (!IsWithinBounds(currentPosition))
        {
          break; // Stop if out of bounds
        }

        if (IsEnemyAt(currentPosition, checker.Color, checkersInPlay))
        {
          Vector2 behindEnemyPosition = currentPosition + diagonal;

          if (IsWithinBounds(behindEnemyPosition) && IsSpaceEmpty(behindEnemyPosition, checkersInPlay)) // Check if the space behind the enemy is free for a capture
          {
            CaptureMoves.Add(behindEnemyPosition);
            break; // Following the rules, the king must capture the enemy and stop right after
          }
          else
          {
            break;
          }
        }
        else if (IsSpaceEmpty(currentPosition, checkersInPlay))
        {
          RegularMoves.Add(currentPosition); // Add a regular move
        }
        else
        {
          break;
        }
      }
    }

    // Prioritize capture moves over regular moves
    if (CaptureMoves.Count > 0)
    {
      foreach (var captureMove in CaptureMoves)
      {
        foreach (Tile tile in BoardTiles)
        {
          if (tile.TilePosition == captureMove)
          {
            tile.Select(true); // Highlight tile for capture
          }
        }
      }
    }
    else // If no capture moves are available, highlight regular moves
    {
      foreach (var regularMove in RegularMoves)
      {
        foreach (Tile tile in BoardTiles)
        {
          if (tile.TilePosition == regularMove)
          {
            tile.Select(false); // Highlight tile for regular move
          }
        }
      }
    }

    IsCaptureMove = CaptureMoves.Count > 0;
    checker.MovementSpaces = IsCaptureMove ? CaptureMoves.Values : RegularMoves;
  }
 */

  // This function will unselect all the tiles that were highlighted
  public void CleanUpFreeTiles(Checker checker)
  {
    foreach (Tile tile in checker.MovementSpaces)
    {
      tile.Unselect();
    }
  }

  // This function will create new tiles and place their respective positions, taking into account the viewport size
  private void FillBoardTiles()
  {
    // Vector2 screenSize = GetViewportRect().Size;
    for (int col = 0; col < BoardSize; col++)
    {
      for (int row = 0; row < BoardSize; row++)
      {
        string tileKey = $"{(char)('A' + col)}{row + 1}";
        // Create Tile and populate with ID / Name
        var tileInstance = tileScene.Instance<Tile>();
        tileInstance.Name = tileKey;
        tileInstance.TilePosition = new Vector2(col, row);
        BoardContainer.AddChild(tileInstance);

        // Determine the color (alternate white/black) based on position
        bool isWhiteTile = (row + col) % 2 == 1;
        tileInstance.SetTileColor(isWhiteTile ? BoardColors.White : BoardColors.Black);
        BoardTiles.Add(tileInstance);
        //tileInstance.AddChild(new Label() { Text = tileKey.ToString() });

      }
    }
  }

  private void FillBoardTilesReverse()
  {
    for (int col = 0; col < BoardSize; col++)
    {
      for (int row = 0; row < BoardSize; row++)
      {
        string tileKey = $"{(char)('H' - col)}{BoardSize - row}"; // Swap row and col
        var tileInstance = tileScene.Instance<Tile>();
        tileInstance.Name = tileKey;
        tileInstance.TilePosition = new Vector2(col, BoardSize - 1 - row); // Swap row and col
        BoardContainer.AddChild(tileInstance);

        bool isWhiteTile = (row + col) % 2 == 1;
        tileInstance.SetTileColor(isWhiteTile ? BoardColors.White : BoardColors.Black);
        BoardTiles.Add(tileInstance);
        //tileInstance.AddChild(new Label() { Text = tileKey.ToString() });
      }
    }
  }


  // Utilitary function to check if that position on the board has an enemy
  private bool IsEnemyAt(Vector2 enemyPosition, BoardColors color, Array<Checker> checkersInPlay)
  {
    foreach (var checker in checkersInPlay)
    {
      if (checker.BoardPosition == enemyPosition && checker.Color == color)
      {
        return false;
      }
      else if (checker.BoardPosition == enemyPosition && checker.Color != color)
      {
        return true;
      }
    }
    return false;
  }

  // Utilitary function to check if that position on the board is empty
  private bool IsSpaceEmpty(Vector2 checkPosition, Array<Checker> checkersInPlay)
  {
    foreach (var checker in checkersInPlay)
    {
      if (checker.BoardPosition == checkPosition)
      {
        return false;
      }
    }
    return true;
  }
  // Utilitary function to check if a position is within the bounds of the board
  private bool IsWithinBounds(Vector2 position)
  {
    return position.x >= 0 && position.x < BoardSize && position.y >= 0 && position.y < BoardSize; // assuming an 8x8 board
  }

  // In the case of a restart, this function will be called
  // So we do a cleanup here to free the memory no longer used
  // We remove tiles or any event subscription, etc...
  public override void _ExitTree()
  {
    // Free the tiles and remove them from the scene (No orphan nodes!)
    foreach (Tile tile in BoardTiles)
    {
      BoardContainer.RemoveChild(tile);
      tile.QueueFree();
    }
    BoardTiles.Clear();
  }
}

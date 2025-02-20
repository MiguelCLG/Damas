using Godot;
using Godot.Collections;
using static Utils;

public partial class Board : Control
{
  [Export] private PackedScene tileScene;
  private Array<Tile> BoardTiles { get; set; }
  private GridContainer BoardContainer { get; set; }
  public bool IsCaptureMove = false;

  public override void _Ready()
  {
    BoardTiles = new Array<Tile>();
    BoardContainer = GetNode<GridContainer>("%BoardContainer");
  }

  // Function called from the GameplaySystem when all events have been registered and subscribed (Preventing a race condition)
  public void InitializeBoard()
  {
    FillBoardTiles();
  }
  public Tile FindTileByName(string name)
  {
    return BoardContainer.GetNode<Tile>(name);
  }

  public bool HasCaptureMove(Checker checker, Array<Checker> checkersInPlay)
  {
    if (checker.isKing)
    {
      return KingHasCaptureMove(checker, checkersInPlay);
    }
    else
    {
      return CheckerHasCaptureMove(checker, checkersInPlay);
    }

  }

  private bool CheckerHasCaptureMove(Checker checker, Array<Checker> checkersInPlay)
  {
    int direction = (checker.Color == BoardColors.Black) ? 1 : -1;

    Vector2[] diagonals = { new Vector2(-1, direction), new Vector2(1, direction) };
    Array<Vector2> captureMoves = new Array<Vector2>();

    foreach (var diagonal in diagonals)
    {
      Vector2 checkPosition = checker.BoardPosition + diagonal;

      if (IsWithinBounds(checkPosition))
      {
        if (IsEnemyAt(checkPosition, checker.Color, checkersInPlay))
        {
          // Check the space behind the enemy
          Vector2 behindEnemyPosition = checker.BoardPosition + diagonal * 2;
          if (IsWithinBounds(behindEnemyPosition))
          {
            if (IsSpaceEmpty(behindEnemyPosition, checkersInPlay))
            {
              captureMoves.Add(behindEnemyPosition);
            }
          }
        }
      }
    }

    return captureMoves.Count > 0;
  }

  private bool KingHasCaptureMove(Checker checker, Array<Checker> checkersInPlay)
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
  }

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
      KingMovement(checker, checkersInPlay);
    }
    else
    {
      CheckerMovement(checker, checkersInPlay);
    }
  }

  private void CheckerMovement(Checker checker, Array<Checker> checkersInPlay)
  {
    int direction = (checker.Color == BoardColors.Black) ? 1 : -1;

    // setting up a diagonals vector so we can check its tiles, basically left or right and up or down (depending on the direction above)
    Vector2[] diagonals = { new Vector2(-1, direction), new Vector2(1, direction) };
    Array<Vector2> captureMoves = new Array<Vector2>();
    Array<Vector2> regularMoves = new Array<Vector2>();

    // we have only 2 directions so this loop will run 2 times
    foreach (var diagonal in diagonals)
    {
      Vector2 checkPosition = checker.BoardPosition + diagonal;

      if (IsWithinBounds(checkPosition))
      {
        if (IsEnemyAt(checkPosition, checker.Color, checkersInPlay))
        {
          // Check the space behind the enemy
          Vector2 behindEnemyPosition = checker.BoardPosition + diagonal * 2;

          // Ensure the behind position is within bounds and empty
          // if so, we add it to the capture moves
          if (IsWithinBounds(behindEnemyPosition) && IsSpaceEmpty(behindEnemyPosition, checkersInPlay))
          {
            captureMoves.Add(behindEnemyPosition);
          }
        }
        else if (IsSpaceEmpty(checkPosition, checkersInPlay))
        {
          regularMoves.Add(checkPosition);
        }
      }
    }

    // Prioritize capture moves over regular moves
    if (captureMoves.Count > 0)
    {
      foreach (var captureMove in captureMoves)
      {
        foreach (Tile tile in BoardTiles)
        {
          if (tile.TilePosition == captureMove)
          {
            tile.Select(true); // Highlight the tile for capture
          }
        }
      }
    }
    else // If no capture moves are available, highlight regular moves
    {
      foreach (var regularMove in regularMoves)
      {
        foreach (Tile tile in BoardTiles)
        {
          if (tile.TilePosition == regularMove)
          {
            tile.Select(false); // Highlight the tile for regular move
          }
        }
      }
    }
    IsCaptureMove = captureMoves.Count > 0;
    checker.MovementSpaces = IsCaptureMove ? captureMoves : regularMoves;
  }

  private void KingMovement(Checker checker, Array<Checker> checkersInPlay)
  {
    // Kings can move in all diagonal directions: up-left, up-right, down-left, down-right
    Vector2[] diagonals = { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) };
    Array<Vector2> captureMoves = new Array<Vector2>();
    Array<Vector2> regularMoves = new Array<Vector2>();

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
            captureMoves.Add(behindEnemyPosition);
            break; // Following the rules, the king must capture the enemy and stop right after
          }
          else
          {
            break;
          }
        }
        else if (IsSpaceEmpty(currentPosition, checkersInPlay))
        {
          regularMoves.Add(currentPosition); // Add a regular move
        }
        else
        {
          break;
        }
      }
    }

    // Prioritize capture moves over regular moves
    if (captureMoves.Count > 0)
    {
      foreach (var captureMove in captureMoves)
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
      foreach (var regularMove in regularMoves)
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

    IsCaptureMove = captureMoves.Count > 0;
    checker.MovementSpaces = IsCaptureMove ? captureMoves : regularMoves;
  }


  // This function will unselect all the tiles that were highlighted
  public void CleanUpFreeTiles(Checker checker)
  {
    foreach (var freeSpace in checker.MovementSpaces)
    {
      foreach (Tile tile in BoardTiles)
      {
        if (tile.TilePosition == freeSpace)
        {
          tile.Unselect();
        }
      }
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
        string tileKey = $"{(char)('A' + row)}{col + 1}";

        // Create Tile and populate with ID / Name
        var tileInstance = tileScene.Instance<Tile>();
        tileInstance.Name = tileKey;
        BoardContainer.AddChild(tileInstance);

        // Determine the color (alternate white/black) based on position
        bool isWhiteTile = (row + col) % 2 == 0;
        tileInstance.SetTileColor(isWhiteTile ? BoardColors.White : BoardColors.Black);
      }
    }
    /* foreach (DictionaryEntry tile in GameState.board)
    {
      // Create Tile and populate with ID / Name
      // Insert tile in Grid Container
      var tileInstance = tileScene.Instance<Tile>();
      tileInstance.Name = $"{tile.Key}";
      GetNode<GridContainer>("%GridContainer").AddChild(tileInstance);
      tileInstance.AddChild(new Label() { Text = $"{tile.Key}" });
      var row = tile.Key.ToString().ToCharArray()[0];
      var column = tile.Key.ToString().ToCharArray()[1];
      var rowNumber = row - 'A' + 1;
      tileInstance.SetTileColor(int.Parse(column.ToString()) % 2 == rowNumber % 2 ? BoardColors.White : BoardColors.Black);

    } */
    /* for (int i = 0; i < BoardSize; i++)
    {
      for (int j = 0; j < BoardSize; j++)
      {
        var tile = tileScene.Instance<Tile>();
        tile.Name = $"Tile_{i}_{j}"; // Names tiles with their respective positions in the grid

        var tileSizeX = TileSize * (screenSize.x * 100 / ViewportBaseX) / 100;
        var tileSizeY = TileSize * (screenSize.x * 100 / ViewportBaseX) / 100;

        // Set the tile's size, this can be changed if the viewport is changed.
        tile.RectMinSize = new Vector2(tileSizeX, tileSizeY);
        tile.RectSize = new Vector2(tileSizeX, tileSizeY);
        // Position the tiles in the grid with a size offset, starting from the calculated position

        tile.RectPosition = new Vector2(i * tileSizeX, j * tileSizeY);
        tile.TilePosition = new Vector2(i, j);
        BoardTiles.Add(tile);
        BoardContainer.AddChild(tile);
        tile.SetTileColor(i % 2 == j % 2 ? BoardColors.White : BoardColors.Black);
      }
    } */
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

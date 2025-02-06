using System;
using Godot;
using Godot.Collections;
using static Utils;

public partial class GameplaySystem : Node2D
{
  [Export] private PackedScene checkerScene;
  [Export] private StyleBoxFlat portraitBgStyle;
  [Export] private StyleBoxFlat portraitBgSelectedStyle;

  private Checker SelectedChecker = null;
  private Checker CheckerToCapture = null;
  private BoardColors CurrentTurn = BoardColors.Black;
  private Array<Checker> BlackCheckers = new Array<Checker>();
  private Array<Checker> WhiteCheckers = new Array<Checker>();
  private Array<Checker> AllCheckers = new Array<Checker>();
  private Board board;
  private Control checkersContainer;
  private GameOverMenu gameOverMenu;
  private ProgressBar BlackPlayerTimer;
  private ProgressBar WhitePlayerTimer;
  private PanelContainer BlackPlayerPortraitBackground;
  private PanelContainer WhitePlayerPortraitBackground;

  private Vector2 screenSize;
  private Vector2 totalBoardSize;
  private Array<Checker> checkersWithCaptureMoves = new Array<Checker>();
  private bool LastMoveWasCapture = false;

  public override void _Ready()
  {
    GetTree().Paused = false;
    // Registering events and subscribing to them, since this is the system that will handle them.
    // To Turn this multiplayer, I believe this will need to be server side since it is what keeps track all the checkers in play
    EventRegistry.RegisterEvent("TileClicked");
    EventRegistry.RegisterEvent("CheckerClicked");
    EventRegistry.RegisterEvent("SpawnCheckers");
    EventSubscriber.SubscribeToEvent("TileClicked", OnTileClicked);
    EventSubscriber.SubscribeToEvent("CheckerClicked", OnCheckerClicked);
    EventSubscriber.SubscribeToEvent("SpawnCheckers", SpawnCheckers);

    // added this signal to make sure we can resize our pieces if the window is resized
    GetTree().Root.Connect("size_changed", this, nameof(OnViewportChanged));

    /* GetNode<Label>("%TurnCount").Text = $"{TurnCount}";
    GetNode<Label>("%CurrentTurn").Text = $"{CurrentTurn}"; */

    // added these for resizing and positioning purposes
    screenSize = GetViewportRect().Size;
    totalBoardSize = new Vector2(BoardSize * TileSize, BoardSize * TileSize);

    BlackPlayerTimer = GetNode<ProgressBar>("%BlackPlayerTimer");
    WhitePlayerTimer = GetNode<ProgressBar>("%WhitePlayerTimer");
    BlackPlayerPortraitBackground = GetNode<PanelContainer>("%BlackPlayerPortraitBackground");
    WhitePlayerPortraitBackground = GetNode<PanelContainer>("%WhitePlayerPortraitBackground");
    board = GetNode<Board>("%Board");
    checkersContainer = GetNode<Control>("%CheckersContainer");
    gameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
    gameOverMenu.Hide();

    GenerateCheckers();
    board.InitializeBoard();
    OnTurnStart();
  }

  public override void _Process(float delta)
  {
    if (CurrentTurn == BoardColors.Black)
    {
      BlackPlayerTimer.Value -= delta;
      if (BlackPlayerTimer.Value <= 0)
      {
        NextTurn();
      }
    }
    else
    {
      WhitePlayerTimer.Value -= delta;
      if (WhitePlayerTimer.Value <= 0)
      {
        NextTurn();
      }
    }

  }

  // Function to handle viewport changes, when the signal SizeChanged is emitted, this is what is executed
  private void OnViewportChanged()
  {
    screenSize = GetViewportRect().Size;
    totalBoardSize = new Vector2(BoardSize * TileSize, BoardSize * TileSize);
    board.OnViewPortChanged();
    PositionCheckers();

  }

  private void OnTurnStart()
  {
    if (CurrentTurn == BoardColors.Black)
    {
      WhitePlayerPortraitBackground.AddStyleboxOverride("panel", portraitBgStyle);
      BlackPlayerPortraitBackground.AddStyleboxOverride("panel", portraitBgSelectedStyle);
    }
    else
    {
      BlackPlayerPortraitBackground.AddStyleboxOverride("panel", portraitBgStyle);
      WhitePlayerPortraitBackground.AddStyleboxOverride("panel", portraitBgSelectedStyle);
    }
    GetNode<Label>("%BlackPieceCount").Text = $"{BlackCheckers.Count}";
    GetNode<Label>("%WhitePieceCount").Text = $"{WhiteCheckers.Count}";
    LastMoveWasCapture = false;
    checkersWithCaptureMoves = new Array<Checker>();
    checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
    foreach (var checker in checkersWithCaptureMoves)
    {
      checker.ChangeTexture("res://Assets/available-capture-checker.webp");
    }
  }

  private Array<Checker> FindCheckersWithCaptureMoves()
  {
    var checkers = new Array<Checker>();
    AllCheckers = new Array<Checker>();
    var currentTurnCheckers = new Array<Checker>();

    foreach (Checker c in BlackCheckers)
    {
      if (c.Color == CurrentTurn)
        currentTurnCheckers.Add(c);
      AllCheckers.Add(c);
    }
    foreach (Checker c in WhiteCheckers)
    {
      if (c.Color == CurrentTurn)
        currentTurnCheckers.Add(c);
      AllCheckers.Add(c);
    }
    foreach (var checker in currentTurnCheckers)
    {
      if (board.HasCaptureMove(checker, AllCheckers))
      {
        checkers.Add(checker);
      }
    }
    return checkers;
  }
  private void PositionCheckers()
  {
    foreach (Checker checker in checkersContainer.GetChildren())
    {
      float tileSizeX = TileSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100;
      float checkerSizeX = CheckerSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100;
      Vector2 tilePosition = new Vector2(checker.BoardPosition.x * tileSizeX, checker.BoardPosition.y * tileSizeX);
      Vector2 checkerPosition = tilePosition + new Vector2(tileSizeX - checkerSizeX, tileSizeX - checkerSizeX) / 2;

      checker.RectPosition = checkerPosition;
    }
  }

  private void GenerateCheckers()
  {
    // Defined the board scheme for an easier way to spawn the checkers (1 = black checker, 2 = white checker, 0 = skip)
    int[,] boardScheme = new int[BoardSize, BoardSize] // Test board with three pieces to be killed.
    {
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 2, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 2, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 0, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 0 },
        { 1, 0, 1, 0, 0, 0, 2, 0 }
    };
    /* int[,] boardScheme = new int[BoardSize, BoardSize]
    {
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 }
    }; */

    /* int[,] boardScheme = new int[BoardSize, BoardSize] // Test board with two pieces to be killed.
    {
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 2, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 0, 0, 2, 0, 2 },
        { 1, 0, 1, 0, 0, 0, 2, 0 }
    }; */


    for (int i = 0; i < BoardSize; i++)
    {
      for (int j = 0; j < BoardSize; j++)
      {
        int checkerType = boardScheme[i, j];
        if (checkerType == 0) continue; // Skip empty tiles

        var checker = checkerScene.Instance<Checker>();

        // Name the checker with its position in the grid (helps with debugging in the editor)
        checker.Name = $"Checker_{i}_{j}";

        // Calculate the position of the checker
        // Starts from the board's starting position, add the tile offset, and center the checker within the tile
        // Using only the X axis for the resize / positioning so that we can resize the window keeping the width constant
        float tileSizeX = TileSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100;
        float checkerSizeX = CheckerSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100;
        Vector2 tilePosition = new Vector2(i * tileSizeX, j * tileSizeX);
        Vector2 checkerPosition = tilePosition + new Vector2(tileSizeX - checkerSizeX, tileSizeX - checkerSizeX) / 2;

        checker.RectPosition = checkerPosition;
        checker.BoardPosition = new Vector2(i, j);
        checker.Color = checkerType == 1 ? BoardColors.Black : BoardColors.White;

        if (checkerType == 1)
          BlackCheckers.Add(checker);
        else
          WhiteCheckers.Add(checker);
        AllCheckers.Add(checker);

      }
    }
  }

  private void SpawnCheckers(object sender, object args)
  {
    ClearCheckers();

    for (int i = 0; i < BlackCheckers.Count; i++)
    {
      Checker checker = BlackCheckers[i];
      checker.RectMinSize = new Vector2(CheckerSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100, CheckerSize * (GetViewportRect().Size.y * 100 / ViewportBaseY) / 100);
      checkersContainer.AddChild(checker);
      checker.SetCheckerColor(BoardColors.Black);
    }
    for (int i = 0; i < WhiteCheckers.Count; i++)
    {
      Checker checker = WhiteCheckers[i];
      checker.RectMinSize = new Vector2(CheckerSize * (GetViewportRect().Size.x * 100 / ViewportBaseX) / 100, CheckerSize * (GetViewportRect().Size.y * 100 / ViewportBaseY) / 100);
      checkersContainer.AddChild(checker);
      checker.SetCheckerColor(BoardColors.White);

    }
  }
  public void ClearCheckers()
  {
    foreach (Checker checker in checkersContainer.GetChildren())
    {
      checkersContainer.RemoveChild(checker);
    }
  }

  // Event OnCheckerClicked will execute this function
  private void OnCheckerClicked(object sender, object args)
  {
    // If we already have a selected checker
    if (SelectedChecker != null)
    {
      // we verify if we are trying to select another checker
      if (args is Checker checker)
      {
        if (checkersWithCaptureMoves.Count > 0)
        {
          if (!checkersWithCaptureMoves.Contains(checker))
            return;
        }
        // we verify that it is one of the current player's checkers
        // if so, we select it
        if (CurrentTurn == checker.Color)
        {
          SelectedChecker.UnselectChecker();
          board.CleanUpFreeTiles(SelectedChecker);
          SelectedChecker = checker;
          checker.SelectChecker();

          // Getting info about the checkers so that the board can get the available tiles
          var allCheckers = new Array<Checker>();
          foreach (Checker c in BlackCheckers)
            allCheckers.Add(c);

          foreach (Checker c in WhiteCheckers)
            allCheckers.Add(c);

          board.OnCheckerClicked(checker, allCheckers);
        }
      }
    }
    else
    { // we do not have a checker selected
      if (args is Checker checker)
      {
        if (checkersWithCaptureMoves.Count > 0)
        {
          if (!checkersWithCaptureMoves.Contains(checker))
            return;
        }
        // Like before we verify that it is one of the current player's checkers
        // if so, we select it
        if (CurrentTurn == checker.Color)
        {
          var allCheckers = new Array<Checker>();
          foreach (Checker c in BlackCheckers)
            allCheckers.Add(c);

          foreach (Checker c in WhiteCheckers)
            allCheckers.Add(c);
          SelectedChecker = checker;
          checker.SelectChecker();
          board.OnCheckerClicked(checker, allCheckers);
        }
      }
    }
  }

  // Event OnTileClicked will execute this function
  private void OnTileClicked(object sender, object args)
  {
    if (SelectedChecker != null)
      if (args is Tile tile)
      {
        if (SelectedChecker.MovementSpaces.Contains(tile.TilePosition))
        {
          Vector2 tilePosition = new Vector2(tile.TilePosition.x * TileSize, tile.TilePosition.y * TileSize);
          Vector2 newCheckerPosition = tilePosition + new Vector2(TileSize - CheckerSize, TileSize - CheckerSize) / 2;
          if (IsCaptureMove(SelectedChecker.BoardPosition, tile.TilePosition))
          {
            OnCheckerKilled(CheckerToCapture);
            LastMoveWasCapture = true;
          }
          SelectedChecker.Move(newCheckerPosition, tile.TilePosition);

          board.CleanUpFreeTiles(SelectedChecker);
          SelectedChecker.UnselectChecker();
          NextTurn();
        }
      }
  }

  private bool IsCaptureMove(Vector2 currentPosition, Vector2 targetPosition)
  {
    if (Math.Abs(currentPosition.x - targetPosition.x) != 2 || Math.Abs(currentPosition.y - targetPosition.y) != 2) // making sure it is a diagonal move
      return false;

    Vector2 midPosition = new Vector2(Mathf.Floor((currentPosition.x + targetPosition.x) / 2), Mathf.Floor((currentPosition.y + targetPosition.y) / 2));

    var checkers = CurrentTurn == BoardColors.Black ? WhiteCheckers : BlackCheckers;
    foreach (Checker checker in checkers)
    {
      if (checker.BoardPosition == midPosition)
      {
        CheckerToCapture = checker;
        return true;
      }
    }

    return false; // No checker found

  }
  private void NextTurn()
  {
    if (SelectedChecker != null)
    {
      board.CleanUpFreeTiles(SelectedChecker);
      SelectedChecker.UnselectChecker();
      foreach (var checker in checkersWithCaptureMoves)
      {
        checker.UnselectChecker();
      }
    }
    if (LastMoveWasCapture && board.HasCaptureMove(SelectedChecker, AllCheckers))
    {
      OnTurnStart();
      return;
    }
    CurrentTurn = CurrentTurn == BoardColors.Black ? BoardColors.White : BoardColors.Black;
    SelectedChecker = null;

    WhitePlayerTimer.Value = 15;
    BlackPlayerTimer.Value = 15;
    OnTurnStart();
  }

  // decreases the amount of checkers in the corresponding color
  // checks for win condition, number of pieces is 0
  private void OnCheckerKilled(Checker checker)
  {

    if (checker.Color == BoardColors.Black)
      BlackCheckers.Remove(checker);
    else
      WhiteCheckers.Remove(checker);
    checker.QueueFree();
    if (BlackCheckers.Count == 0)
    {
      GD.Print("White wins!");
      gameOverMenu.SetWinnerName(BoardColors.White.ToString());
      gameOverMenu.Show();
      GetTree().Paused = true;
    }
    else if (WhiteCheckers.Count == 0)
    {
      GD.Print("Black wins!");
      gameOverMenu.SetWinnerName(BoardColors.Black.ToString());
      gameOverMenu.Show();
      GetTree().Paused = true;
    }

  }

  // In the case of a restart, this function will be called
  // So we do a cleanup here to free the memory no longer used
  // We remove tiles or any event subscription, etc...
  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("TileClicked", OnTileClicked);
    EventSubscriber.UnsubscribeFromEvent("CheckerClicked", OnCheckerClicked);
    EventSubscriber.UnsubscribeFromEvent("SpawnCheckers", SpawnCheckers);
    EventRegistry.UnregisterEvent("TileClicked");
    EventRegistry.UnregisterEvent("CheckerClicked");
    EventRegistry.UnregisterEvent("SpawnCheckers");
  }
}

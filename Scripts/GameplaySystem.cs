using Godot;
using Godot.Collections;
using static Utils;
using static GameState;
using System.Collections;

public partial class GameplaySystem : Node2D
{
  [Export] private PackedScene checkerScene;
  private Checker SelectedChecker = null;
  private Checker CheckerToCapture = null;
  private BoardColors CurrentTurn = BoardColors.Black;
  private Array<Checker> BlackCheckers = new Array<Checker>();
  private Array<Checker> WhiteCheckers = new Array<Checker>();
  private Array<Checker> AllCheckers = new Array<Checker>();
  private Board board;
  private GameOverMenu gameOverMenu;
  private ProgressBar BlackPlayerTimer;
  private ProgressBar WhitePlayerTimer;
  private Panel BlackPlayerPortraitBackground;
  private Panel WhitePlayerPortraitBackground;
  private Array<Checker> checkersWithCaptureMoves = new Array<Checker>();
  private bool LastMoveWasCapture = false;

  public override void _Ready()
  {
    GetTree().Paused = false;
    // Registering events and subscribing to them, since this is the system that will handle them.
    // To Turn this multiplayer, I believe this will need to be server side since it is what keeps track all the checkers in play
    EventRegistry.RegisterEvent("TileClicked");
    EventRegistry.RegisterEvent("CheckerClicked");
    EventSubscriber.SubscribeToEvent("TileClicked", OnTileClicked);
    EventSubscriber.SubscribeToEvent("CheckerClicked", OnCheckerClicked);

    BlackPlayerTimer = GetNode<ProgressBar>("%BlackPlayerTimer");
    WhitePlayerTimer = GetNode<ProgressBar>("%WhitePlayerTimer");
    BlackPlayerPortraitBackground = GetNode<Panel>("%BlackPlayerPortraitBackground");
    WhitePlayerPortraitBackground = GetNode<Panel>("%WhitePlayerPortraitBackground");
    WhitePlayerPortraitBackground.Visible = false;
    BlackPlayerPortraitBackground.Visible = true;
    board = GetNode<Board>("%Board");
    gameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
    gameOverMenu.Hide();

    board.InitializeBoard();
    GenerateCheckers();
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

  private void OnTurnStart()
  {
    if (CurrentTurn == BoardColors.Black)
    {
      WhitePlayerPortraitBackground.Visible = false;
      BlackPlayerPortraitBackground.Visible = true;
    }
    else
    {
      BlackPlayerPortraitBackground.Visible = false;
      WhitePlayerPortraitBackground.Visible = true;
    }
    GetNode<Label>("%BlackPieceCount").Text = $"{BlackCheckers.Count}";
    GetNode<Label>("%WhitePieceCount").Text = $"{WhiteCheckers.Count}";
    LastMoveWasCapture = false;
    checkersWithCaptureMoves = new Array<Checker>();
    checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
    foreach (var checker in checkersWithCaptureMoves)
    {
      checker.SelectCapture();
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

  private void GenerateCheckers()
  {

    // TODO:
    // Get boardscheme from game state
    // Create checkers based on the board scheme and add the id data with them

    foreach (DictionaryEntry pair in initialBoard)
    {
      if (pair.Value == null) continue;
      Tile tile = board.FindTileByName(pair.Key.ToString());
      BoardColors checkerColor = pair.Value.ToString().Contains("W") ? BoardColors.White : BoardColors.Black;
      var checker = checkerScene.Instance<Checker>();
      checker.Name = pair.Value.ToString();
      checker.BoardPosition = new Vector2(tile.TilePosition.x, tile.TilePosition.y);
      if (checkerColor == BoardColors.Black)
        BlackCheckers.Add(checker);
      else
        WhiteCheckers.Add(checker);
      AllCheckers.Add(checker);
      tile.AddChild(checker);
      checker.SetCheckerColor(checkerColor);
    }

  }


  public void ClearCheckers()
  {
    foreach (Checker checker in AllCheckers)
    {
      checker.GetParent().RemoveChild(checker);
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

          if (CurrentTurn == BoardColors.Black)
          {
            if (tile.TilePosition.y == BoardSize - 1)
              SelectedChecker.SetKing();
          }
          else if (CurrentTurn == BoardColors.White)
            if (tile.TilePosition.y == 0)
              SelectedChecker.SetKing();

          board.CleanUpFreeTiles(SelectedChecker);
          SelectedChecker.UnselectChecker();
          NextTurn();
        }
      }
  }

  private bool IsCaptureMove(Vector2 currentPosition, Vector2 targetPosition)
  {

    var direction = new Vector2(Mathf.Sign(targetPosition.x - currentPosition.x), Mathf.Sign(targetPosition.y - currentPosition.y));
    Vector2 midPosition = targetPosition - direction;

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
    }
    if (LastMoveWasCapture && board.HasCaptureMove(SelectedChecker, AllCheckers))
    {
      OnTurnStart();
      return;
    }
    CurrentTurn = CurrentTurn == BoardColors.Black ? BoardColors.White : BoardColors.Black;
    SelectedChecker = null;

    // clean up capture status of checkers
    foreach (var checker in checkersWithCaptureMoves)
    {
      checker.UnselectChecker();
    }
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
    AllCheckers.Remove(checker);
    checker.QueueFree();

    if (BlackCheckers.Count == 0)
    {
      GD.Print("White wins!");
      gameOverMenu.SetWinnerName(BoardColors.White);
      gameOverMenu.Show();
      GetTree().Paused = true;
    }
    else if (WhiteCheckers.Count == 0)
    {
      GD.Print("Black wins!");
      gameOverMenu.SetWinnerName(BoardColors.Black);
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
    EventRegistry.UnregisterEvent("TileClicked");
    EventRegistry.UnregisterEvent("CheckerClicked");
  }
}

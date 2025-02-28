using Godot;
using Godot.Collections;
using static GameState;
using System.Collections;
using Newtonsoft.Json;
using System.Threading.Tasks;

public partial class GameplaySystem : Node2D
{
  [Export] private PackedScene checkerScene;
  [Export] private Texture[] playerPortraits;
  [Export] private Texture[] pieceCountTextures;
  private TextureRect OpponentPieceCountTexture;
  private TextureRect PlayerPieceCountTexture;
  private Checker SelectedChecker = null;
  private BoardColors CurrentTurn = BoardColors.Black;
  private Array<Checker> BlackCheckers = new Array<Checker>();
  private Array<Checker> WhiteCheckers = new Array<Checker>();
  private Array<Checker> AllCheckers = new Array<Checker>();
  private Board board;
  private GameOverMenu gameOverMenu;
  private ProgressBar OpponentTimer;
  private ProgressBar PlayerTimer;
  private Panel OpponentPortraitBackground;
  private Panel PlayerPortraitBackground;
  private Label OpponentNameLabel;
  private Label PlayerNameLabel;
  private TextureRect PlayerPortrait;
  private TextureRect OpponentPortrait;
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
    EventSubscriber.SubscribeToEvent("OnMovePiece", OnMovePiece);
    EventSubscriber.SubscribeToEvent("OnTimerUpdate", OnTimerUpdate);
    EventSubscriber.SubscribeToEvent("OnTurnSwitch", OnTurnSwitch);

    PlayerPortrait = GetNode<TextureRect>("%PlayerPortrait");
    OpponentPortrait = GetNode<TextureRect>("%OpponentPortrait");
    OpponentPieceCountTexture = GetNode<TextureRect>("%OpponentPieceCountTexture");
    PlayerPieceCountTexture = GetNode<TextureRect>("%PlayerPieceCountTexture");

    OpponentTimer = GetNode<ProgressBar>("%OpponentTimer");
    PlayerTimer = GetNode<ProgressBar>("%PlayerTimer");

    OpponentPortraitBackground = GetNode<Panel>("%OpponentPortraitBackground");
    PlayerPortraitBackground = GetNode<Panel>("%PlayerPortraitBackground");
    PlayerPortraitBackground.Visible = CurrentTurn == currentGameColor;
    OpponentPortraitBackground.Visible = CurrentTurn != currentGameColor;

    PlayerNameLabel = GetNode<Label>("%PlayerNameLabel");
    OpponentNameLabel = GetNode<Label>("%OpponentNameLabel");
    PlayerNameLabel.Text = player.name;
    OpponentNameLabel.Text = opponentName;

    board = GetNode<Board>("%Board");
    gameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
    gameOverMenu.Hide();

    PlayerPortrait.Texture = currentGameColor == BoardColors.Black ? playerPortraits[0] : playerPortraits[1];
    OpponentPortrait.Texture = currentGameColor == BoardColors.White ? playerPortraits[0] : playerPortraits[1];

    PlayerPieceCountTexture.Texture = currentGameColor == BoardColors.Black ? pieceCountTextures[0] : pieceCountTextures[1];
    OpponentPieceCountTexture.Texture = currentGameColor == BoardColors.White ? pieceCountTextures[0] : pieceCountTextures[1];
    board.InitializeBoard();
    GenerateCheckers();
    OnTurnStart();
  }

  private void OnTurnSwitch(object sender, object args)
  {
    if (args is string player_id)
    {
      GD.Print("___ SWITCHING TURNS __ ");
      NextTurn(player_id);
    }
  }

  private void OnMovePiece(object sender, object args)
  {
    if (args is MovePieceData data)
    {


      Tile toTile = board.FindTileByName(data.to);
      Tile fromTile = board.FindTileByName(data.from);
      Checker checker = fromTile.GetNode<Checker>(data.piece_id);
      checker.Move(toTile, toTile.TilePosition);
      /* fromTile.RemoveChild(checker);
      toTile.AddChild(checker); */

      if (data.is_capture)
      {
        Tile captureTile = GetCheckerToBeCaptured(fromTile, toTile);
        OnCheckerKilled(captureTile.GetChild<Checker>(captureTile.GetChildCount() - 1));
      }

      if (data.is_kinged)
      {
        checker.SetKing();
      }

      checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
      foreach (var c in checkersWithCaptureMoves)
      {
        c.UnselectChecker();
      }
      OnTurnStart();
    }
  }

  private void OnTurnStart()
  {
    GD.Print("ON_TURN_START_CURRENT TURN: " + CurrentTurn);
    if (CurrentTurn != currentGameColor) return;
    if ((CurrentTurn == BoardColors.Black && currentGameColor == BoardColors.Black) || (CurrentTurn == BoardColors.White && currentGameColor == BoardColors.White))
    {
      PlayerPortraitBackground.Visible = true;
      OpponentPortraitBackground.Visible = false;
    }
    else
    {
      PlayerPortraitBackground.Visible = false;
      OpponentPortraitBackground.Visible = true;
    }
    GetNode<Label>("%OpponentPieceCountLabel").Text = $"{(currentGameColor == BoardColors.Black ? WhiteCheckers.Count : BlackCheckers.Count)}";
    GetNode<Label>("%PlayerPieceCountLabel").Text = $"{(currentGameColor == BoardColors.White ? WhiteCheckers.Count : BlackCheckers.Count)}";
    LastMoveWasCapture = false;
    checkersWithCaptureMoves = new Array<Checker>();
    checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
    foreach (var checker in checkersWithCaptureMoves)
    {
      GD.Print($"selecting checker with id {checker.Name} on tile {checker.GetParent().Name}");
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
      if (board.HasCaptureMove(checker))
      {
        checkers.Add(checker);
      }
    }
    return checkers;
  }

  private void GenerateCheckers()
  {

    foreach (DictionaryEntry pair in initialBoard)
    {
      if (pair.Value == null) continue;
      Tile tile = board.FindTileByName(pair.Key.ToString());

      Piece piece = JsonConvert.DeserializeObject<Piece>(pair.Value.ToString());

      BoardColors checkerColor = piece.type == "w" ? BoardColors.White : BoardColors.Black;
      var checker = checkerScene.Instance<Checker>();
      checker.Name = piece.piece_id;
      checker.BoardPosition = new Vector2(tile.TilePosition.x, tile.TilePosition.y);
      if (checkerColor == BoardColors.Black)
        BlackCheckers.Add(checker);
      else
        WhiteCheckers.Add(checker);
      AllCheckers.Add(checker);
      checker.RectMinSize = tile.GetParent<GridContainer>().RectSize / 8;
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
    if (currentGameColor != CurrentTurn) return;
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
    if (currentGameColor != CurrentTurn) return;
    if (SelectedChecker != null)
      if (args is Tile tile)
      {
        if (SelectedChecker.MovementSpaces.Contains(tile))
        {
          if (board.HasCaptureMove(SelectedChecker))
          {
            Tile fromTile = SelectedChecker.GetParent<Tile>();
            Tile captureTile = GetCheckerToBeCaptured(fromTile, tile);
            OnCheckerKilled(captureTile.GetChild<Checker>(captureTile.GetChildCount() - 1));
            LastMoveWasCapture = true;
          }
          if (CurrentTurn == BoardColors.Black)
          {
            if (tile.Name[0] == 'H')
              SelectedChecker.SetKing();
          }
          else if (CurrentTurn == BoardColors.White)
            if (tile.Name[0] == 'A')
              SelectedChecker.SetKing();


          MovePieceData movePieceData = new MovePieceData();
          movePieceData.player_id = player.id;
          movePieceData.piece_id = SelectedChecker.Name;
          movePieceData.from = SelectedChecker.GetParent<Tile>().Name;
          movePieceData.to = tile.Name;
          movePieceData.is_capture = LastMoveWasCapture;
          movePieceData.is_kinged = SelectedChecker.isKing;

          SelectedChecker.Move(tile, tile.TilePosition);

          EventRegistry.GetEventPublisher("SendMessage").RaiseEvent(movePieceData);

          board.CleanUpFreeTiles(SelectedChecker);
          SelectedChecker.UnselectChecker();
          foreach (var c in checkersWithCaptureMoves)
          {
            c.UnselectChecker();
          }
          if (LastMoveWasCapture && board.HasCaptureMove(SelectedChecker))
          {
            OnTurnStart();
          }
        }
      }
  }

  private Tile GetCheckerToBeCaptured(Tile fromTile, Tile toTile)
  {
    string fromTileName = fromTile.Name;
    string toTileName = toTile.Name;
    char letter;
    int number;
    if (fromTileName[0] > toTileName[0])
    {
      letter = (char)(toTileName[0] + 1);

    }
    else
    {
      letter = (char)(toTileName[0] - 1);

    }

    int toTileNumber = int.Parse(toTileName[1].ToString());
    int fromTileNumber = int.Parse(fromTileName[1].ToString());

    if (toTileNumber > fromTileNumber)
    {
      number = toTileNumber - 1;
    }
    else
    {
      number = toTileNumber + 1;
    }

    string tileName = letter.ToString() + number.ToString();
    GD.Print($"target: {tileName}");
    return board.FindTileByName(tileName);
  }
  private void NextTurn(string player_id)
  {
    if (SelectedChecker != null)
    {
      board.CleanUpFreeTiles(SelectedChecker);
      SelectedChecker.UnselectChecker();
    }

    GD.Print("from turn: " + CurrentTurn);

    CurrentTurn = CurrentTurn == BoardColors.Black ? BoardColors.White : BoardColors.Black;

    SelectedChecker = null;
    GD.Print("to turn: " + CurrentTurn);

    // clean up capture status of checkers
    checkersWithCaptureMoves = new Array<Checker>();
    checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
    foreach (var checker in checkersWithCaptureMoves)
    {
      checker.UnselectChecker();
    }
    PlayerTimer.Value = 15;
    OpponentTimer.Value = 15;
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
    checker.GetParent().RemoveChild(checker);
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

  public void OnTimerUpdate(object sender, object args)
  {
    if (args is GameTimer timerData)
    {
      /* if (player.id == timerData.current_player_id)
      {
        if (currentGameColor != CurrentTurn)
        {
          CurrentTurn = currentGameColor;
          OnTurnStart();
        }
      } */
      if (currentGameColor == CurrentTurn)
      {
        PlayerTimer.Value = timerData.player_timer;
      }
      else
      {
        OpponentTimer.Value = timerData.player_timer;
      }
    }
  }

  // In the case of a restart, this function will be called
  // So we do a cleanup here to free the memory no longer used
  // We remove tiles or any event subscription, etc...
  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnTimerUpdate", OnTimerUpdate);
    EventSubscriber.UnsubscribeFromEvent("OnMovePiece", OnMovePiece);
    EventSubscriber.UnsubscribeFromEvent("TileClicked", OnTileClicked);
    EventSubscriber.UnsubscribeFromEvent("CheckerClicked", OnCheckerClicked);
    EventRegistry.UnregisterEvent("TileClicked");
    EventRegistry.UnregisterEvent("CheckerClicked");
  }
}

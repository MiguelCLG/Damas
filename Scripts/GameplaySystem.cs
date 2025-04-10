using Godot;
using Godot.Collections;
using System.Collections;
using Newtonsoft.Json;
using static GameState;
using static Utils;

public partial class GameplaySystem : Node2D
{
	[Export] private PackedScene checkerScene;
	[Export] private Texture[] playerPortraits;
	[Export] private Texture[] pieceCountTextures;
	/*  [Export] private Texture[] bidTextures; */
	[Export] private AudioOptionsResource captureSound;
	[Export] private AudioOptionsResource moveSound;
	[Export] private AudioOptionsResource kingSound;
	[Export] private AudioOptionsResource winningSound;
	[Export] private AudioOptionsResource losingSound;
	private TextureRect OpponentPieceCountTexture;
	private TextureRect PlayerPieceCountTexture;
	private TextureRect OpponentDisconnectIcon;
	private Panel ConnectionPopup;
	private Checker SelectedChecker = null;
	private BoardColors CurrentTurn = BoardColors.Black;
	private Array<Checker> BlackCheckers = new Array<Checker>();
	private Array<Checker> WhiteCheckers = new Array<Checker>();
	private Array<Checker> AllCheckers = new Array<Checker>();
	private Board board;
	private GameOverMenu gameOverMenu;
	private ProgressBar OpponentTimer;
	private ProgressBar PlayerTimer;
	private VBoxContainer PlayerTurnBox;
	private VBoxContainer OpponentTurnBox;
	private Label OpponentNameLabel;
	private Label WinnngsLabel;
	private Label PlayerNameLabel;
	private TextureRect PlayerPortrait;
	private TextureRect OpponentPortrait;
	private Array<Checker> checkersWithCaptureMoves = new Array<Checker>();
	private bool LastMoveWasCapture = false;
	private AudioManager audioManager;
	AnimationPlayer PlayerTimerAnimationPlayer;
	AnimationPlayer OpponentTimerAnimationPlayer;
	public override void _Ready()
	{
		GetTree().Paused = false;
		RegisterEvents();
		SubscribeToEvents();
		Initialize();
	}

	private void Initialize()
	{
		audioManager = GetNode<AudioManager>("/root/AudioManager");
		PlayerPortrait = GetNode<TextureRect>("%PlayerPortrait");
		OpponentPortrait = GetNode<TextureRect>("%OpponentPortrait");
		OpponentPieceCountTexture = GetNode<TextureRect>("%OpponentPieceCountTexture");
		PlayerPieceCountTexture = GetNode<TextureRect>("%PlayerPieceCountTexture");
		OpponentDisconnectIcon = GetNode<TextureRect>("%OpponentDisconnectIcon");
		ConnectionPopup = GetNode<Panel>("%ConnectionPopup");

		GetNode<Label>("%GameIdLabel").Text = $"Round ID: {game_id}";
		PlayerTimerAnimationPlayer = GetNode<AnimationPlayer>("%PlayerTimerAnimationPlayer");
		OpponentTimerAnimationPlayer = GetNode<AnimationPlayer>("%OpponentTimerAnimationPlayer");

		OpponentDisconnectIcon.Visible = false;

		OpponentTimer = GetNode<ProgressBar>("%OpponentTimer");
		PlayerTimer = GetNode<ProgressBar>("%PlayerTimer");

		PlayerTimer.MaxValue = MaxTimer;
		OpponentTimer.MaxValue = MaxTimer;
		PlayerTimer.Value = MaxTimer;
		OpponentTimer.Value = MaxTimer;

		CurrentTurn = currentInGameTurn;
		PlayerTurnBox = GetNode<VBoxContainer>("%PlayerTurn");
		OpponentTurnBox = GetNode<VBoxContainer>("%OpponentTurn");
		bool isPlayerBoxVisible = CurrentTurn == currentGameColor;
		PlayerTurnBox.Modulate = isPlayerBoxVisible ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0); // using modulate because Visible removes the container virtually and it allows the other containers to stretch
		OpponentTurnBox.Modulate = !isPlayerBoxVisible ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);

		PlayerNameLabel = GetNode<Label>("%PlayerNameLabel");
		OpponentNameLabel = GetNode<Label>("%OpponentNameLabel");
		WinnngsLabel = GetNode<Label>("%WinningsLabel2");
		PlayerNameLabel.Text = player.name;
		OpponentNameLabel.Text = opponentName;
		WinnngsLabel.Text = FormatMoneyText(possibleWinnings.ToString(), GameState.Currency);

		board = GetNode<Board>("%Board");
		gameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
		gameOverMenu.Hide();

		ConnectionPopup.Hide();
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
		if (args is string)
		{
			NextTurn();
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

			if (data.is_capture)
			{
				audioManager?.Play(captureSound, this);
				Tile captureTile = GetCheckerToBeCaptured(fromTile, toTile);
				OnCheckerKilled(captureTile.GetChild<Checker>(captureTile.GetChildCount() - 1));
			}
			else audioManager?.Play(moveSound, this);

			if (data.is_kinged)
			{
				audioManager?.Play(kingSound, this);
				checker.SetKing();
			}

			checkersWithCaptureMoves = FindCheckersWithCaptureMoves();
			foreach (var c in checkersWithCaptureMoves)
			{
				c.UnselectChecker();
			}
			fromTile.SelectAsPreviousMovement();
			toTile.SelectAsPreviousMovement();
			OnTurnStart();
		}
	}

	private void OnTurnStart()
	{
		RaiseCheckersOpacity();
		PlayerTimerAnimationPlayer.Stop();
		OpponentTimerAnimationPlayer.Stop();
		if ((CurrentTurn == BoardColors.Black && currentGameColor == BoardColors.Black) || (CurrentTurn == BoardColors.White && currentGameColor == BoardColors.White))
		{
			PlayerTurnBox.Modulate = new Color(1, 1, 1, 1);
			OpponentTurnBox.Modulate = new Color(1, 1, 1, 0);
		}
		else
		{
			PlayerTurnBox.Modulate = new Color(1, 1, 1, 0);
			OpponentTurnBox.Modulate = new Color(1, 1, 1, 1);
		}
		GetNode<Label>("%OpponentPieceCountLabel").Text = $"{(currentGameColor == BoardColors.Black ? WhiteCheckers.Count : BlackCheckers.Count)}";
		GetNode<Label>("%PlayerPieceCountLabel").Text = $"{(currentGameColor == BoardColors.White ? WhiteCheckers.Count : BlackCheckers.Count)}";
		if (CurrentTurn != currentGameColor) return;

		LastMoveWasCapture = false;
		checkersWithCaptureMoves = new Array<Checker>();
		checkersWithCaptureMoves = FindCheckersWithCaptureMoves();


		foreach (var checker in AllCheckers)
		{
			if (checker.Color == CurrentTurn)
			{
				if (checkersWithCaptureMoves.Count == 0)
				{
					if (board.CanMove(checker))
						checker.GetParent<Tile>().SelectAsCanMove();
					else
						LowerCheckerOpacity(checker); // if it cant move, then turn opacity to 0.8

				}
				else
				{
					if (board.HasCaptureMove(checker))
					{
						checker.SelectCapture();
						checker.GetParent<Tile>().SelectAsCanCapture();
					}
					else
						LowerCheckerOpacity(checker);
				}
			}
			else
			{

				LowerCheckerOpacity(checker);// if it is not its turn, then turn opacity to 0.8
			}
		}
	}

	private void RaiseCheckersOpacity()
	{
		foreach (var checker in AllCheckers)
		{
			checker.Modulate = new Color(1, 1, 1, 1);
			checker.GetParent<Tile>().Unselect();
		}
	}
	private void LowerCheckerOpacity(Checker checker)
	{
		checker.Modulate = new Color(1, 1, 1, 0.8f);

	}


	private Array<Checker> FindCheckersWithCaptureMoves()
	{
		var checkersWithCaptureMove = new Array<Checker>();
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
				checkersWithCaptureMove.Add(checker);
				checker.Modulate = new Color(1, 1, 1, 1);
			}

		}
		return checkersWithCaptureMove;
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
			if (piece.is_kinged)
				checker.SetKing();
		}

	}

	public void ClearCheckers()
	{
		foreach (Checker checker in AllCheckers)
		{
			checker.GetParent().RemoveChild(checker);
		}
	}

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
					board.CleanUpMovementTiles();
					board.CleanUpCaptureTiles(); //TODO: Make another button texture for obligatory catching (like the green but for capture)
					SelectedChecker = checker;
					checker.SelectChecker();

					// Getting info about the checkers so that the board can get the available tiles
					var allCheckers = new Array<Checker>();
					foreach (Checker c in BlackCheckers)
						allCheckers.Add(c);

					foreach (Checker c in WhiteCheckers)
						allCheckers.Add(c);

					board.OnCheckerClicked(checker);
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
					board.OnCheckerClicked(checker);
				}
			}
		}
	}

	private void OnTileClicked(object sender, object args)
	{
		if (currentGameColor != CurrentTurn) return;
		if (SelectedChecker != null)
			if (args is Tile tile)
			{
				if (SelectedChecker.MovementSpaces.Contains(tile))
				{
					var thisMoveWasKinged = false;
					if (board.HasCaptureMove(SelectedChecker))
					{
						Tile startingTile = SelectedChecker.GetParent<Tile>();
						lastCheckerCaptureTile = GetCheckerToBeCaptured(startingTile, tile);
						lastCheckerCaptured = lastCheckerCaptureTile.GetChild<Checker>(lastCheckerCaptureTile.GetChildCount() - 1);
						OnCheckerKilled(lastCheckerCaptured);
						LastMoveWasCapture = true;
					}
					if (!SelectedChecker.isKing)
					{
						if (CurrentTurn == BoardColors.Black)
						{
							if (tile.Name[0] == 'H')
							{
								SelectedChecker.SetKing();
								thisMoveWasKinged = true;
							}
						}
						else if (CurrentTurn == BoardColors.White)
							if (tile.Name[0] == 'A')
							{
								SelectedChecker.SetKing();
								thisMoveWasKinged = true;
							}
					}


					MovePieceData movePieceData = new MovePieceData();
					movePieceData.player_id = player.id;
					movePieceData.piece_id = SelectedChecker.Name;
					movePieceData.from = SelectedChecker.GetParent<Tile>().Name;
					movePieceData.to = tile.Name;
					movePieceData.is_capture = LastMoveWasCapture;
					movePieceData.is_kinged = thisMoveWasKinged;

					if (LastMoveWasCapture)
					{
						audioManager?.Play(captureSound, this);
					}
					else
					{
						audioManager?.Play(moveSound, this);
					}

					if (thisMoveWasKinged)
					{
						audioManager?.Play(kingSound, this);
					}
					SelectedChecker.Move(tile, tile.TilePosition);
					lastMove = movePieceData;
					EventRegistry.GetEventPublisher("SendMessage").RaiseEvent(movePieceData);

					board.CleanUpFreeTiles();

					SelectedChecker.UnselectChecker();
					foreach (var c in checkersWithCaptureMoves)
					{
						c.UnselectChecker();
					}
					if (LastMoveWasCapture && board.HasCaptureMove(SelectedChecker))
					{
						SelectedChecker.SelectCapture();
						SelectedChecker.OnCheckerClicked();

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

	private void NextTurn()
	{
		if (SelectedChecker != null)
		{
			board.CleanUpFreeTiles();
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
	}

	private void RevertCapture()
	{
		if (lastCheckerCaptured.Color == BoardColors.Black)
			BlackCheckers.Add(lastCheckerCaptured);
		else
			WhiteCheckers.Add(lastCheckerCaptured);
		AllCheckers.Add(lastCheckerCaptured);
		lastCheckerCaptureTile.AddChild(lastCheckerCaptured);
	}
	private void ToggleReconnectPopup(object sender, object args)
	{
		ConnectionPopup.Visible = args is bool isVisible && isVisible;
	}

	private void OnGameOver(object sender, object args)
	{
		if (args is GameOver gameOverInfo)
		{
			GD.Print(player.id, gameOverInfo.winner.id);
			if (gameOverInfo.winner.id == player.id)
			{
				audioManager.StopSound(this);
				audioManager.Play(winningSound, this);
				gameOverMenu?.SetWinnerName(currentGameColor, gameOverInfo.winnings);
			}
			else
			{
				audioManager.StopSound(this);
				audioManager.Play(losingSound, this);
				gameOverMenu?.SetWinnerName(currentGameColor == BoardColors.Black ? BoardColors.White : BoardColors.Black);
			}
			GetNode<Panel>("%ConcedeConfirmation").Visible = false;
			gameOverMenu?.Show();
			GetTree().Paused = true;
		}
	}

	public void OnTimerUpdate(object sender, object args)
	{
		if (args is GameTimer timerData)
		{
			BoardColors opponentColor = currentGameColor == BoardColors.Black ? BoardColors.White : BoardColors.Black;

			GD.Print(player.id.Equals(timerData.current_player_id));
			CurrentTurn = player.id.Equals(timerData.current_player_id) ?
			currentGameColor : opponentColor;

			if (currentGameColor == CurrentTurn)
			{
				PlayerTimer.Value = timerData.player_timer;
				if (timerData.player_timer <= 30)
				{

					if (PlayerTimerAnimationPlayer.CurrentAnimation != "time_30_seconds")
					{
						PlayerTimerAnimationPlayer.Play("time_30_seconds");
					}
				}
			}
			else
			{
				OpponentTimer.Value = timerData.player_timer;
				if (timerData.player_timer <= 30)
				{
					if (OpponentTimerAnimationPlayer.CurrentAnimation != "time_30_seconds")
						OpponentTimerAnimationPlayer.Play("time_30_seconds");
				}
			}
		}
	}
	private void OnInvalidMove(object sender, object args)
	{
		//revert checker last move

		Tile toTile = board.FindTileByName(lastMove.to);
		Tile fromTile = board.FindTileByName(lastMove.from);
		Checker checkerToRevert = toTile.GetNode<Checker>(lastMove.piece_id);
		toTile.RemoveChild(checkerToRevert);
		fromTile.AddChild(checkerToRevert);

		if (lastMove.is_kinged)
			checkerToRevert.RevertKing();
		if (lastMove.is_capture)
			RevertCapture();
	}

	public void OnConcedeClicked()
	{
		GetNode<Panel>("%ConcedeConfirmation").Visible = true;
	}

	public void OnConcedeConfirmed()
	{
		// TODO: send concede message to server
		EventRegistry.GetEventPublisher("OnConcede").RaiseEvent(null);
		OnCancelConcede();
	}

	public void OnCancelConcede()
	{
		GetNode<Panel>("%ConcedeConfirmation").Visible = false;
	}

	private void OnOpponentDisconnectedGame(object sender, object args)
	{
		if (args is string status)
		{
			if (status == "disconnected")
			{

				OpponentDisconnectIcon.Visible = true;
			}
			else
			{
				OpponentDisconnectIcon.Visible = false;
			}
		}
	}

	public void OnGameReconnect(object sender, object args) => Initialize();

	public void SubscribeToEvents()
	{
		EventSubscriber.SubscribeToEvent("TileClicked", OnTileClicked);
		EventSubscriber.SubscribeToEvent("CheckerClicked", OnCheckerClicked);
		EventSubscriber.SubscribeToEvent("OnMovePiece", OnMovePiece);
		EventSubscriber.SubscribeToEvent("OnTimerUpdate", OnTimerUpdate);
		EventSubscriber.SubscribeToEvent("OnTurnSwitch", OnTurnSwitch);
		EventSubscriber.SubscribeToEvent("OnGameOver", OnGameOver);
		EventSubscriber.SubscribeToEvent("OnGameReconnect", OnGameReconnect);
		EventSubscriber.SubscribeToEvent("ToggleReconnectPopup", ToggleReconnectPopup);
		EventSubscriber.SubscribeToEvent("OnOpponentDisconnectedGame", OnOpponentDisconnectedGame);
		EventSubscriber.SubscribeToEvent("OnInvalidMove", OnInvalidMove);

	}

	public void RegisterEvents()
	{
		// Registering events and subscribing to them, since this is the system that will handle them.
		// To Turn this multiplayer, I believe this will need to be server side since it is what keeps track all the checkers in play
		EventRegistry.RegisterEvent("TileClicked");
		EventRegistry.RegisterEvent("CheckerClicked");
	}

	// In the case of a restart, this function will be called
	// So we do a cleanup here to free the memory no longer used
	// We remove tiles or any event subscription, etc...
	public override void _ExitTree()
	{
		audioManager.StopSound(this);
		EventSubscriber.UnsubscribeFromEvent("OnInvalidMove", OnInvalidMove);
		EventSubscriber.UnsubscribeFromEvent("TileClicked", OnTileClicked);
		EventSubscriber.UnsubscribeFromEvent("CheckerClicked", OnCheckerClicked);
		EventSubscriber.UnsubscribeFromEvent("OnMovePiece", OnMovePiece);
		EventSubscriber.UnsubscribeFromEvent("OnTimerUpdate", OnTimerUpdate);
		EventSubscriber.UnsubscribeFromEvent("OnTurnSwitch", OnTurnSwitch);
		EventSubscriber.UnsubscribeFromEvent("OnGameOver", OnGameOver);
		EventSubscriber.UnsubscribeFromEvent("OnGameReconnect", OnGameReconnect);
		EventSubscriber.UnsubscribeFromEvent("ToggleReconnectPopup", ToggleReconnectPopup);
		EventSubscriber.UnsubscribeFromEvent("OnOpponentDisconnectedGame", OnOpponentDisconnectedGame);
		EventRegistry.UnregisterEvent("TileClicked");
		EventRegistry.UnregisterEvent("CheckerClicked");
	}
}

# Lab 2

With all the labs, you have write access to the repo and can add your own code to the _labs_ folder. Use a subdirectory with your name to avoid conflicts.

Use the Codebreaker initial solution.

## Trying out the solution

- Run the solution locally with Visual Studio and try to run it locally.

## Logging

- Add this strongly typed logging class

```csharp
public static partial class Log
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Error,
        Message = "{ErrorMessage}")]
    public static partial void Error(this ILogger logger, Exception ex, string errorMessage);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Warning,
        Message = "Game {GameId} not found")]
    public static partial void GameNotFound(this ILogger logger, Guid gameId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Invalid game type requested: {GameType}")]
    public static partial void InvalidGameType(this ILogger logger, string gameType);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "Invalid move received {GameId}, guesses: {Guesses}, {ErrorMessage}")]
    public static partial void InvalidMoveReceived(this ILogger logger, Guid gameId, string guesses, string errorMessage);

    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Information,
        Message = "The game {GameId} started")]
    public static partial void GameStarted(this ILogger logger, Guid gameId);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "The move {Move} was set for {GameId} with result {Result}")]
    public static partial void SendMove(this ILogger logger, string move, Guid gameId, string result);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Game won after {Moves} moves and {Seconds} seconds with game {GameId}")]
    private static partial void GameWon(this ILogger logger, int moves, int seconds, Guid gameId);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Information,
        Message = "Game lost after {Seconds} seconds with game {GameId}")]
    private static partial void GameLost(this ILogger logger, int seconds, Guid gameId);

    public static void GameEnded(this ILogger logger, Game game)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            if (game.IsVictory)
            {
                logger.GameWon(game.Moves.Count, game.Duration?.Seconds ?? 0, game.Id);
            }
            else
            {
                logger.GameLost(game.Duration?.Seconds ?? 0, game.Id);
            }
        }
    }

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Query for game {GameId}")]
    public static partial void QueryGame(this ILogger logger, Guid gameId);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Returned {NumberGames} games using {Query}")]
    private static partial void QueryGames(this ILogger logger, int numberGames, string query);

    public static void QueryGames(this ILogger logger, IEnumerable<Game> games, string query)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            QueryGames(logger, games.Count(), query);
        }
    }
}
```

- Use the custom `Log` class from the `GamesService` class, add logging
- Test the application to check logging information

## Metrics

- Create the `GamesMetrics` class

```csharp
public sealed class GamesMetrics : IDisposable
{
    public const string MeterName = "Codebreaker.Games";
    public const string Version = "1.0";
    private readonly Meter _meter;

    private readonly UpDownCounter<long> _activeGamesCounter;
    private readonly Histogram<double> _gameDuration;
    private readonly Histogram<double> _moveThinkTime;
    private readonly Histogram<int> _movesPerGameWin;
    private readonly Counter<long> _invalidMoveCounter;
    private readonly Counter<long> _gamesWonCounter;
    private readonly Counter<long> _gamesLostCounter;

    private readonly ConcurrentDictionary<Guid, DateTime> _moveTimes = new();

    public GamesMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName, Version);

        _activeGamesCounter = _meter.CreateUpDownCounter<long>(
            "codebreaker.active_games",
            unit: "{games}",
            description: "Number of games that are currently active on the server.");

        _gameDuration = _meter.CreateHistogram<double>(
            "codebreaker.game_duration",
            unit: "s",
            description: "Duration of a game in seconds.");

        _moveThinkTime = _meter.CreateHistogram<double>(
            "codebreaker.move_think_time",
            unit: "s",
            description: "Think time of a move in seconds.");

        _movesPerGameWin = _meter.CreateHistogram<int>(
            "codebreaker.game_moves-per-win",
            unit: "{moves}",
            description: "The number of moves needed for a game win");

        _invalidMoveCounter = _meter.CreateCounter<long>(
            "codebreaker.invalid_moves",
            unit: "{moves}",
            description: "Number of invalid moves.");

        _gamesWonCounter = _meter.CreateCounter<long>(
            "codebreaker.games.won",
            unit: "{won}",
            description: "Number of games won.");

        _gamesLostCounter = _meter.CreateCounter<long>(
            "codebreaker.games.lost",
            unit: "{lost}",
            description: "Number of games lost.");
    }

    private static KeyValuePair<string, object?> CreateGameTypeTag(string gameType) => KeyValuePair.Create<string, object?>("GameType", gameType);
    private static KeyValuePair<string, object?> CreateGameIdTag(Guid id) => KeyValuePair.Create<string, object?>("GameId", id.ToString());

    public void GameStarted(Game game)
    {
        if (_moveThinkTime.Enabled)
        {
            _moveTimes.TryAdd(game.Id, game.StartTime);
        }

        if (_activeGamesCounter.Enabled)
        {
            _activeGamesCounter.Add(1, CreateGameTypeTag(game.GameType));
        }
    }

    public void MoveSet(Guid id, DateTime moveTime, string gameType)
    {
        if (_moveThinkTime.Enabled)
        {
            _moveTimes.AddOrUpdate(id, moveTime, (id1, prevTime) =>
            {
                _moveThinkTime.Record((moveTime - prevTime).TotalSeconds, [CreateGameIdTag(id1), CreateGameTypeTag(gameType)]);
                return moveTime;
            });
        }
    }

    public void InvalidMove()
    {
        if (_invalidMoveCounter.Enabled)
        {
            _invalidMoveCounter.Add(1);
        }
    }

    public void GameEnded(Game game)
    {
        if (!game.HasEnded())
        {
            return;
        }
        if (_gameDuration.Enabled && game.Duration is not null)
        {
            _gameDuration.Record(game.Duration.Value.TotalSeconds, CreateGameTypeTag(game.GameType)); // game.Duration is not null if Ended() is true
        }
        if (_activeGamesCounter.Enabled)
        {
            _activeGamesCounter.Add(-1, CreateGameTypeTag(game.GameType));
        }
        if (game.IsVictory && _movesPerGameWin.Enabled)
        {
            _movesPerGameWin.Record(game.LastMoveNumber, CreateGameTypeTag(game.GameType));
        }
        if (game.IsVictory && _gamesWonCounter.Enabled)
        {
            _gamesWonCounter.Add(1, CreateGameTypeTag(game.GameType));
        }
        if (!game.IsVictory && _gamesLostCounter.Enabled)
        {
            _gamesLostCounter.Add(1, CreateGameTypeTag(game.GameType));
        }

        _moveTimes.TryRemove(game.Id, out _);
    }

    public void Dispose() => _meter?.Dispose();
}
```

- Add metrics to the DI container

```csharp
        builder.Services.AddMetrics();

        builder.Services.AddOpenTelemetry().WithMetrics(m => m.AddMeter(GamesMetrics.MeterName));

        builder.Services.AddSingleton<GamesMetrics>();
```

- Test the application to check metrics information

## Distributed Tracing

- Configure the DI container

```csharp
        const string ActivitySourceName = "Codebreaker.GameAPIs";
        const string ActivitySourceVersion = "1.0.0";

        builder.Services.AddKeyedSingleton(ActivitySourceName, (services, _) =>
            new ActivitySource(ActivitySourceName, ActivitySourceVersion));
```

- Inject the ActivitySource with the `GamesService`

```csharp
[FromKeyedServices("Codebreaker.GameAPIs")] ActivitySource activitySource
```

- Create and start an activity, and set the status:

```csharp
        Game game;
        using var activity = activitySource.CreateActivity("StartGame", ActivityKind.Server);
        try
        {
            game = GamesFactory.CreateGame(gameType, playerName);
            activity?.AddTag(GameTypeTagName, game.GameType)
                .AddTag(GameIdTagName, game.Id.ToString())
                .Start();

            await dataRepository.AddGameAsync(game, cancellationToken);
            metrics.GameStarted(game);
            logger.GameStarted(game.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);
```

- Do this with the game move as well.
- Monitor activities of the application.

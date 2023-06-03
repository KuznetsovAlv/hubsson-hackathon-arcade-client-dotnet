using Hubsson.Hackathon.Arcade.Client.Dotnet.Contracts;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Domain;
using ClientGameState = Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.ClientGameState;

namespace Hubsson.Hackathon.Arcade.Client.Dotnet.Services
{
    public class MatchService
    {
        private readonly Direction[] directions = new[]
        {
            Direction.Right,
            Direction.Down,
            Direction.Left,
            Direction.Up
        };

        private MatchRepository _matchRepository;
        private ArcadeSettings _arcadeSettings;
        
        public MatchService(ArcadeSettings settings)
        {
            _matchRepository = new MatchRepository();
            _arcadeSettings = settings;
        }
        
        public void Init() { }

        public Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.Action Update(ClientGameState gameState)
        {
            InitializeBoardIfNeed(gameState.width, gameState.height);
            UpdateBoard(gameState);
            var direction = GetNextDirection(gameState);
            return new Domain.Action
            {
                direction = direction,
                iteration = _matchRepository.CurrentIteration++,
            };
        }

        private void InitializeBoardIfNeed(int width, int height)
        {
            if (_matchRepository.Board == null)
            {
                _matchRepository.Board = new bool[height][];
                for (var i = 0; i < _matchRepository.Board.Length; i++)
                    _matchRepository.Board[i] = new bool[width];
            }
        }

        private void UpdateBoard(ClientGameState gameState)
        {
            foreach (var player in gameState.players)
            {
                var lastCoordinate = player.coordinates.LastOrDefault();
                if (lastCoordinate != null)
                    _matchRepository.Board[lastCoordinate.y][lastCoordinate.x] = true;
            }
        }

        private Direction GetNextDirection(ClientGameState gameState)
        {
            while (IsThereObstacle(gameState))
                _matchRepository.CurrentDirectionIndex = (_matchRepository.CurrentDirectionIndex + 1) % 4;
            return directions[_matchRepository.CurrentDirectionIndex];
        }

        private bool IsThereObstacle(ClientGameState gameState)
        {
            var our = gameState.players.First(p => p.playerId == _arcadeSettings.TeamId).coordinates.Last();
            var nextPossibleCoordinate = GetNextPossibleCoordinate(our);
            return nextPossibleCoordinate.x < 0 || 
                nextPossibleCoordinate.y < 0 ||
                nextPossibleCoordinate.x >= gameState.width ||
                nextPossibleCoordinate.y >= gameState.height ||
                _matchRepository.Board[nextPossibleCoordinate.y][nextPossibleCoordinate.x];
        }

        private Coordinate GetNextPossibleCoordinate(Coordinate our)
        {
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Right)
                return new Coordinate { x = our.x + 1, y = our.y };
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Left)
                return new Coordinate { x = our.x - 1, y = our.y };
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Down)
                return new Coordinate { x = our.x, y = our.y + 1 };
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Up)
                return new Coordinate { x = our.x, y = our.y - 1 };
            return null;
        }

        private class MatchRepository
        {
            public int CurrentDirectionIndex { get; set; } = 0;

            public int CurrentIteration { get; set; } = 0;

            public bool[][] Board { get; set; }
        }
    }
}
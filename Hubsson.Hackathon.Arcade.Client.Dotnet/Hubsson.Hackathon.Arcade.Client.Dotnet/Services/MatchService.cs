using Hubsson.Hackathon.Arcade.Client.Dotnet.Contracts;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Domain;
using ClientGameState = Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.ClientGameState;

namespace Hubsson.Hackathon.Arcade.Client.Dotnet.Services
{
    public class MatchService
    {
        private const int RIGHT_DIRECTION_INDEX = 0;
        private const int LEFT_DIRECTION_INDEX = 2;

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
        
        public void Init()
        {
            _matchRepository.CurrentDirectionIndex = -1;
            _matchRepository.CurrentIteration = 0;
            _matchRepository.Board = null;
        }

        public Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.Action Update(ClientGameState gameState)
        {
            InitializeBoardIfNeed(gameState.width, gameState.height);
            InitializeDirectionIndexIfNeed(gameState);
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
            // clean board
            if (_matchRepository.Board == null)
            {
                _matchRepository.Board = new bool[height][];
                for (var i = 0; i < _matchRepository.Board.Length; i++)
                    _matchRepository.Board[i] = new bool[width];
            }
        }

        private void InitializeDirectionIndexIfNeed(ClientGameState gameState)
        {
            if (_matchRepository.CurrentDirectionIndex == -1)
            {
                var ourCoordinates = GetOurPlayerCoordinates(gameState);
                if (ourCoordinates.x < gameState.width / 2)
                {
                    _matchRepository.CurrentDirectionIndex = RIGHT_DIRECTION_INDEX;
                }
                else
                {
                    _matchRepository.CurrentDirectionIndex = LEFT_DIRECTION_INDEX;
                }
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
            var ourCoordinates = GetOurPlayerCoordinates(gameState);
            var nextPossibleCoordinate = GetNextPossibleCoordinate(ourCoordinates);
            return nextPossibleCoordinate.x < 0 || 
                nextPossibleCoordinate.y < 0 ||
                nextPossibleCoordinate.x >= gameState.width ||
                nextPossibleCoordinate.y >= gameState.height ||
                _matchRepository.Board[nextPossibleCoordinate.y][nextPossibleCoordinate.x];
        }

        private (int x, int y) GetNextPossibleCoordinate(Coordinate our)
        {
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Right)
                return (our.x + 1, our.y);
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Left)
                return (our.x - 1, our.y);
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Down)
                return (our.x, our.y + 1);
            if (directions[_matchRepository.CurrentDirectionIndex] == Direction.Up)
                return (our.x, our.y - 1);
            return (0, 0);
        }

        private Coordinate GetOurPlayerCoordinates(ClientGameState gameState)
        {
            return gameState.players.First(p => p.playerId == _arcadeSettings.TeamId).coordinates.Last();
        }

        private class MatchRepository
        {
            public int CurrentDirectionIndex { get; set; } = -1;

            public int CurrentIteration { get; set; } = 0;

            public bool[][] Board { get; set; }
        }
    }
}
using System;

namespace Gameplay
{
    public class GameModel
    {
        public static GamePhase CurrentGamePhase => instance.currentGamePhase;
        
        private GamePhase currentGamePhase = GamePhase.IntroPhase;

        private static GameModel instance;

        public Action<GamePhase> GamePhaseChanged;
        
        public void Initialize()
        {
            instance = this;
        }

        public void SetGamePhase(GamePhase newPhase)
        {
            currentGamePhase = newPhase;
            GamePhaseChanged?.Invoke(currentGamePhase);
        }
    }
}
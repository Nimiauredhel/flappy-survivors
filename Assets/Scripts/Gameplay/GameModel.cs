using System;

namespace Gameplay
{
    public class GameModel
    {
        public static GamePhase CurrentGamePhase => instance.currentGamePhase;
        public static bool Won => instance.won;
        public static float TimeLeft => instance.timeLeft;
        
        public Action<GamePhase> GamePhaseChanged;
        
        private GamePhase currentGamePhase = GamePhase.IntroPhase;
        
        private static GameModel instance;
        private bool won = false;
        private float timeLeft;
        
        public void Initialize(float levelDuration)
        {
            timeLeft = levelDuration;
            instance = this;
        }

        public void SetGamePhase(GamePhase newPhase)
        {
            currentGamePhase = newPhase;
            GamePhaseChanged?.Invoke(currentGamePhase);
        }

        public static void ChangeTimeLeft(float delta)
        {
            instance.timeLeft += delta;
        }
    }
}
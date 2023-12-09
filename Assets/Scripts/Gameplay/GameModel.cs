using System;
using UnityEngine;

namespace Gameplay
{
    public class GameModel
    {
        public static GamePhase CurrentGamePhase => instance.currentGamePhase;
        public static bool Paused => instance.paused;
        public static bool CanPause => instance.canPause;
        public static bool Won => instance.won;
        public static float TimeLeft => instance.timeLeft;

        public Action<bool> GameSetPaused;
        public Action<GamePhase> GamePhaseChanged;
        
        private GamePhase currentGamePhase = GamePhase.None;
        
        private static GameModel instance;
        private bool paused = false;
        private bool canPause = true;
        private bool won = false;
        private float timeLeft;
        
        public void Initialize(float levelDuration)
        {
            timeLeft = levelDuration;
            instance = this;
        }

        public void TogglePause()
        {
            SetPaused(!paused);
        }

        public void SetPaused(bool value)
        {
            if (paused == value) return;
            if (!canPause) return;
            
            paused = value;
            GameSetPaused?.Invoke(value);
        }

        public void SetCanPause(bool value)
        {
            canPause = value;
        }

        public void SetWonGame()
        {
            won = true;
        }

        public void SetGamePhase(GamePhase newPhase)
        {
            if (newPhase == currentGamePhase) return;
            
            currentGamePhase = newPhase;
            GamePhaseChanged?.Invoke(currentGamePhase);
        }

        public static void ChangeTimeLeft(float delta)
        {
            instance.timeLeft += delta;
        }
    }
}
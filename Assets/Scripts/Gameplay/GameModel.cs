using System;
using Audio;
using UnityEngine;

namespace Gameplay
{
    public class GameModel
    {
        public static GamePhase CurrentGamePhase => instance.currentGamePhase;
        public static bool Paused => instance == null || instance.paused;
        public static bool CanPause => instance.canPause;
        public static bool Won => instance.won;
        public static float TimeLeft => instance.timeLeft;
        public static float TimeElapsed => instance.timeElapsed;

        public static int TotalDamageTaken => instance.totalDamageTaken;
        public static int TotalDamageDealt => instance.totalDamageDealt;
        public static int TotalEnemiesDestroyed => instance.totalEnemiesDestroyed;

        public Action<bool> GameSetPaused;
        public Action<GamePhase> GamePhaseChanged;
        
        private GamePhase currentGamePhase = GamePhase.None;
        
        private static GameModel instance;
        private bool paused = false;
        private bool canPause = true;
        private bool won = false;
        
        private int totalDamageTaken = 0;
        private int totalDamageDealt = 0;
        private int totalEnemiesDestroyed = 0;
        
        private float timeLeft;
        private float timeElapsed;
        
        public void Initialize(float levelDuration)
        {
            timeLeft = levelDuration;
            timeElapsed = 0.0f;
            instance = this;

            if (FocusListener.Instance != null)
            {
                FocusListener.Instance.FocusChanged += OnFocusChanged;
            }
        }

        public void Dispose()
        {
            if (FocusListener.Instance != null)
            {
                FocusListener.Instance.FocusChanged -= OnFocusChanged;
            }
        }

        public void TogglePause()
        {
            AudioService.Instance.PlayEnemyHit();
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

        public void OnDealtDamage(int value)
        {
            totalDamageDealt += value;
        }
        
        public void OnTookDamage(int value)
        {
            totalDamageTaken += value;
        }
        
        public void OnDestroyedEnemy(int value)
        {
            totalEnemiesDestroyed += value;
        }

        public static void ElapseTime(float delta)
        {
            instance.timeElapsed += delta;
            instance.timeLeft -= delta;
        }

        private void OnFocusChanged(object sender, bool focus)
        {
            if (!focus) SetPaused(true);
        }
    }
}
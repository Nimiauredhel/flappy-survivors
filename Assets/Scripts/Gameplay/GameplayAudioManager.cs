using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Gameplay
{
    public class GameplayAudioManager : MonoBehaviour
    {
        [SerializeField] private EventReference gameplayMusicReference;

        private EventInstance gameplayMusicInstance;

        public void Initialize()
        {
            gameplayMusicInstance = RuntimeManager.CreateInstance(gameplayMusicReference);
            gameplayMusicInstance.start();
        }

        public void HandlePhaseChange(GamePhase newPhase)
        {
            gameplayMusicInstance.setParameterByName("GamePhase", (float)newPhase);
        }

        private void OnDestroy()
        {
            gameplayMusicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            gameplayMusicInstance.release();
        }
    }
}
using FMOD.Studio;
using FMODUnity;
using Gameplay;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "Audio Service Asset", menuName = "Config/Audio Service Asset", order = 0)]
    public class AudioService : ScriptableObject
    {
        private const string GAME_PHASE = "GamePhase";
        
        [SerializeField] private EventReference gameplayMusicReference;
        [SerializeField] private EventReference mainMenuMusicReference;
        
        [SerializeField] private EventReference enemyHitReference;
        [SerializeField] private EventReference enemyDestroyedReference;
        [SerializeField] private EventReference pickupSpawnedReference;
        [SerializeField] private EventReference pickupCollectedReference;

        private EventInstance gameplayMusicInstance;
        private EventInstance mainMenuMusicInstance;

        public static AudioService Instance;

        public void Initialize()
        {
            Instance = this;
        }

        public void PlayGameplayMusic()
        {
            gameplayMusicInstance = StartNewEvent(gameplayMusicReference);
        }

        public void ReleaseGameplayMusic()
        {
            gameplayMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            gameplayMusicInstance.release();
        }

        public void PlayMainMenuMusic()
        {
            mainMenuMusicInstance = StartNewEvent(mainMenuMusicReference);
        }

        public void ReleaseMainMenuMusic()
        {
            mainMenuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mainMenuMusicInstance.release();
        }

        public void PlayEnemyHit()
        {
            RuntimeManager.PlayOneShot(enemyHitReference);
        }

        public void PlayEnemyDestroyed()
        {
            RuntimeManager.PlayOneShot(enemyDestroyedReference);
        }

        public void PlayPickupSpawned()
        {
            RuntimeManager.PlayOneShot(pickupSpawnedReference);
        }

        public void PlayPickupCollected()
        {
            RuntimeManager.PlayOneShot(pickupCollectedReference);
        }

        public void HandlePhaseChange(GamePhase newPhase)
        {
            gameplayMusicInstance.setParameterByName(GAME_PHASE, (int)newPhase, true);
        }

        private EventInstance StartNewEvent(EventReference reference)
        {
            EventInstance newInstance = RuntimeManager.CreateInstance(reference);
            newInstance.start();
            return newInstance;
        }
    }
}

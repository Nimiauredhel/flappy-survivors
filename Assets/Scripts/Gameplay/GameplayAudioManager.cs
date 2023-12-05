using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Gameplay
{
    public class GameplayAudioManager : MonoBehaviour
    {
        [SerializeField] private EventReference gameplayMusicReference;
        [SerializeField] private EventReference enemyHitReference;
        [SerializeField] private EventReference enemyDestroyedReference;
        [SerializeField] private EventReference pickupSpawnedReference;
        [SerializeField] private EventReference pickupCollectedReference;

        private EventInstance gameplayMusicInstance;

        public static GameplayAudioManager Instance;

        public void Initialize()
        {
            Instance = this;
            gameplayMusicInstance = StartNewEvent(gameplayMusicReference);
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
            gameplayMusicInstance.setParameterByName("GamePhase", (float)newPhase);
        }

        private EventInstance StartNewEvent(EventReference reference)
        {
            EventInstance newInstance = RuntimeManager.CreateInstance(reference);
            newInstance.start();
            return newInstance;
        }

        private void OnDestroy()
        {
            gameplayMusicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            gameplayMusicInstance.release();
        }
    }
}
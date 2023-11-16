using System;
using Gameplay.Player;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;
        [Inject] private readonly VFXService vfxService;

        public void Start()
        {
            Application.targetFrameRate = 60;
            
            enemiesController.Initialize();
            enemiesController.EnemyKilled += EnemyKilledHandler;
            
            vfxService.Initialize();
            pickupsController.Initialize();
        }

        public void Tick()
        {
            enemiesController.DoUpdate();
            pickupsController.DoUpdate();
        }

        public void FixedTick()
        {
            enemiesController.DoFixedUpdate();
            pickupsController.DoFixedUpdate();
        }

        public void Dispose()
        {
            enemiesController.EnemyKilled -= EnemyKilledHandler;
        }

        private void EnemyKilledHandler(int value, Vector3 position)
        {
            vfxService.RequestExplosionAt(position);
            playerController.HandleEnemyKilled();
            
            int xpValue = Random.Range(0, value);
            
            if (xpValue > 0)
            {
                pickupsController.SpawnPickup(position, xpValue, PickupType.XP);
            }
        }
    }
}

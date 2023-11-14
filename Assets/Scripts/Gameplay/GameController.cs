using System;
using Gameplay.Player;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;

        public void Start()
        {
            enemiesController.Initialize();
            enemiesController.EnemyKilled += EnemyKilledHandler;
            
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
            pickupsController.SpawnPickup(position, value, PickupType.XP);
        }

        private void XPGainedHandler(int value)
        {
            playerController.ChangePlayerXP(value);
        }
    }
}

using System;
using System.Collections.Generic;
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

        private Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(32);
        
        public void Start()
        {
            Application.targetFrameRate = 60;

            playerController.ComboBreak += ComboBrokenHandler;
            
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

            if (value != 0)
            {
                PickupType type;

                if (value < 0)
                {
                    type = PickupType.Health;
                    value *= -1;
                }
                else
                {
                    type = PickupType.XP;
                }

                int pickupValue = Random.Range(Mathf.CeilToInt(value * 0.55f), value);
                comboBalloon.Push(new PickupDropOrder(pickupValue, type, position));
            }
        }

        private void ComboBrokenHandler(int brokenCombo)
        {
            Stack<PickupDropOrder> pickupsToDrop = new Stack<PickupDropOrder>(comboBalloon);
            comboBalloon.Clear();

            float comboModifier = Constants.Map(0, 100, 1.0f, 2.0f, brokenCombo);
            
            pickupsController.SpawnPickups(pickupsToDrop, comboModifier);
        }
    }
}

using System;
using Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly EnemiesController _enemiesController;
        [Inject] private readonly PlayerController _playerController;

        public void Start()
        {
            _enemiesController.Initialize();
            _enemiesController.EnemyKilled += EnemyKilledHandler;
        }

        public void Tick()
        {
            _enemiesController.DoUpdate();
        }

        public void FixedTick()
        {
            _enemiesController.DoFixedUpdate();
        }

        public void Dispose()
        {
            _enemiesController.EnemyKilled -= EnemyKilledHandler;
        }

        private void EnemyKilledHandler(object sender, int value)
        {
            _playerController.ChangePlayerXP(value);
        }
    }
}

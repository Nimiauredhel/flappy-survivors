using System;
using Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly ObjectMover _objectMover;
        [Inject] private readonly PlayerController _playerController;

        public void Start()
        {
            _objectMover.Initialize();
            _objectMover.EnemyKilled += EnemyKilledHandler;
        }

        public void Tick()
        {
            _objectMover.DoUpdate();
        }

        public void FixedTick()
        {
            _objectMover.DoFixedUpdate();
        }

        public void Dispose()
        {
            _objectMover.EnemyKilled -= EnemyKilledHandler;
        }

        private void EnemyKilledHandler(object sender, int value)
        {
            _playerController.GetXP(value);
        }
    }
}

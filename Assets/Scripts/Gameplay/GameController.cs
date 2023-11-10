using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable
    {
        [Inject] readonly ObjectMover _objectMover;

        public void Start()
        {
            _objectMover.Initialize();
        }

        public void Tick()
        {
            _objectMover.DoUpdate();
        }

        public void FixedTick()
        {
            _objectMover.DoFixedUpdate();
        }
    }
}

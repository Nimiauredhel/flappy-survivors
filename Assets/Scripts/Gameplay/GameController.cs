using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] readonly TouchReceiver _touchReceiver;
        [Inject] readonly PlayerCharacter _playerCharacter;
        [Inject] readonly ObjectMover _objectMover;

        public void Start()
        {
            _touchReceiver.PointerDown += OnPointerDown;
            _touchReceiver.PointerUp += OnPointerUp;
            _objectMover.Initialize();
        }

        public void Tick()
        {
            _playerCharacter.DoUpdate();
            _objectMover.DoUpdate();
        }

        public void FixedTick()
        {
            _playerCharacter.DoFixedUpdate();
            _objectMover.DoFixedUpdate();
        }

        public void Dispose()
        {
            _touchReceiver.PointerDown -= OnPointerDown;
            _touchReceiver.PointerUp -= OnPointerUp;
        }

        private void OnPointerDown()
        {
            _playerCharacter.ClimbCommand();
        }

        private void OnPointerUp()
        {
            _playerCharacter.DiveCommand();
        }
    }
}

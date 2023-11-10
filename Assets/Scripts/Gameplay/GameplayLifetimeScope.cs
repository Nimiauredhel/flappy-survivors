using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private TouchReceiver _touchReceiver;
        [SerializeField] private PlayerCharacter _playerCharacter;
        [SerializeField] private ObjectMover _objectMover;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_touchReceiver);
            builder.RegisterComponent(_playerCharacter);
            builder.RegisterComponent(_objectMover);
            builder.RegisterEntryPoint<GameController>();
            
        }
    }
}

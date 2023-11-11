using Gameplay.Data;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private TouchReceiver _touchReceiver;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private PlayerUIView _playerUIView;
        [SerializeField] private ObjectMover _objectMover;
        [SerializeField] private PlayerWeaponsComponent _playerWeapons;
        [SerializeField] private PlayerMovementData _playerMovementData;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_touchReceiver);
            builder.RegisterComponent(_playerView);
            builder.RegisterComponent(_playerUIView);
            builder.RegisterComponent(_objectMover);
            builder.RegisterComponent(_playerWeapons);
            builder.RegisterComponent(_playerMovementData);
            
            builder.Register<PlayerModel>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<PlayerController>().AsSelf();
            builder.RegisterEntryPoint<GameController>();
        }
    }
}

using Gameplay.Configuration;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private TouchReceiver touchReceiver;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private PlayerUIView playerUIView;
        [FormerlySerializedAs("objectMover")] [SerializeField] private EnemiesController enemiesController;
        [SerializeField] private PlayerWeaponsComponent playerWeapons;
        [SerializeField] private PlayerCharacterConfiguration characterConfig;
        [SerializeField] private PlayerMovementConfiguration playerMovementConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(touchReceiver);
            builder.RegisterComponent(playerView);
            builder.RegisterComponent(playerUIView);
            builder.RegisterComponent(enemiesController);
            builder.RegisterComponent(playerWeapons);
            builder.RegisterComponent(characterConfig);
            builder.RegisterComponent(playerMovementConfig);
            
            builder.Register<PlayerModel>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<PlayerController>().AsSelf();
            builder.RegisterEntryPoint<GameController>();
        }
    }
}

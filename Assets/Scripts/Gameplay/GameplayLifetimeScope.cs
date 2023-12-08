using Audio;
using CommandTerminal;
using Configuration;
using Gameplay.Level;
using Gameplay.Player;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private TouchReceiver touchReceiver;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private GameplayUIView gameplayUIView;
        [SerializeField] private PlayerUIView playerUIView;

        [SerializeField] private BurstSignalReceiver burstSignalReceiver;
        [SerializeField] private PlayableDirector levelDirector;
        
        [SerializeField] private PlayerMovementConfiguration playerMovementConfig;
        [SerializeField] private EnemyControllerConfig enemyControllerConfig;
        [SerializeField] private PickupControllerConfig pickupControllerConfig;
        [SerializeField] private VFXConfiguration vfxConfig;
        
        [SerializeField] private AudioService audioServiceInstance;
        [SerializeField] private Transform scrolledObjectParent;
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            audioServiceInstance.Initialize();
            
            builder.RegisterComponent(touchReceiver);
            builder.RegisterComponent(playerView);
            builder.RegisterComponent(gameplayUIView);
            builder.RegisterComponent(playerUIView);
            
            builder.RegisterComponent(burstSignalReceiver);
            builder.RegisterComponent(levelDirector);
            
            builder.RegisterComponent(playerMovementConfig);
            builder.RegisterComponent(enemyControllerConfig);
            builder.RegisterComponent(pickupControllerConfig);
            builder.RegisterComponent(vfxConfig);

            builder.RegisterComponent(scrolledObjectParent);

            builder.Register<VFXService>(Lifetime.Singleton);
            builder.Register<GameModel>(Lifetime.Singleton);
            builder.Register<PlayerController>(Lifetime.Singleton);
            builder.Register<PlayerModel>(Lifetime.Singleton);
            builder.Register<EnemiesController>(Lifetime.Singleton);
            builder.Register<PickupsController>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameController>();
        }
    }
}

using Configuration;
using Gameplay.Player;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Playables;
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
        [SerializeField] private UpgradesUIView upgradesUIView;
        
        [SerializeField] private EnemiesController enemiesController;
        [SerializeField] private PickupsController pickupsController;
        [SerializeField] private PlayerWeaponsComponent playerWeapons;
        [SerializeField] private VFXService vfxService;
        [SerializeField] private PlayableDirector levelDirector;
        
        [SerializeField] private PlayerMovementConfiguration playerMovementConfig;
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(touchReceiver);
            builder.RegisterComponent(playerView);
            builder.RegisterComponent(playerUIView);
            builder.RegisterComponent(upgradesUIView);
            
            builder.RegisterComponent(enemiesController);
            builder.RegisterComponent(pickupsController);
            builder.RegisterComponent(playerWeapons);
            builder.RegisterComponent(vfxService);
            builder.RegisterComponent(levelDirector);
            
            builder.RegisterComponent(playerMovementConfig);

            builder.Register<GameModel>(Lifetime.Singleton);
            builder.Register<PlayerController>(Lifetime.Singleton);
            builder.Register<PlayerModel>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameController>();
        }
    }
}

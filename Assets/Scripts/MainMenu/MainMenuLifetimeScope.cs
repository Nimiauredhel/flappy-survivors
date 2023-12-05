using Audio;
using Configuration;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace MainMenu
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MenuUIView menuUIView;
        [SerializeField] private UpgradeTreeConfiguration upgradeTreeConfig;
        [SerializeField] private LevelRegistry levelRegistry;
        [SerializeField] private PlayerCharacterConfiguration defaultCharacterConfig;
        [SerializeField] private AudioService audioServiceInstance;
        
        protected override void Configure(IContainerBuilder builder)
        {
            audioServiceInstance.Initialize();
            
            builder.RegisterComponent(menuUIView);
            builder.RegisterComponent(upgradeTreeConfig);
            builder.RegisterComponent(levelRegistry);
            builder.RegisterComponent(defaultCharacterConfig);
            builder.RegisterEntryPoint<MenuController>();
        }
    }
}

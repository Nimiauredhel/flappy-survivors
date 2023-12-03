using Configuration;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace MainMenu
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [FormerlySerializedAs("menuLoadoutsUIView")] [SerializeField] private MenuUIView menuUIView;
        [SerializeField] private UpgradeTreeConfiguration upgradeTreeConfig;
        [SerializeField] private LevelRegistry levelRegistry;
        [SerializeField] private PlayerCharacterConfiguration defaultCharacterConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(menuUIView);
            builder.RegisterComponent(upgradeTreeConfig);
            builder.RegisterComponent(levelRegistry);
            builder.RegisterComponent(defaultCharacterConfig);
            builder.RegisterEntryPoint<MenuController>();
        }
    }
}

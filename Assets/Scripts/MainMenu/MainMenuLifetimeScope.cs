using Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MainMenu
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MenuUIView menuUIView;
        [SerializeField] private UpgradeTreeConfiguration upgradeTreeConfig;
        [SerializeField] private PlayerCharacterConfiguration defaultCharacterConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(menuUIView);
            builder.RegisterComponent(upgradeTreeConfig);
            builder.RegisterComponent(defaultCharacterConfig);
            builder.RegisterEntryPoint<MenuController>();
        }
    }
}

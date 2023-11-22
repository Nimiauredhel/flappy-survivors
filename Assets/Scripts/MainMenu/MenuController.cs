using System.Collections;
using System.Collections.Generic;
using Configuration;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace MainMenu
{
    public class MenuController : IStartable
    {
        [Inject] private readonly MenuUIView view;
        [Inject] private readonly UpgradeTreeConfiguration upgradeTreeConfig;
        [Inject] private readonly PlayerCharacterConfiguration defaultPlayerConfig;

        private UpgradeTree currentUpgradeTree;
        
        public void Start()
        {
            currentUpgradeTree = upgradeTreeConfig.GetFreshUpgradeTree();
            
            List<UpgradeOption> allStartingOptions = currentUpgradeTree.GetAllCurrentOptions(1);
            
            view.DisplayLoadoutsDialog(allStartingOptions, LoadoutSelectedHandler);
        }

        private void LoadoutSelectedHandler(UpgradeOption selectedUpgrade)
        {
            selectedUpgrade.Taken = true;
            WeaponConfiguration[] startingWeapons = new WeaponConfiguration[1]{(WeaponConfiguration)selectedUpgrade.UpgradeConfig};
            PlayerCharacterConfiguration newPlayerConfig =
                ScriptableObject.CreateInstance<PlayerCharacterConfiguration>();
            newPlayerConfig.Initialize(defaultPlayerConfig.GetStats, startingWeapons, currentUpgradeTree);
            ConfigSelectionMediator.SetCharacterLoadout(newPlayerConfig, currentUpgradeTree);
            
            view.StartCoroutine(GameLoadingRoutine());
        }

        private IEnumerator GameLoadingRoutine()
        {
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync("Gameplay");

            while (loadingOperation.progress < 1.0f)
            {
                view.SetLoadingBar(loadingOperation.progress);
                yield return null;
            }
        }
    }
}
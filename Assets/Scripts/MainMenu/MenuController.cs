using System.Collections;
using System.Collections.Generic;
using Audio;
using Configuration;
using FMOD.Studio;
using FMODUnity;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace MainMenu
{
    public class MenuController : IStartable
    {
        [Inject] private readonly MenuUIView view;
        [Inject] private readonly UpgradeTreeConfiguration upgradeTreeConfig;
        [Inject] private readonly LevelRegistry levelRegistry;
        [Inject] private readonly PlayerCharacterConfiguration defaultPlayerConfig;

        private UpgradeTree currentUpgradeTree;
        private EventInstance mainMenuMusicInstance;
        
        public void Start()
        {
            InitLevelOptions();
            InitLoadoutOptions();
            
            AudioService.Instance.PlayMainMenuMusic();
        }

        private void InitLevelOptions()
        {
            view.DisplayLevelsDialog(levelRegistry.Levels, LevelSelectedHandler);
        }

        private void InitLoadoutOptions()
        {
            currentUpgradeTree = upgradeTreeConfig.GetFreshUpgradeTree();
            List<UpgradeOption> allStartingOptions = currentUpgradeTree.GetAllCurrentOptions(1);
            view.DisplayLoadoutsDialog(allStartingOptions, LoadoutSelectedHandler);
        }

        private void LevelSelectedHandler(LevelConfiguration selectedLevel)
        {
            ConfigSelectionMediator.SetLevel(selectedLevel);
        }

        private void LoadoutSelectedHandler(UpgradeOption selectedUpgrade)
        {
            AudioService.Instance.ReleaseMainMenuMusic();
            
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
using System.Collections;
using System.Collections.Generic;
using Audio;
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
        [Inject] private readonly LevelRegistry levelRegistry;
        [Inject] private readonly PlayerCharacterConfiguration defaultPlayerConfig;

        private UpgradeTree currentUpgradeTree;
        
        public void Start()
        {
            view.SetFadeAlpha(1.0f, 0.0f);
            view.SetCanvasAlpha(0.0f, 0.0f);
            
            InitLevelOptions();
            InitLoadoutOptions();
            
            AudioService.Instance.PlayMainMenuMusic();
            
            view.SetFadeAlpha(0.0f, 2.0f);
            view.SetCanvasAlpha(1.0f, 1.0f);
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
            view.SetCanvasAlpha(0.0f, 0.5f);
            view.SetFadeAlpha(1.0f, 1.0f);
            
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
            float elapsedTime = 0.0f;
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync("Gameplay");
            loadingOperation.allowSceneActivation = false;
            
            while (loadingOperation.progress < 0.9f || elapsedTime < 1.0f)
            {
                elapsedTime += Time.deltaTime;
                view.SetLoadingBar(loadingOperation.progress);
                yield return null;
            }

            loadingOperation.allowSceneActivation = true;
            AudioService.Instance.ReleaseMainMenuMusic();
        }
    }
}
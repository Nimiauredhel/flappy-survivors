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
        
        public async void Start()
        {
            AudioService.Instance.PlayMainMenuMusic();
            
            InitLevelOptions();
            InitLoadoutOptions();
            view.SetupPlayButton(PlayClickedHandler);

            await Awaitable.WaitForSecondsAsync(0.25f);
            view.SetFadeAlpha(0.0f, 5.0f);
            await Awaitable.WaitForSecondsAsync(1.0f);
            view.SetCanvasAlpha(0, 1.0f, 0.5f);
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
            currentUpgradeTree.ResetUpgradeTree();
            selectedUpgrade.Taken = true;
            WeaponConfiguration[] startingWeapons = new WeaponConfiguration[1]{(WeaponConfiguration)selectedUpgrade.UpgradeConfig};
            PlayerCharacterConfiguration newPlayerConfig =
                ScriptableObject.CreateInstance<PlayerCharacterConfiguration>();
            newPlayerConfig.Initialize(defaultPlayerConfig.GetStats, startingWeapons, currentUpgradeTree);
            ConfigSelectionMediator.SetCharacterLoadout(newPlayerConfig, currentUpgradeTree);
        }

        private void PlayClickedHandler()
        {
            _ = GameLoadingRoutine();
        }

        private async Awaitable GameLoadingRoutine()
        {
            float elapsedTime = 0.0f;
            float fakeProgress = 0.0f;
            float visualProgress = 0.0f;
            float minLoadTime = 0.5f;
            
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync("Gameplay");
            loadingOperation.allowSceneActivation = false;
            
            while (loadingOperation.progress < 0.9f || fakeProgress < 1.0f)
            {
                elapsedTime += Time.deltaTime;
                fakeProgress = Mathf.InverseLerp(0.0f, minLoadTime, elapsedTime);
                visualProgress = (fakeProgress + loadingOperation.progress) * 0.5f;
                view.SetCanvasAlpha(2, 1.0f - visualProgress, 0.0f);
                view.SetFadeAlpha(visualProgress, 0.0f);
                await Awaitable.NextFrameAsync();
            }
            
            view.SetCanvasAlpha(2, 0.0f, 0.2f);
            view.SetFadeAlpha(1.0f, 0.2f);
            await Awaitable.WaitForSecondsAsync(0.25f);
            
            loadingOperation.allowSceneActivation = true;
            AudioService.Instance.ReleaseMainMenuMusic();
        }
    }
}
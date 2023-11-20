using System;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Player;
using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using Gameplay.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly GameModel gameModel;
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;
        [Inject] private readonly VFXService vfxService;
        [Inject] private UpgradeTree upgradeTree;
        
        private Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(32);

        private AudioSource musicSource;
        private Camera gameplayCamera;
        
        public void Start()
        {
            Application.targetFrameRate = 60;
            gameplayCamera = Camera.main;
            
            gameModel.Initialize();
            
            playerController.ComboBreak += ComboBrokenHandler;
            playerController.LevelUp += LevelUpHandler;
            playerController.PlayerDamaged += PlayerDamagedHandler;
            
            enemiesController.Initialize();
            enemiesController.EnemyKilled += EnemyKilledHandler;
            
            playerController.Initialize();
            vfxService.Initialize();
            pickupsController.Initialize();
            
            SetMusic(true);
            
            LevelUpHandler(1);
        }

        public void Tick()
        {
            playerController.DoUpdate();
            enemiesController.DoUpdate();
            pickupsController.DoUpdate();
        }

        public void FixedTick()
        {
            playerController.DoFixedUpdate();
            enemiesController.DoFixedUpdate();
            pickupsController.DoFixedUpdate();
        }

        public void Dispose()
        {
            playerController.OnDispose();
            playerController.ComboBreak -= ComboBrokenHandler;
            playerController.LevelUp -= LevelUpHandler;
            
            enemiesController.EnemyKilled -= EnemyKilledHandler;
        }

        private void EnemyKilledHandler(int value, Vector3 position)
        {
            vfxService.RequestExplosionAt(position);
            playerController.HandleEnemyKilled();

            if (value != 0)
            {
                PickupType type;

                if (value < 0)
                {
                    type = PickupType.Health;
                    value *= -1;
                }
                else
                {
                    type = PickupType.XP;
                }

                int pickupValue = Random.Range(Mathf.CeilToInt(value * 0.55f), value);
                comboBalloon.Push(new PickupDropOrder(pickupValue, type, position));
            }
        }
        
        private void PlayerDamagedHandler(int damage)
        {
            Vector3 strength = new Vector3(1, 1, 0) * (damage / 10.0f);
            gameplayCamera.DOShakePosition(0.25f, strength);
        }

        private void ComboBrokenHandler(int brokenCombo)
        {
            Stack<PickupDropOrder> pickupsToDrop = new Stack<PickupDropOrder>(comboBalloon);
            comboBalloon.Clear();

            float comboModifier = Constants.Map(0, 100, 1.0f, 2.0f, brokenCombo);
            
            pickupsController.SpawnPickups(pickupsToDrop, comboModifier);
        }

        private void LevelUpHandler(int newLevel)
        {
            Vector3[] positions = new[] { new Vector3(25, 5), new Vector3(25, -1), new Vector3(25, -7) };
            List<UpgradeOption> allCurrentOptions = upgradeTree.GetAllCurrentOptions(newLevel);
            Stack<PickupDropOrder> shortList = new Stack<PickupDropOrder>(4);
            
            for (int i = 0; i < 3; i++)
            {
                if (allCurrentOptions.Count <= 0) break;
                
                UpgradeOption option = allCurrentOptions[Random.Range(0, allCurrentOptions.Count)];
                shortList.Push(new PickupDropOrder(option, PickupType.Upgrade, positions[i]));
                allCurrentOptions.Remove(option);
            }

            if (shortList.Count > 0)
            {
                gameModel.SetGamePhase(GamePhase.UpgradePhase);
                SetMusic(false);
            
                pickupsController.PurgeAllPickups(true);
                
                ScrolledObjectView[] upgradePickups = pickupsController.SpawnAndReturnPickups(shortList, 0.0f);

                Action finishUpgradeAction = delegate { FinishUpgradePhase(upgradePickups); };
            
                for (int i = 0; i < upgradePickups.Length; i++)
                {
                    upgradePickups[i].Deactivated += finishUpgradeAction;
                }
            }
        }

        private void FinishUpgradePhase(ScrolledObjectView[] upgradePickups)
        {
            if (GameModel.CurrentGamePhase != GamePhase.UpgradePhase) return;

            gameModel.SetGamePhase(GamePhase.HordePhase);
            SetMusic(true);

            for (int i = 0; i < upgradePickups.Length; i++)
            {
                if (upgradePickups[i].Active)
                {
                    upgradePickups[i].Deactivate();
                }
            }
        }

        private void SetMusic(bool fighting)
        {
            if (musicSource == null)
            {
                GameObject go = new GameObject();
                musicSource = go.AddComponent<AudioSource>();
                go.name = "MusicSource";
            }

            musicSource.Stop();
            musicSource.loop = true;
            musicSource.clip = fighting
                ? Resources.Load<AudioClip>("Music/Fighting")
                : Resources.Load<AudioClip>("Music/Upgrading");
            musicSource.Play();
        }
    }
}

using System;
using System.Collections.Generic;
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
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;
        [Inject] private readonly VFXService vfxService;
        [Inject] private UpgradeTree upgradeTree;

        private bool allowEnemiesToSpawn = false;
        private Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(32);

        private AudioSource musicSource;
        
        public void Start()
        {
            Application.targetFrameRate = 60;

            playerController.ComboBreak += ComboBrokenHandler;
            playerController.LevelUp += LevelUpHandler;
            
            enemiesController.Initialize();
            enemiesController.EnemyKilled += EnemyKilledHandler;
            
            playerController.Initialize();
            vfxService.Initialize();
            pickupsController.Initialize();
            
            allowEnemiesToSpawn = true;
            SetMusic(true);
            
            LevelUpHandler(1);
        }

        public void Tick()
        {
            playerController.DoUpdate();
            enemiesController.DoUpdate(allowEnemiesToSpawn);
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

        private void ComboBrokenHandler(int brokenCombo)
        {
            Stack<PickupDropOrder> pickupsToDrop = new Stack<PickupDropOrder>(comboBalloon);
            comboBalloon.Clear();

            float comboModifier = Constants.Map(0, 100, 1.0f, 2.0f, brokenCombo);
            
            pickupsController.SpawnPickups(pickupsToDrop, comboModifier);
        }

        private void LevelUpHandler(int newLevel)
        {
            allowEnemiesToSpawn = false;
            SetMusic(false);
            
            Vector3[] positions = new[] { new Vector3(25, 5), new Vector3(25, 0), new Vector3(25, -5) };
            List<UpgradeOption> allCurrentOptions = upgradeTree.GetAllCurrentOptions(newLevel);
            Stack<PickupDropOrder> shortList = new Stack<PickupDropOrder>(4);
            
            for (int i = 0; i < 3; i++)
            {
                if (allCurrentOptions.Count <= 0) break;
                
                UpgradeOption option = allCurrentOptions[Random.Range(0, allCurrentOptions.Count)];
                shortList.Push(new PickupDropOrder(option, PickupType.Upgrade, positions[i]));
                allCurrentOptions.Remove(option);
            }
            
            ScrolledObjectView[] upgradePickups = pickupsController.SpawnAndReturnPickups(shortList, 0.0f);

            
            
            for (int i = 0; i < upgradePickups.Length; i++)
            {
                upgradePickups[i].Deactivated += delegate { FinishUpgradePhase(upgradePickups); };
            }
        }

        private void FinishUpgradePhase(ScrolledObjectView[] upgradePickups)
        {
            if (allowEnemiesToSpawn) return;

            allowEnemiesToSpawn = true;
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

﻿using System;
using Configuration;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeOption
    {
        [HideInInspector]
        public bool Taken = false;
        public WeaponConfiguration UpgradeConfig => upgradeConfig;
        
        [SerializeField] private WeaponConfiguration upgradeConfig;
    }
}
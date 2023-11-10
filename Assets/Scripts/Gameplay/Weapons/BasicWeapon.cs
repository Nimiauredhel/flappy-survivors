using System;
using System.Threading;
using System.Threading.Tasks;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class BasicWeapon : WeaponSandbox
    {
        [SerializeField] private Collider2D hitBox;
        private WeaponData _weaponData;
        private CancellationTokenSource attackCancellationTokenSource = new CancellationTokenSource();
        
        public override void Draw(WeaponData weaponData)
        {
            _weaponData = weaponData;
            
            AttackAsync();
        }

        public override void Sheathe(WeaponData weaponData)
        {
            hitBox.gameObject.SetActive(false);
        }

        private async Task AttackAsync()
        {
            hitBox.gameObject.SetActive(true);
            await Task.Delay(TimeSpan.FromSeconds(_weaponData.Duration));
            hitBox.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ScrolledObject SO = other.gameObject.GetComponentInParent<ScrolledObject>();

            if (SO != null && SO.Active)
            {
                Debug.Log("Hit SO!");
                SO.TakeDamage(_weaponData.BaseDamage);
            }
        }
    }
}

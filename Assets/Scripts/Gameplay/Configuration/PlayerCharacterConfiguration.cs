using UnityEngine;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "Config/Character Config", order = 0)]
    public class PlayerCharacterConfiguration : ScriptableObject
    {
        public WeaponConfiguration[] Weapons => weapons;
        
        [SerializeField] private WeaponConfiguration[] weapons;
    }
}

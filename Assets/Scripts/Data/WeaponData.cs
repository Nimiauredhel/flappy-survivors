using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Data/Weapon Data", order = 0)]
public class WeaponData : ScriptableObject
{

    public enum WeaponType
    {
        None,
        Climbing,
        Diving,
        Both
    }

    public WeaponType Type => weaponType;
    public float Cooldown => cooldown;
    public float Duration => duration;
    public float BaseDamage => baseDamage;
    public WeaponData NextLevel => nextLevel;

    public void WeaponUpdate()
    {
        logic.Draw(this);
    }

    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float cooldown;
    [SerializeField] private float duration;
    [Space] 
    [SerializeField] private float baseDamage;
    [Space]
    [SerializeField] private WeaponSandbox logic;
    [SerializeField] private WeaponData nextLevel;
}

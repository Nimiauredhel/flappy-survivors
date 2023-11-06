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
    public WeaponSandbox Logic => logic;
    public WeaponData NextLevel => nextLevel;

    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float cooldown;
    [SerializeField] private float duration;
    [SerializeField] private WeaponSandbox logic;
    [SerializeField] private WeaponData nextLevel;
}

using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Characters
{
    public class PawnWeaponModule : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private WeaponData attackWeapon;
        [SerializeField] private WeaponData kickWeapon;
        [SerializeField] private WeaponData aerialWeapon;
        [SerializeField] private WeaponData[] equippedWeapons;
        [SerializeField] private int startingWeaponIndex;

        private int _activeWeaponIndex;
        private ActorAnimationDriver _animationDriver;

        public WeaponData AttackWeapon => attackWeapon;
        public WeaponData KickWeapon => kickWeapon;
        public WeaponData AerialWeapon => aerialWeapon;
        public WeaponData ActiveWeapon => (equippedWeapons != null && equippedWeapons.Length > _activeWeaponIndex) ? equippedWeapons[_activeWeaponIndex] : null;

        private void Awake()
        {
            _animationDriver = GetComponent<ActorAnimationDriver>();
            if (equippedWeapons != null && equippedWeapons.Length > 0)
            {
                _activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, equippedWeapons.Length - 1);
            }
            ApplyActiveWeapon();
        }

        public void CycleWeapon(int direction)
        {
            if (equippedWeapons == null || equippedWeapons.Length <= 1)
                return;

            _activeWeaponIndex = (_activeWeaponIndex + direction + equippedWeapons.Length) % equippedWeapons.Length;
            ApplyActiveWeapon();
        }

        public void ApplyActiveWeapon()
        {
            WeaponData weapon = ActiveWeapon;
            _animationDriver?.SetRuntimeControllerOverride(weapon != null ? weapon.overrideController : null);
        }

        public void SetWeapons(WeaponData attack, WeaponData kick, WeaponData aerial)
        {
            attackWeapon = attack;
            kickWeapon = kick;
            aerialWeapon = aerial;
            ApplyActiveWeapon();
        }
    }
}
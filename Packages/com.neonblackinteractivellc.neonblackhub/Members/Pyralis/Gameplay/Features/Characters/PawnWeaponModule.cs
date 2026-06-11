using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Inventory,
        Relevance = "Pawn module for managing equipped weapon data and animation overrides.",
        AssignmentFields = new[] { nameof(attackWeapon), nameof(kickWeapon), nameof(aerialWeapon), nameof(equippedWeapons) },
        FirstProof = "Assign a weapon and verify the pawn's animator controller is overridden at runtime.",
        ExpertAdvice = "WeaponData assets can override the base animator controller. Ensure your weapon assets have the correct 'overrideController' assigned.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
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
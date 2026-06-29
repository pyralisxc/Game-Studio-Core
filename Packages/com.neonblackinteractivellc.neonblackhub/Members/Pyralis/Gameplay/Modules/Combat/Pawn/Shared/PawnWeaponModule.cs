using UnityEngine;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat, Inventory",
        CapabilityPath = "Combat/Actions/Pawn Weapon Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Pawn module for managing equipped weapon data and animation overrides.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        RequiredFields = new[] { nameof(attackWeapon), nameof(kickWeapon), nameof(aerialWeapon), nameof(equippedWeapons) },
        SetupSteps = new[] { "Attach to the pawn root that owns combat animation.", "Assign WeaponData assets for the attacks this pawn can perform." },
        SuccessChecks = new[] { "Assign a weapon and verify the pawn's animator controller is overridden at runtime." },
        Tags = new[] { "capability:Combat", "capability:Inventory" }
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
        private IActorAnimationController _animationDriver;

        public WeaponData AttackWeapon => attackWeapon;
        public WeaponData KickWeapon => kickWeapon;
        public WeaponData AerialWeapon => aerialWeapon;
        public WeaponData ActiveWeapon => (equippedWeapons != null && equippedWeapons.Length > _activeWeaponIndex) ? equippedWeapons[_activeWeaponIndex] : null;

        private void Awake()
        {
            _animationDriver = GetComponent<IActorAnimationController>();
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

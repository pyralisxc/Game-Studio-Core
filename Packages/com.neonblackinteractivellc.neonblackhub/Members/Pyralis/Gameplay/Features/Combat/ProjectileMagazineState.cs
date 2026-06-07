using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public sealed class ProjectileMagazineState
    {
        public int ClipSize { get; }
        public int AmmoPerShot { get; }
        public int CurrentClipAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        public bool IsUnlimited => ClipSize <= 0;
        public bool HasUnlimitedReserve { get; }
        public bool CanReload => !IsUnlimited && CurrentClipAmmo < ClipSize && (HasUnlimitedReserve || ReserveAmmo > 0);
        public bool CanFire => IsUnlimited || CurrentClipAmmo >= AmmoPerShot;

        public ProjectileMagazineState(FireModeDefinition fireMode, int reserveAmmo = -1)
        {
            ClipSize = fireMode != null ? Mathf.Max(0, fireMode.clipSize) : 0;
            AmmoPerShot = fireMode != null ? Mathf.Max(1, fireMode.ammoPerShot) : 1;
            CurrentClipAmmo = ClipSize;
            HasUnlimitedReserve = reserveAmmo < 0;
            ReserveAmmo = HasUnlimitedReserve ? 0 : Mathf.Max(0, reserveAmmo);
        }

        public bool TryConsumeShot()
        {
            if (IsUnlimited)
                return true;

            if (CurrentClipAmmo < AmmoPerShot)
                return false;

            CurrentClipAmmo -= AmmoPerShot;
            return true;
        }

        public bool TryReload()
        {
            if (!CanReload)
                return false;

            int needed = ClipSize - CurrentClipAmmo;
            int loaded = HasUnlimitedReserve ? needed : Mathf.Min(needed, ReserveAmmo);
            if (loaded <= 0)
                return false;

            CurrentClipAmmo += loaded;
            if (!HasUnlimitedReserve)
                ReserveAmmo -= loaded;

            return true;
        }

        public void SetCurrentClipAmmo(int amount)
        {
            CurrentClipAmmo = IsUnlimited ? 0 : Mathf.Clamp(amount, 0, ClipSize);
        }

        public void AddReserveAmmo(int amount)
        {
            if (!HasUnlimitedReserve && amount > 0)
                ReserveAmmo += amount;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Fire Mode Definition", fileName = "FireModeDefinition")]
    public class FireModeDefinition : ScriptableObject
    {
        public string fireModeId = "fire.single";
        public string displayName = "Single Shot";
        public bool automatic;
        public float cooldown = 0.2f;
        public int ammoPerShot = 1;
        public int clipSize = 0;
        public float reloadDuration = 0f;
        public int burstCount = 1;
        public float burstInterval = 0.05f;
        public int projectilesPerShot = 1;
        public float spreadAngle = 0f;

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            fireModeId = !string.IsNullOrWhiteSpace(fireModeId) ? fireModeId.Trim() : name;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : fireModeId;
            cooldown = Mathf.Max(0f, cooldown);
            ammoPerShot = Mathf.Max(0, ammoPerShot);
            clipSize = Mathf.Max(0, clipSize);
            reloadDuration = Mathf.Max(0f, reloadDuration);
            burstCount = Mathf.Max(1, burstCount);
            burstInterval = Mathf.Max(0f, burstInterval);
            projectilesPerShot = Mathf.Max(1, projectilesPerShot);
            spreadAngle = Mathf.Max(0f, spreadAngle);
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(fireModeId))
                issues.Add("Fire mode id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (clipSize > 0 && ammoPerShot <= 0)
                issues.Add("Ammo per shot must be greater than zero when clip size is used.");

            if (clipSize > 0 && reloadDuration <= 0f)
                issues.Add("Reload duration should be greater than zero when clip size is used.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}

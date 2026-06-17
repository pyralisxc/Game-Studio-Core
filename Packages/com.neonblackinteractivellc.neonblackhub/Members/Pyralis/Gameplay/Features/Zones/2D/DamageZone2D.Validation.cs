using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Zones
{
    public partial class DamageZone2D
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
                yield return "Collider2D is required for 2D trigger damage.";
            else if (!zoneCollider.isTrigger)
                yield return "Collider2D is not set to Is Trigger. Awake will force it on.";

            if (impactProfile == null && damagePerTick <= 0f)
                yield return "Fallback Damage Per Tick must be greater than zero when Impact Profile is empty.";

            if (tickInterval <= 0f)
                yield return "Tick Interval must be greater than zero.";
        }
    }
}

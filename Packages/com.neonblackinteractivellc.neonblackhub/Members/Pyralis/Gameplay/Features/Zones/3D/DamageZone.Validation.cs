using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Zones
{
    public partial class DamageZone
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                yield return "BoxCollider is required for 3D trigger damage.";
            else if (!box.isTrigger)
                yield return "BoxCollider is not set to Is Trigger. Awake will force it on.";

            if (impactProfile == null && damagePerTick <= 0f)
                yield return "Fallback Damage Per Tick must be greater than zero when Impact Profile is empty.";

            if (tickInterval <= 0f)
                yield return "Tick Interval must be greater than zero.";
        }
    }
}

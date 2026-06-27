using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    public partial class DamageZone
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                yield return PyralisRuntimeValidationIssue.Required("BoxCollider is required for 3D trigger damage.");
            else if (!box.isTrigger)
                yield return PyralisRuntimeValidationIssue.Required("BoxCollider is not set to Is Trigger. Awake will force it on.");

            if (impactProfile == null && damagePerTick <= 0f)
                yield return PyralisRuntimeValidationIssue.Required("Fallback Damage Per Tick must be greater than zero when Impact Profile is empty.");

            if (tickInterval <= 0f)
                yield return PyralisRuntimeValidationIssue.Required("Tick Interval must be greater than zero.");
        }
    }
}

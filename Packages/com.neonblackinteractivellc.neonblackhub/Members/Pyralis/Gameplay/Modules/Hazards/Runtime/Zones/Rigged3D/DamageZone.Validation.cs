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

            if (impactProfile == null)
            {
                yield return PyralisRuntimeValidationIssue.Required("Hazard Impact Profile is required. Damage zones use profile-owned impact payloads.");
                yield break;
            }

            foreach (PyralisRuntimeValidationIssue issue in impactProfile.GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    yield return new PyralisRuntimeValidationIssue(
                        $"Impact Profile: {issue.Message}",
                        "impactProfile",
                        nameof(DamageZone),
                        "Open the assigned HazardImpactProfile and resolve the named issue.",
                        "Assigned HazardImpactProfile reports no validation issues.",
                        issue.Severity,
                        "DamageZone.ImpactProfile." + issue.IssueCode);
            }
        }
    }
}

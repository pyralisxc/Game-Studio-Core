using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    public partial class DamageZone2D
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
                yield return RuntimeValidationIssue.Required("Collider2D is required for 2D trigger damage.");
            else if (!zoneCollider.isTrigger)
                yield return RuntimeValidationIssue.Required("Collider2D is not set to Is Trigger. Awake will force it on.");

            if (impactProfile == null)
            {
                yield return RuntimeValidationIssue.Required("Hazard Impact Profile is required. Damage zones use profile-owned impact payloads.");
                yield break;
            }

            foreach (RuntimeValidationIssue issue in impactProfile.GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    yield return new RuntimeValidationIssue(
                        $"Impact Profile: {issue.Message}",
                        "impactProfile",
                        nameof(DamageZone2D),
                        "Open the assigned HazardImpactProfile and resolve the named issue.",
                        "Assigned HazardImpactProfile reports no validation issues.",
                        issue.Severity,
                        "DamageZone2D.ImpactProfile." + issue.IssueCode);
            }
        }
    }
}

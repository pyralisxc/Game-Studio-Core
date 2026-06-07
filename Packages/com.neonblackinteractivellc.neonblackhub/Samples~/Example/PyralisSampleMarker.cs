using UnityEngine;

namespace NeonBlack.Gameplay.Samples
{
    /// <summary>
    /// Marks an imported Pyralis sample root so users can identify package sample content.
    /// </summary>
    public sealed class PyralisSampleMarker : MonoBehaviour
    {
        [SerializeField] private string setupGuidePath = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/START_HERE.md";

        public string SetupGuidePath => setupGuidePath;
    }
}

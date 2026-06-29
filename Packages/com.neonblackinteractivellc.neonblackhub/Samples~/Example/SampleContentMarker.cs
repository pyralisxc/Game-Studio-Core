using UnityEngine;

namespace NeonBlack.Gameplay.Samples
{
    /// <summary>
    /// Marks an imported gameplay sample root so users can identify package sample content.
    /// </summary>
    public sealed class SampleContentMarker : MonoBehaviour
    {
        [SerializeField] private string setupGuidePath = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/README.md";

        public string SetupGuidePath => setupGuidePath;
    }
}

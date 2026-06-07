using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
    /// <summary>
    /// ScriptableObject defining all parameters for a SpriteFlasher color effect.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFlashPreset", menuName = "NeonBlack/Visual/Flash Preset")]
    public class FlashPresetSO : ScriptableObject
    {
        public enum FlashEase
        {
            Linear,
            InSine,
            OutSine,
            InOutSine,
            InQuad,
            OutQuad,
            InOutQuad,
            InCubic,
            OutCubic
        }

        public enum FlashMode
        {
            Pulse,
            Strobe,
            Blink,
            ColorCycle,
        }

        [Header("Mode")]
        public FlashMode mode = FlashMode.Pulse;

        [Header("Colors")]
        public Color flashColor = Color.red;
        public bool useRendererColorAsBase = true;
        public Color baseColor = Color.white;
        public Color[] cycleColors = new Color[0];

        [Header("Timing")]
        [Min(0.01f)] public float flashDuration = 0.15f;
        [Min(0f)] public float interval = 0.1f;
        [Min(0f)] public float cycleDelay = 0f;
        public int loopCount = -1;

        [Header("Easing")]
        public FlashEase easeIn = FlashEase.InOutSine;
        public FlashEase easeOut = FlashEase.InOutSine;

        [Header("Alpha Override")]
        public bool overrideAlpha = false;
        [Range(0f, 1f)] public float flashAlpha = 1f;
        [Range(0f, 1f)] public float baseAlpha = 1f;
    }
}

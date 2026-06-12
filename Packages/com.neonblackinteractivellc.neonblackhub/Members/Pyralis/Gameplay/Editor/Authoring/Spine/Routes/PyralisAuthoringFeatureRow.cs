namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringFeatureRow
    {
        public PyralisAuthoringFeatureRow(
            string feature,
            string source,
            string gameplayEffect,
            string unitySetup,
            string customization)
        {
            Feature = feature;
            Source = source;
            GameplayEffect = gameplayEffect;
            UnitySetup = unitySetup;
            Customization = customization;
        }

        public string Feature { get; }
        public string Source { get; }
        public string GameplayEffect { get; }
        public string UnitySetup { get; }
        public string Customization { get; }
    }
}

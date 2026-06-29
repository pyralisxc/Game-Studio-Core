using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines camera presentation choices for shared or split participant views.
    /// </summary>
    [AuthoringContract(
        StableId = "camera.rig.profile",
        Category = "Camera",
        CapabilityPath = "World & Meta/Camera/Camera Rig Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Project-window creation path for gameplay camera focus and saved Cinemachine recipe values.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/camera",
        RequiredFields = new[] { nameof(presentationMode), nameof(focusMode), nameof(useCinemachine) },
        PrerequisiteStableIds = new[] { "mode.definition" },
        RouteStage = "Game Mode Asset",
        RouteOrder = 40,
        SetupDomain = "Camera",
        ProofTarget = "CameraRigProfile routes gameplay focus and applies saved Cinemachine recipe values.",
        NativeActionKind = AuthoringActionKind.CreateAsset,
        SuccessChecks = new[] { "Verify Cinemachine follows the profile's selected focus target and applies the saved recipe." },
        RoleTags = new[] { "CameraProfile", "ParticipantFollow", "PlayfieldView" },
        Tags = new[] { "capability:Camera", "runtime:CameraInput", "lane:Camera", "priority:AuxiliaryDefault" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Camera Rig Profile", fileName = "CameraRigProfile", order = -70)]
    public class CameraRigProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (useCinemachine && (reflectedSettings == null || reflectedSettings.Count == 0))
            {
                yield return RuntimeValidationIssue.Optional(
                    "No reflected Cinemachine recipe is saved yet. Use the CameraRigProfile inspector to sync from a scene Cinemachine rig.");
            }
        }

        public enum CameraPresentationMode
        {
            Shared,
            SplitScreen
        }

        public enum CameraFocusMode
        {
            ManualCinemachine,
            ParticipantPawns,
            ParticipantGroup,
            PlayfieldCenter,
            ExplicitSceneTarget
        }

        public IReadOnlyList<CameraProfileEntry> ReflectedSettings => reflectedSettings;

        public CameraPresentationMode presentationMode = CameraPresentationMode.Shared;
        [Tooltip("Chooses which gameplay target the gameplay camera route sends into Cinemachine. Manual Cinemachine leaves Follow/LookAt untouched.")]
        public CameraFocusMode focusMode = CameraFocusMode.ParticipantGroup;
        public bool useCinemachine = true;
        public bool lockToPlayfield = true;

        [SerializeField]
        private List<CameraProfileEntry> reflectedSettings = new List<CameraProfileEntry>();

        public void Sanitize()
        {
            reflectedSettings ??= new List<CameraProfileEntry>();
            for (int i = reflectedSettings.Count - 1; i >= 0; i--)
            {
                if (reflectedSettings[i] == null)
                    reflectedSettings.RemoveAt(i);
                else
                    reflectedSettings[i].Sanitize();
            }
        }

        public void ReplaceReflectedSettings(IEnumerable<CameraProfileEntry> entries)
        {
            reflectedSettings = entries != null
                ? new List<CameraProfileEntry>(entries)
                : new List<CameraProfileEntry>();
            Sanitize();
        }

        public void ClearMissingReflectedSettings()
        {
            reflectedSettings ??= new List<CameraProfileEntry>();
            reflectedSettings.RemoveAll(entry =>
                entry == null
                || entry.Status == CameraProfileMappingStatus.MissingComponent
                || entry.Status == CameraProfileMappingStatus.MissingProperty
                || entry.Status == CameraProfileMappingStatus.Unsupported
                || entry.Status == CameraProfileMappingStatus.Stale);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }

}

using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared presentation authoring profile for pawn visuals and camera-facing behavior.
    /// </summary>
    [AuthoringContract(
        Category = "Animation, V F X",
        CapabilityPath = "Presentation/Feedback/Pawn Presentation Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Project-window creation path for pawn presentation lane and visual setup choices.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/visuals",
        RequiredFields = new[] { nameof(presentationMode) },
        SuccessChecks = new[] { "Change the primary tint and see it reflected on the pawn in the scene." },
        Tags = new[] { "capability:Animation", "capability:VFX", "runtime:AnimationPresentation" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Presentation Profile", fileName = "PawnPresentationProfile", order = -40)]
    public class PawnPresentationProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            yield break;
        }

        public ActorPresentationMode presentationMode = ActorPresentationMode.Sprite2D;
        public GameObject hudPrefab;
        public BillboardFacingMode billboardFacingMode = BillboardFacingMode.YAxisOnly;
        public bool spriteDefaultFacesRight = true;
        public RiggedAnimationRigType rigType = RiggedAnimationRigType.Generic;
        public bool useSharedCamera = true;
        public Color primaryTint = Color.white;
        public ActorShadowMode shadowMode = ActorShadowMode.Auto;
        public Sprite shadowSprite;
        public GameObject shadowPrefab;
        public Vector3 shadowLocalOffset = new Vector3(0f, -0.2f, 0f);
        public Vector3 shadowScale = Vector3.one;
        public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
        public string shadowSortingLayerName = "Default";
        public int shadowSortingOrder = -1;
        public float shadowHeightScaleResponse = 0.15f;
        public bool castModelShadows = true;
        public bool receiveModelShadows = true;
    }
}

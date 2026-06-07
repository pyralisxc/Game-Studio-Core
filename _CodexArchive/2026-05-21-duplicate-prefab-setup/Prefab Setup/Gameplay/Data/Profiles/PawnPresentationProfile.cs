using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared presentation authoring profile for pawn visuals and camera-facing behavior.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Pawn Presentation Profile", fileName = "PawnPresentationProfile")]
    public class PawnPresentationProfile : ScriptableObject
    {
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

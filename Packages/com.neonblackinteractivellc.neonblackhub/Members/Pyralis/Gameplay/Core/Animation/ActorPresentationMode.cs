namespace NeonBlack.Gameplay.Presentation.Animation
{
    /// <summary>
    /// Defines the primary camera perspective and visual rendering style for a participant or actor.
    /// This controls framing logic, input-to-visual mapping, and primary rendering backend.
    /// </summary>
    public enum ActorPresentationMode
    {
        /// <summary> Side-scroller or flat top-down 2D rendering using SpriteRenderers. </summary>
        Sprite2D,
        
        /// <summary> Classic retro 2.5D; 2D sprites moving through 3D depth lanes or environments. </summary>
        Billboard2_5D,
        
        /// <summary> 3D rigged models viewed from a trailing, orbital, or isometric target camera. </summary>
        ThirdPerson3D,
        
        /// <summary> 3D rigged models with the camera pinned inside or near the character's eyes. </summary>
        FirstPerson3D,
        
        /// <summary> Overhead board layout, grid spaces, or card playing fields. </summary>
        TabletopBoard,
        
        /// <summary> Screen-space overlay or menu-only presentation with no active world camera. </summary>
        UiMenuOnly,

        /// <summary> Dynamically switches between multiple presentation modes or camera profiles. </summary>
        Mixed
    }

    public enum BillboardFacingMode
    {
        YAxisOnly,
        FullFacing
    }

    public enum RiggedAnimationRigType
    {
        Generic,
        Humanoid
    }
}

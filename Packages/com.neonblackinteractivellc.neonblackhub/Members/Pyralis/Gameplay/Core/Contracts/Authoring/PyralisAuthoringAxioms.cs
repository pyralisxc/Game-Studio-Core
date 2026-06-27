namespace NeonBlack.Gameplay.Core.Contracts
{
    [System.Flags]
    public enum AuthoringWorldAxiom
    {
        None = 0,
        
        // Dimensionality (Logic & Physics Constraints)
        Dimensions2D = 1 << 0,
        Dimensions3D = 1 << 1,
        
        // Physics / Gravity Mechanics
        GravityVertical = 1 << 2,
        GravityNone = 1 << 3,
        GravityRadial = 1 << 4,
        
        // Time / Sequence Logic
        Realtime = 1 << 5,
        TurnBased = 1 << 6,
        
        // Spatial Topology
        BoundedSpace = 1 << 7,
        WrappedSpace = 1 << 8,
        InfiniteSpace = 1 << 9,

        // Networking Mechanics
        Networked = 1 << 10
    }
}

using System;

namespace NeonBlack.Gameplay.Core.Contracts
{
    [System.Flags]
    public enum AuthoringWorldAxiom
{
        None = 0,
        
        // Dimensionality
        Dimensions2D = 1 << 0,
        Dimensions3D = 1 << 1,
        
        // Physics / Movement Constraints
        GravityVertical = 1 << 2,
        GravityNone = 1 << 3,
        GravityRadial = 1 << 4,
        
        // Time / Sequence
        Realtime = 1 << 5,
        TurnBased = 1 << 6,
        
        // Topology
        BoundedSpace = 1 << 7,
        WrappedSpace = 1 << 8,
        InfiniteSpace = 1 << 9,
        
        // Presentation Defaults (Axiomatic)
        SpriteVisuals = 1 << 10,
        MeshVisuals = 1 << 11,
        BillboardVisuals = 1 << 12
    }

    public static class AuthoringWorldAxiomRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<AuthoringWorldAxiom, AxiomMetadata> _metadata = new System.Collections.Generic.Dictionary<AuthoringWorldAxiom, AxiomMetadata>
        {
            { AuthoringWorldAxiom.Dimensions2D, new AxiomMetadata("2D", "Flat, 2D coordinate space (XY or XZ).") },
            { AuthoringWorldAxiom.Dimensions3D, new AxiomMetadata("3D", "Full 3D coordinate space (XYZ).") },
            { AuthoringWorldAxiom.GravityVertical, new AxiomMetadata("Vertical Gravity", "Standard downward gravity vector.") },
            { AuthoringWorldAxiom.GravityNone, new AxiomMetadata("Zero Gravity", "No inherent gravity vector; movement is unconstrained.") },
            { AuthoringWorldAxiom.GravityRadial, new AxiomMetadata("Radial Gravity", "Gravity pulls toward a specific point or center.") },
            { AuthoringWorldAxiom.Realtime, new AxiomMetadata("Real-time", "Continuous time flow; actions happen immediately.") },
            { AuthoringWorldAxiom.TurnBased, new AxiomMetadata("Turn-based", "Discrete time steps; actions are phased or queued.") },
            { AuthoringWorldAxiom.BoundedSpace, new AxiomMetadata("Bounded", "The world has explicit limits or walls.") },
            { AuthoringWorldAxiom.WrappedSpace, new AxiomMetadata("Wrapped", "Moving past one edge teleports to the opposite edge.") },
            { AuthoringWorldAxiom.InfiniteSpace, new AxiomMetadata("Infinite", "The world has no inherent bounds or limits.") },
            { AuthoringWorldAxiom.SpriteVisuals, new AxiomMetadata("Sprites", "Uses 2D sprites as the primary visual representation.") },
            { AuthoringWorldAxiom.MeshVisuals, new AxiomMetadata("Meshes", "Uses 3D meshes as the primary visual representation.") },
            { AuthoringWorldAxiom.BillboardVisuals, new AxiomMetadata("Billboards", "Uses 2D sprites that face the camera in 3D space.") }
        };

        public static string GetDisplayName(AuthoringWorldAxiom axiom)
        {
            if (axiom == AuthoringWorldAxiom.None) return "General";
            return _metadata.TryGetValue(axiom, out var meta) ? meta.DisplayName : axiom.ToString();
        }

        public static string GetTooltip(AuthoringWorldAxiom axiom)
        {
            return _metadata.TryGetValue(axiom, out var meta) ? meta.Tooltip : "A reflective world axiom.";
        }

        private struct AxiomMetadata
        {
            public string DisplayName;
            public string Tooltip;

            public AxiomMetadata(string displayName, string tooltip)
            {
                DisplayName = displayName;
                Tooltip = tooltip;
            }
        }
    }
}

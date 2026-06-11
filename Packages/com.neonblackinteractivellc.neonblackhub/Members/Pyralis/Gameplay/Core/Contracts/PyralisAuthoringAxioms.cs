using System;

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

    public static class AuthoringWorldAxiomRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<AuthoringWorldAxiom, AxiomMetadata> _metadata = new System.Collections.Generic.Dictionary<AuthoringWorldAxiom, AxiomMetadata>
        {
            { AuthoringWorldAxiom.Dimensions2D, new AxiomMetadata("2D Logic", "The world logic operates in a flat 2D plane (XY or XZ).") },
            { AuthoringWorldAxiom.Dimensions3D, new AxiomMetadata("3D Logic", "The world logic operates in a full 3D coordinate space (XYZ).") },
            { AuthoringWorldAxiom.GravityVertical, new AxiomMetadata("Vertical Gravity", "Standard downward gravity vector affecting mechanics.") },
            { AuthoringWorldAxiom.GravityNone, new AxiomMetadata("Zero Gravity", "No inherent gravity vector; movement mechanics are unconstrained.") },
            { AuthoringWorldAxiom.GravityRadial, new AxiomMetadata("Radial Gravity", "Gravity pulls toward a specific point or center in the world.") },
            { AuthoringWorldAxiom.Realtime, new AxiomMetadata("Real-time Sequence", "Continuous time flow; mechanical actions resolve immediately.") },
            { AuthoringWorldAxiom.TurnBased, new AxiomMetadata("Turn-based Sequence", "Discrete time steps; mechanics are phased or queued.") },
            { AuthoringWorldAxiom.BoundedSpace, new AxiomMetadata("Bounded Topology", "The physical world has explicit limits or walls.") },
            { AuthoringWorldAxiom.WrappedSpace, new AxiomMetadata("Wrapped Topology", "Mechanically, moving past one edge teleports to the opposite edge.") },
            { AuthoringWorldAxiom.InfiniteSpace, new AxiomMetadata("Infinite Topology", "The physical world has no inherent mechanical bounds or limits.") },
            { AuthoringWorldAxiom.Networked, new AxiomMetadata("Networked", "Game state and mechanics are replicated across a network.") }
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

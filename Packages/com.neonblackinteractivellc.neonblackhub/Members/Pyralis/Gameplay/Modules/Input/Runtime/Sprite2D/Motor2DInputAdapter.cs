using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Input
{
/// <summary>
/// Supported neutral input adapter for <see cref="Motor2D"/>.
/// Use this component on player-controlled 2D pawns.
/// It translates Unity Input System actions into movement direction for the 2D motor.
/// </summary>
[AuthoringContract(
        Category = "Input, Movement",
        CapabilityPath = "Core Setup/Input/Sprite2D Motor Input Adapter",
        Surface = AuthoringSurface.Adapter,
        Summary = "Primary input module for 2D characters. Translates participant input into Motor2D movement.",
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Character.Motor2D" },
        PrerequisiteStableIds = new[] { "input.profile", "movement.pawn.2d" },
        RouteStage = "Pawn Prefab",
        RouteOrder = 95,
        SetupDomain = "Input",
        ProofTarget = "Participant input reaches the 2D pawn movement receiver.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SuccessChecks = new[] { "Verify that player input moves the pawn in 2D space and respects the active InputProfile." },
        RoleTags = new[] { "IntentRouteEssential", "InputRouteSupport", "ParticipantInputConsumer", "Motor2DInput", "PawnInputAdapter" },
        Tags = new[] { "capability:Input", "capability:Movement", "runtime:CharacterPawnGameplay", "axiom:Dimensions2D" },
        Selectable = false
    )]
[AddComponentMenu("NeonBlack/Gameplay/Modules/Input/Sprite2D/Motor Input Adapter")]
public class Motor2DInputAdapter : PlayerInputHandler
{
    // Motor2DInputAdapter inherits the Participant-ready flow from PlayerInputHandler.
    // It provides the concrete component identity for 2D pawn composition.
}
}

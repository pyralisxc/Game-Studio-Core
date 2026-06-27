using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Modules.Character;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Input
{
/// <summary>
/// Supported neutral input adapter for <see cref="Motor2D"/>.
/// Use this component on player-controlled 2D pawns.
/// It translates Unity Input System actions into movement direction for the 2D motor.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Input | AuthoringCapability.Movement, 
    CapabilityPath = "Core Setup/Input/Sprite2D Motor Input Adapter",
    RuntimeFamilies = new[] { RuntimeCapabilityFamily.CharacterPawnGameplay },
    Relevance = "Primary input module for 2D characters. Translates participant input into Motor2D movement.",
    Axioms = AuthoringWorldAxiom.Dimensions2D,
    RequiredComponents = new[] { typeof(Motor2D) },
    RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.InputRouteSupport, "ParticipantInputConsumer", "Motor2DInput", "PawnInputAdapter" },
    Proof = "Verify that player input moves the pawn in 2D space and respects the active InputProfile.",
    Surface = AuthoringContractSurface.Adapter
)]
[AddComponentMenu("NeonBlack/Gameplay/Modules/Input/Sprite2D/Motor Input Adapter")]
public class Motor2DInputAdapter : PlayerInputHandler
{
    // Motor2DInputAdapter inherits the Participant-ready flow from PlayerInputHandler.
    // It provides the concrete component identity for 2D pawn composition.
}
}

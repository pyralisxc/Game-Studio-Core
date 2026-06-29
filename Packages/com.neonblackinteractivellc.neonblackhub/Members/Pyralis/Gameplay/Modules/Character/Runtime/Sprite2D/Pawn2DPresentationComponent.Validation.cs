using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            SpriteRenderer resolvedSpriteRenderer = spriteRenderer != null
                ? spriteRenderer
                : GetComponentInChildren<SpriteRenderer>(true);
            if (resolvedSpriteRenderer == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Sprite Renderer is empty and no child SpriteRenderer was found.",
                    nameof(spriteRenderer),
                    nameof(Pawn2DPresentationComponent),
                    "Assign Pawn2DPresentationComponent.spriteRenderer or add a child SpriteRenderer for this 2D pawn.",
                    "The 2D pawn has a visible sprite surface.",
                    "Pawn2DPresentation.SpriteRenderer.Missing");
            }
            else if (resolvedSpriteRenderer.enabled && resolvedSpriteRenderer.sprite == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "SpriteRenderer is enabled but no Sprite is assigned.",
                    "SpriteRenderer.sprite",
                    nameof(Pawn2DPresentationComponent),
                    "Assign a character Sprite on the pawn SpriteRenderer, or disable the SpriteRenderer until a different presentation route owns visuals.",
                    "The 2D pawn has a visible sprite when Play Mode starts.",
                    "Pawn2DPresentation.SpriteRenderer.Sprite.Missing");
            }

            if (GetComponent<IActorAnimationController>() == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Pawn2DPresentationComponent needs a component that implements IActorAnimationController.",
                    "IActorAnimationController",
                    nameof(Pawn2DPresentationComponent),
                    "Add ActorAnimationDriver or another presentation-owned animation controller to the pawn root.",
                    "The 2D pawn can receive animation signals from movement and feedback.",
                    "Pawn2DPresentation.AnimationController.Missing");
            }

            if (stretchAmount < 1f)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Stretch Amount should be at least 1.",
                    nameof(stretchAmount),
                    nameof(Pawn2DPresentationComponent),
                    "Set Pawn2DPresentationComponent.stretchAmount to 1 or higher.",
                    "Squash/stretch never inverts or shrinks below the authored baseline.",
                    "Pawn2DPresentation.StretchAmount.Minimum");
            }
        }
    }
}

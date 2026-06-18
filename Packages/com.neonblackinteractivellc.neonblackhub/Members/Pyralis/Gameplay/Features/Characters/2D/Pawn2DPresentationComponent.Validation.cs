using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (spriteRenderer == null && GetComponentInChildren<SpriteRenderer>(true) == null)
                yield return PyralisRuntimeValidationIssue.Required("Sprite Renderer is empty and no child SpriteRenderer was found.");
            if (stretchAmount < 1f)
                yield return PyralisRuntimeValidationIssue.Required("Stretch Amount should be at least 1.");
        }
    }
}

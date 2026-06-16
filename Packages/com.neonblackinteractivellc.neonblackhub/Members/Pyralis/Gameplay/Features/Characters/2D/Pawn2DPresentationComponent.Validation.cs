using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (spriteRenderer == null && GetComponentInChildren<SpriteRenderer>(true) == null)
                yield return "Sprite Renderer is empty and no child SpriteRenderer was found.";
            if (stretchAmount < 1f)
                yield return "Stretch Amount should be at least 1.";
        }
    }
}

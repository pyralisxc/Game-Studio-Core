using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AuthoringContract(
        StableId = "enemy.reaction.state",
        Category = "Combat",
        Surface = AuthoringSurface.Profile,
        RequiredFields = new[]
        {
            "EnemyReactionComponent.reactionProfile"
        },
        RequiredInterfaces = new[] { typeof(IEnemyReactionState) },
        SetupSteps = new[]
        {
            "add EnemyReactionComponent to the enemy root",
            "assign EnemyReactionProfile",
            "ensure HealthComponent and Health/animation sources are present for reaction pathways"
        },
        SuccessChecks = new[] { "Verify that IsReactionLocked is true when the enemy is staggered or hit." },
        Tags = new[] { "capability:Combat" },
        Selectable = false
    )]
    public interface IEnemyReactionState
    {
        bool IsReactionLocked { get; }
    }
}

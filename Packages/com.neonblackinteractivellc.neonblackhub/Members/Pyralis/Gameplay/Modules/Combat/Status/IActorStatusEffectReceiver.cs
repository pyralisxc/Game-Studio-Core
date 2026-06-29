using System;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        StableId = "combat.status-effect.receiver",
        Category = "Combat, Stats",
        Surface = AuthoringSurface.Profile,
        Summary = "Handles the application and management of combat status effects and stat modifiers on actors.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/actor-status",
        RequiredFields = new[]
        {
            "ActorStatusEffectComponent.statusProfile"
        },
        RequiredInterfaces = new Type[]
        {
            typeof(IActorStatusEffectReceiver),
            typeof(IDamageModifier)
        },
        SetupSteps = new[]
        {
            "add ActorStatusEffectComponent to the actor root",
            "assign ActorStatusEffectProfile",
            "ensure HealthComponent and status modifier receivers are present on actors that consume status effects"
        },
        SuccessChecks = new[] { "Apply a status effect to an actor and verify it appears in the active effect list." },
        Tags = new[] { "capability:Combat", "capability:Stats" },
        Selectable = false
    )]
    public interface IActorStatusEffectReceiver : IActorStatusEffectSink
    {
    }
}

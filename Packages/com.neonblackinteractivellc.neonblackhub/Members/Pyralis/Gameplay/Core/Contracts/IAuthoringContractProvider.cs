using System.Collections.Generic;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Interface for types that can provide one or more authoring contracts.
    /// This allows for dynamic or multiple contracts per type beyond the static attribute.
    /// </summary>
    public interface IAuthoringContractProvider
    {
        IEnumerable<AuthoringContractAttribute> GetAuthoringContracts();
    }
}

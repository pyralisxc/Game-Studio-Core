using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Optional validation surface for authored runtime components that need
    /// configuration checks beyond simple interface presence.
    /// </summary>
    public interface IRuntimeValidationProvider
    {
        IEnumerable<string> GetRuntimeValidationIssues();
    }
}
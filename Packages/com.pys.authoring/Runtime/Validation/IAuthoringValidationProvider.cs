using System;
using System.Collections.Generic;

namespace Pys.Authoring.Contracts
{
    [Obsolete("Target projects should own validation and expose public runtime validation methods such as GetRuntimeValidationIssues. PYS observes those methods reflectively.")]
    public interface IAuthoringValidationProvider
    {
        IEnumerable<AuthoringIssue> GetAuthoringIssues();
    }
}

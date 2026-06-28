using System.Collections.Generic;

namespace Pys.Authoring.Contracts
{
    public interface IAuthoringValidationProvider
    {
        IEnumerable<AuthoringIssue> GetAuthoringIssues();
    }
}

using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Tests
{
    /// <summary>
    /// A test interface to verify that the reflective authoring system picks it up.
    /// </summary>
    [AuthoringContract(Capability = AuthoringCapability.Setup, Relevance = "This is a test contract to verify the reflective authoring UI.", Axioms = AuthoringWorldAxiom.None)]
    public interface ITestReflectiveContract
    {
        void DoSomething();
    }

    public class TestReflectiveContractComponent : MonoBehaviour, ITestReflectiveContract
    {
        public void DoSomething() { Debug.Log("Test"); }
    }
}

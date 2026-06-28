using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.InputRouting;
using NeonBlack.Gameplay.Modules.Character;
using NUnit.Framework;
using Pys.Authoring.Editor.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PyralisAuthoringPackageConsumptionTests
    {
        private const string GameplayRoot =
            "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay";

        [Test]
        public void PyralisAuthoringImplementation_IsNotActiveInNeonBlackPackage()
        {
            string projectRoot = Path.Combine(Application.dataPath, "..");

            Assert.That(Directory.Exists(Path.Combine(projectRoot, GameplayRoot, "Editor", "Authoring")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(projectRoot, GameplayRoot, "Core", "Contracts", "Authoring")), Is.False);
        }

        [Test]
        public void RepresentativePyralisContracts_ResolveThroughPysAuthoring()
        {
            AssertPysContract(typeof(SessionDefinition), "session.definition", "Session");
            AssertPysContract(typeof(ParticipantInputRouter), "participant.input-router", "Input, Setup");
            AssertPysContract(typeof(Motor2D), null, "Kinetic Motor2 D");
        }

        [Test]
        public void PyralisInspectorHandoff_OpensStandalonePysAuthoring()
        {
            string projectRoot = Path.Combine(Application.dataPath, "..");
            string handoffPath = Path.Combine(projectRoot, GameplayRoot, "Editor", "Inspectors", "Pyralis", "PyralisInspectorHandoff.cs");
            string source = File.ReadAllText(handoffPath);

            Assert.That(source, Does.Contain("Tools/PYS/Authoring"));
            Assert.That(source, Does.Not.Contain("PyralisAuthoringWindow.Open"));
        }

        [Test]
        public void OldPyralisAuthoringSymbols_DoNotRemainInActiveSource()
        {
            string projectRoot = Path.Combine(Application.dataPath, "..");
            string[] activeFiles = Directory.GetFiles(Path.Combine(projectRoot, GameplayRoot), "*.cs", SearchOption.AllDirectories);

            string[] forbidden =
            {
                "ResolvedAuthoringContractRegistry",
                "AuthoringCapability.",
                "AuthoringWorldAxiom.",
                "AuthoringContractSurface.",
                "RuntimeCapabilityFamily.",
                "PyralisAuthoringWindow",
                "PyralisAuthoringSetupGraph",
                "PyralisSetupDependencyTree"
            };

            string[] hits = activeFiles
                .SelectMany(path => forbidden
                    .Where(token => File.ReadAllText(path).Contains(token))
                    .Select(token => path + " :: " + token))
                .ToArray();

            Assert.That(hits, Is.Empty, string.Join("\n", hits));
        }

        [Test]
        public void ExplicitPyralisAuthoringContractStableIds_AreUnique()
        {
            string projectRoot = Path.Combine(Application.dataPath, "..");
            string[] activeFiles = Directory.GetFiles(Path.Combine(projectRoot, GameplayRoot), "*.cs", SearchOption.AllDirectories);
            var contractPattern = new Regex(@"\[AuthoringContract\((.*?)\)\]", RegexOptions.Singleline);
            var stableIdPattern = new Regex(@"StableId\s*=\s*""([^""]+)""");

            var duplicateStableIds = activeFiles
                .SelectMany(path =>
                {
                    string source = File.ReadAllText(path);
                    return contractPattern.Matches(source)
                        .Cast<Match>()
                        .Select(match => new
                        {
                            Path = path,
                            StableId = stableIdPattern.Match(match.Groups[1].Value).Groups[1].Value
                        })
                        .Where(match => !string.IsNullOrWhiteSpace(match.StableId));
                })
                .GroupBy(match => match.StableId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key + " :: " + string.Join(", ", group.Select(match => match.Path)))
                .ToArray();

            Assert.That(duplicateStableIds, Is.Empty, string.Join("\n", duplicateStableIds));
        }

        private static void AssertPysContract(System.Type type, string stableId, string category)
        {
            var contracts = AuthoringContractResolver.Resolve(type);
            Assert.That(contracts, Is.Not.Empty, type.FullName);
            var contract = string.IsNullOrWhiteSpace(stableId)
                ? contracts[0]
                : contracts.FirstOrDefault(candidate => candidate.StableId == stableId);

            Assert.That(contract, Is.Not.Null, type.FullName);
            Assert.That(contract.Category, Is.EqualTo(category));
            Assert.That(contract.Surface, Is.Not.EqualTo(Pys.Authoring.Contracts.AuthoringSurface.Auto));
        }
    }
}

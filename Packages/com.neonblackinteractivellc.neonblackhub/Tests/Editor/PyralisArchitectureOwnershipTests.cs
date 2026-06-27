using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PyralisArchitectureOwnershipTests
    {
        private const string GameplayRoot =
            "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay";

        [Test]
        public void CoreFolder_DoesNotContainOptionalDomainImplementations()
        {
            string coreRoot = Path.Combine(Application.dataPath, "..", GameplayRoot, "Core");
            string[] forbiddenSegments =
            {
                Path.Combine("Core", "Actions"),
                Path.Combine("Core", "Rpg"),
                Path.Combine("Core", "Navigation"),
                Path.Combine("Core", "Navigation", "UI"),
                Path.Combine("Core", "Rules")
            };

            string[] existingForbidden = forbiddenSegments
                .Select(segment => Path.Combine(Application.dataPath, "..", GameplayRoot, segment))
                .Where(Directory.Exists)
                .ToArray();

            Assert.That(existingForbidden, Is.Empty, string.Join("\n", existingForbidden));
            Assert.That(Directory.Exists(coreRoot), Is.True);
        }

        [Test]
        public void CoreFolder_DoesNotContainGlueImplementations()
        {
            string coreRoot = Path.Combine(Application.dataPath, "..", GameplayRoot, "Core");
            string[] forbiddenFiles =
            {
                "SceneLoader.cs",
                "SceneNavigator.cs",
                "TimeManager.cs",
                "LocalSessionOwnershipService.cs",
                "LocalParticipantAuthorityService.cs"
            };

            string[] existingForbidden = forbiddenFiles
                .Select(fileName => Path.Combine(coreRoot, fileName))
                .Where(File.Exists)
                .ToArray();

            Assert.That(existingForbidden, Is.Empty, string.Join("\n", existingForbidden));
        }

        [Test]
        public void CoreFolder_UsesContractsAndTypesRoots()
        {
            string coreRoot = Path.Combine(Application.dataPath, "..", GameplayRoot, "Core");
            string[] topLevelSourceFiles = Directory.GetFiles(coreRoot, "*.cs", SearchOption.TopDirectoryOnly);

            Assert.That(Directory.Exists(Path.Combine(coreRoot, "Contracts")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(coreRoot, "Types")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(coreRoot, "Types", "Actions")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(coreRoot, "Types", "Animation")), Is.True);
            Assert.That(File.Exists(Path.Combine(coreRoot, "Types", "MovementMode.cs")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(coreRoot, "AuthoringContracts")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(coreRoot, "RuntimeContracts")), Is.False);
            Assert.That(topLevelSourceFiles, Is.Empty, string.Join("\n", topLevelSourceFiles));
        }
    }
}

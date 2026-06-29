using System;
using System.IO;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RuntimeTickOwnershipTests
    {
        [Test]
        public void RuleChangingRuntime_DoesNotReadUnityTimeDirectly()
        {
            string projectRoot = Directory.GetCurrentDirectory();
            string gameplayRoot = Path.Combine(
                projectRoot,
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Gameplay");

            Assert.That(Directory.Exists(gameplayRoot), Is.True, $"Missing gameplay root: {gameplayRoot}");

            string[] scanRoots =
            {
                Path.Combine(gameplayRoot, "Modules"),
                Path.Combine(gameplayRoot, "Glue"),
            };

            var violations = new System.Collections.Generic.List<string>();

            foreach (string root in scanRoots)
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
                {
                    string normalized = file.Replace('\\', '/');
                    if (IsAllowedLocalUnityTickFile(normalized))
                        continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith("///", StringComparison.Ordinal))
                            continue;

                        bool readsUnityTime =
                            line.Contains("Time.deltaTime", StringComparison.Ordinal) ||
                            line.Contains("Time.fixedDeltaTime", StringComparison.Ordinal) ||
                            line.Contains("Time.unscaledDeltaTime", StringComparison.Ordinal);
                        bool declaresUnityTick =
                            line.Contains(" void Update(", StringComparison.Ordinal) ||
                            line.Contains(" void FixedUpdate(", StringComparison.Ordinal) ||
                            line.Contains(" void LateUpdate(", StringComparison.Ordinal);

                        if (readsUnityTime || declaresUnityTick)
                        {
                            string relative = Path.GetRelativePath(projectRoot, file).Replace('\\', '/');
                            violations.Add($"{relative}:{i + 1}: {line}");
                        }
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Gameplay-rule runtime should use GameplayTickBehaviour/GameplayTickContext. " +
                "Presentation, UI, and local input polling may keep Unity Update/Time when listed as allowed display/input surfaces.");
        }

        private static bool IsAllowedLocalUnityTickFile(string normalizedPath)
        {
            return normalizedPath.Contains("/Modules/Combat/UI/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Feedback/Runtime/UI/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Feedback/Runtime/Shared/ActorFloatingFeedbackReceiver", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Hazards/Runtime/Shared/HazardFeedbackRuntime", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Environment/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Tabletop/UI/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Input/Runtime/Sprite2D/VirtualJoystick.cs", StringComparison.Ordinal)
                || normalizedPath.Contains("/Modules/Character/Runtime/Sprite2D/Pawn2DPresentationComponent", StringComparison.Ordinal)
                || normalizedPath.Contains("/Glue/SceneFlow/Navigation/", StringComparison.Ordinal);
        }
    }
}

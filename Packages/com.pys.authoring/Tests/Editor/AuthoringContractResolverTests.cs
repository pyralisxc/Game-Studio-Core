using NUnit.Framework;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Contracts;

namespace Pys.Authoring.Editor.Tests
{
    public sealed class AuthoringContractResolverTests
    {
        [Test]
        public void Resolve_UsesDeclaredStableMetadata()
        {
            var contracts = AuthoringContractResolver.Resolve(typeof(ResolverFixture));

            Assert.That(contracts, Has.Count.EqualTo(1));
            Assert.That(contracts[0].StableId, Is.EqualTo("fixture.contract"));
            Assert.That(contracts[0].Category, Is.EqualTo("Fixture"));
            Assert.That(contracts[0].CapabilityPath, Is.EqualTo("Fixture/Example"));
            Assert.That(contracts[0].Surface, Is.EqualTo(AuthoringSurface.RequiredSetup));
            Assert.That(contracts[0].MetadataGaps, Is.Empty);
        }

        [Test]
        public void Resolve_ReportsSelectableMetadataGaps()
        {
            var contracts = AuthoringContractResolver.Resolve(typeof(MissingMetadataFixture));

            Assert.That(contracts, Has.Count.EqualTo(1));
            Assert.That(contracts[0].MetadataGaps, Does.Contain("category"));
            Assert.That(contracts[0].MetadataGaps, Does.Contain("surface"));
            Assert.That(contracts[0].MetadataGaps, Does.Contain("capabilityPath"));
        }

        [Test]
        public void Resolve_CarriesIntentCompositionAndReadinessHints()
        {
            var contracts = AuthoringContractResolver.Resolve(typeof(IntentCompositionFixture));

            Assert.That(contracts, Has.Count.EqualTo(1));
            Assert.That(contracts[0].SuccessDescription, Is.EqualTo("Set up a controllable actor with camera support."));
            Assert.That(contracts[0].ReadinessHint, Is.EqualTo("Actor can enter Play Mode and receive input."));
            Assert.That(contracts[0].ValidationOwnerStableId, Is.EqualTo("validation.actor.route"));
            Assert.That(contracts[0].ExpectedEvidence, Is.EqualTo(new[] { "scene.object:Actor", "component:Controller" }));
            Assert.That(contracts[0].CompletionSignals, Is.EqualTo(new[] { "Play Mode enters without validation issues" }));
            Assert.That(contracts[0].IntentToggles, Is.EqualTo(new[] { "Combat", "Camera" }));
            Assert.That(contracts[0].IntentLanes, Is.EqualTo(new[] { "Sprite2D", "Rigged3D" }));
            Assert.That(contracts[0].CompatibleStableIds, Is.EqualTo(new[] { "feature.inventory" }));
            Assert.That(contracts[0].SupportingStableIds, Is.EqualTo(new[] { "setup.camera" }));
            Assert.That(contracts[0].HoverExplanations, Is.EqualTo(new[] { "Camera adds follow framing." }));
        }

        [TestCase("simple_name", "Simple Name")]
        [TestCase("simple-name", "Simple Name")]
        [TestCase("simple.name", "Simple Name")]
        [TestCase("simple/name", "Simple Name")]
        [TestCase("simple:name", "Simple Name")]
        [TestCase("simpleName", "Simple Name")]
        [TestCase("SimpleName", "Simple Name")]
        [TestCase("XMLParser", "XML Parser")]
        [TestCase("HTTPRequest2DHandler", "HTTP Request 2D Handler")]
        [TestCase("camera2dRig", "Camera 2D Rig")]
        [TestCase("UIVFX3DOverlay", "UI VFX 3D Overlay")]
        public void Prettify_HandlesCommonNamingStyles(string input, string expected)
        {
            Assert.That(AuthoringContractResolver.Prettify(input), Is.EqualTo(expected));
        }

        [AuthoringContract(
            StableId = "fixture.contract",
            DisplayName = "Fixture Contract",
            Category = "Fixture",
            CapabilityPath = "Fixture/Example",
            Surface = AuthoringSurface.RequiredSetup,
            RequiredFields = new[] { "field" })]
        private sealed class ResolverFixture
        {
        }

        [AuthoringContract]
        private sealed class MissingMetadataFixture
        {
        }

        [AuthoringContract(
            StableId = "fixture.intent",
            DisplayName = "Actor Route",
            Category = "Fixture",
            CapabilityPath = "Fixture/Actor",
            Surface = AuthoringSurface.Goal,
            SuccessDescription = "Set up a controllable actor with camera support.",
            ReadinessHint = "Actor can enter Play Mode and receive input.",
            ValidationOwnerStableId = "validation.actor.route",
            ExpectedEvidence = new[] { "scene.object:Actor", "component:Controller" },
            CompletionSignals = new[] { "Play Mode enters without validation issues" },
            IntentToggles = new[] { "Combat", "Camera" },
            IntentLanes = new[] { "Sprite2D", "Rigged3D" },
            CompatibleStableIds = new[] { "feature.inventory" },
            SupportingStableIds = new[] { "setup.camera" },
            HoverExplanations = new[] { "Camera adds follow framing." })]
        private sealed class IntentCompositionFixture
        {
        }
    }
}

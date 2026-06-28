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
    }
}

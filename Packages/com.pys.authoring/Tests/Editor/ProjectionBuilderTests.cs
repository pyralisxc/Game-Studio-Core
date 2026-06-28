using NUnit.Framework;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Hygiene;
using Pys.Authoring.Editor.Projections;

namespace Pys.Authoring.Editor.Tests
{
    public sealed class ProjectionBuilderTests
    {
        [Test]
        public void BuildFacts_CountsGraphNodeKinds()
        {
            AuthoringGraph graph = new AuthoringGraph();
            graph.Nodes.Add(new AuthoringGraphNode("assembly:test", "Test", AuthoringGraphNodeKind.Assembly));
            graph.Nodes.Add(new AuthoringGraphNode("contract:test", "Test", AuthoringGraphNodeKind.Contract));
            graph.Nodes.Add(new AuthoringGraphNode("issue:test", "Issue", AuthoringGraphNodeKind.Issue));
            graph.Nodes.Add(new AuthoringGraphNode("asset:test", "Asset", AuthoringGraphNodeKind.Asset));

            FactsProjection facts = AuthoringProjectionBuilder.BuildFacts(graph);

            Assert.That(facts.AssemblyCount, Is.EqualTo(1));
            Assert.That(facts.ContractCount, Is.EqualTo(1));
            Assert.That(facts.IssueCount, Is.EqualTo(1));
            Assert.That(facts.AssetCount, Is.EqualTo(1));
            Assert.That(facts.Rows, Has.Count.GreaterThanOrEqualTo(4));
            Assert.That(facts.Rows, Has.Some.Matches<FactRow>(row => row.Kind == "Contract" && row.Label == "Test"));
        }

        [Test]
        public void BuildIntent_ProjectsSelectableContracts()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["selectable"] = "true";
            contract.Metadata["category"] = "System";
            contract.Metadata["capabilityPath"] = "System/Example";
            contract.Metadata["surface"] = "RuntimeComponent";
            contract.Metadata["summary"] = "Example summary.";
            graph.Nodes.Add(contract);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);

            Assert.That(intent.SelectableCount, Is.EqualTo(1));
            Assert.That(intent.Rows, Has.Count.EqualTo(1));
            Assert.That(intent.Rows[0].DisplayName, Is.EqualTo("Test Contract"));
            Assert.That(intent.Rows[0].CapabilityPath, Is.EqualTo("System/Example"));
            Assert.That(intent.Rows[0].Selectable, Is.True);
        }

        [Test]
        public void BuildIntent_ProjectsSelectedContract()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["selectable"] = "true";
            graph.Nodes.Add(contract);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph, "contract:test");

            Assert.That(intent.SelectedContractId, Is.EqualTo("contract:test"));
            Assert.That(intent.SelectedDisplayName, Is.EqualTo("Test Contract"));
        }

        [Test]
        public void BuildIntent_DoesNotMixDuplicateStableIdContracts()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode health = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@HealthComponent", "Health Component", AuthoringGraphNodeKind.Contract);
            health.Metadata["stableId"] = "proof.npc-enemy-behavior";
            health.Metadata["sourceType"] = "Game.HealthComponent";
            health.Metadata["sourcePath"] = "Packages/Game/HealthComponent.cs";
            health.Metadata["selectable"] = "true";
            health.Metadata["category"] = "Combat";
            health.Metadata["capabilityPath"] = "Combat/Health/Health Component";
            health.Metadata["summary"] = "Tracks actor health.";
            health.Metadata["duplicateStableId"] = "true";
            graph.Nodes.Add(health);

            AuthoringGraphNode enemy = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@EnemyAI", "Enemy AI", AuthoringGraphNodeKind.Contract);
            enemy.Metadata["stableId"] = "proof.npc-enemy-behavior";
            enemy.Metadata["sourceType"] = "Game.EnemyAI";
            enemy.Metadata["sourcePath"] = "Packages/Game/EnemyAI.cs";
            enemy.Metadata["selectable"] = "true";
            enemy.Metadata["category"] = "Tactics";
            enemy.Metadata["capabilityPath"] = "Movement/Traversal/Enemy AI";
            enemy.Metadata["summary"] = "Handles patrol and detection.";
            enemy.Metadata["duplicateStableId"] = "true";
            graph.Nodes.Add(enemy);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);

            Assert.That(intent.Rows, Has.Count.EqualTo(2));
            Assert.That(intent.Rows[0].DisplayName, Is.EqualTo("Health Component"));
            Assert.That(intent.Rows[0].CapabilityPath, Is.EqualTo("Combat/Health/Health Component"));
            Assert.That(intent.Rows[0].Summary, Is.EqualTo("Tracks actor health."));
            Assert.That(intent.Rows[0].Selectable, Is.False);
            Assert.That(intent.Rows[0].DisabledReason, Does.Contain("Duplicate StableId"));
            Assert.That(intent.Rows[1].DisplayName, Is.EqualTo("Enemy AI"));
            Assert.That(intent.Rows[1].CapabilityPath, Is.EqualTo("Movement/Traversal/Enemy AI"));
            Assert.That(intent.Rows[1].Summary, Is.EqualTo("Handles patrol and detection."));
        }

        [Test]
        public void BuildOverview_ReportsIssueAsNextAction()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode issue = new AuthoringGraphNode("issue:test", "Fix Me", AuthoringGraphNodeKind.Issue);
            issue.Metadata["issueCode"] = "Example.Issue";
            issue.Metadata["nativeAction"] = "Inspect the object.";
            issue.Metadata["actionKind"] = AuthoringActionKind.InspectObject.ToString();
            graph.Nodes.Add(issue);

            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph);

            Assert.That(overview.IssueCount, Is.EqualTo(1));
            Assert.That(overview.NextAction, Is.EqualTo("Inspect the object."));
            Assert.That(overview.Reason, Is.EqualTo("Example.Issue"));
        }

        [Test]
        public void BuildOverview_UsesSelectedGuidePath()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["setupSteps"] = "Assign the data asset.";
            graph.Nodes.Add(contract);
            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:test");

            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

            Assert.That(overview.SelectedIntent, Is.EqualTo("Test Contract"));
            Assert.That(overview.NextAction, Is.EqualTo("Assign the data asset."));
            Assert.That(overview.Readiness, Is.EqualTo("Blocked"));
        }

        [Test]
        public void BuildGuide_UsesSelectedContractInstanceMetadata()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode selected = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@EnemyAI", "Enemy AI", AuthoringGraphNodeKind.Contract);
            selected.Metadata["stableId"] = "proof.npc-enemy-behavior";
            selected.Metadata["selectable"] = "true";
            selected.Metadata["capabilityPath"] = "Movement/Traversal/Enemy AI";
            selected.Metadata["setupSteps"] = "Assign an enemy profile.";
            graph.Nodes.Add(selected);

            AuthoringGraphNode other = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@HealthComponent", "Health Component", AuthoringGraphNodeKind.Contract);
            other.Metadata["stableId"] = "proof.npc-enemy-behavior";
            other.Metadata["selectable"] = "true";
            other.Metadata["metadataGaps"] = "capabilityPath";
            graph.Nodes.Add(other);

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, selected.Id);
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

            Assert.That(guide.SelectedDisplayName, Is.EqualTo("Enemy AI"));
            Assert.That(guide.Rows, Has.None.Matches<GuideRow>(row => row.OwnerId == selected.Id && row.Detail.Contains("capabilityPath")));
            Assert.That(overview.NextAction, Is.EqualTo("Assign an enemy profile."));
            Assert.That(overview.Reason, Is.EqualTo("Assign an enemy profile."));
        }

        [Test]
        public void BuildGuide_ProjectsActionKindAndLabel()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode issue = new AuthoringGraphNode("issue:test", "Assign Thing", AuthoringGraphNodeKind.Issue);
            issue.Metadata["issueCode"] = "Example.Assign";
            issue.Metadata["nativeAction"] = "Assign a field.";
            issue.Metadata["successCheck"] = "Field is assigned.";
            issue.Metadata["actionKind"] = AuthoringActionKind.AssignField.ToString();
            graph.Nodes.Add(issue);

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph);

            Assert.That(guide.Rows, Has.Count.EqualTo(1));
            Assert.That(guide.Rows[0].ActionKind, Is.EqualTo("AssignField"));
            Assert.That(guide.Rows[0].ActionLabel, Is.EqualTo("Assign Field"));
        }

        [Test]
        public void BuildGuide_ProjectsSelectedProofPath()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["surface"] = AuthoringSurface.Goal.ToString();
            contract.Metadata["setupSteps"] = "Create a scene object.\nAssign a profile.";
            contract.Metadata["successChecks"] = "Enter Play Mode and verify behavior.";
            graph.Nodes.Add(contract);

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:test");

            Assert.That(guide.SelectedDisplayName, Is.EqualTo("Test Contract"));
            Assert.That(guide.ProofTarget, Is.EqualTo("Test Contract"));
            Assert.That(guide.ProofReady, Is.False);
            Assert.That(guide.Rows, Has.Count.EqualTo(3));
            Assert.That(guide.Rows[0].Order, Is.EqualTo(1));
            Assert.That(guide.Rows[0].Role, Is.EqualTo("SetupStep"));
            Assert.That(guide.Rows[0].BlocksProof, Is.True);
            Assert.That(guide.Rows[2].Role, Is.EqualTo("ProofCheck"));
            Assert.That(guide.Rows[2].BlocksProof, Is.False);
        }

        [Test]
        public void BuildMap_IncludesAssetRows()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode asset = new AuthoringGraphNode("asset:test", "Test Asset", AuthoringGraphNodeKind.Asset);
            asset.Metadata["sourcePath"] = "Assets/Test.asset";
            graph.Nodes.Add(asset);

            MapProjection map = AuthoringProjectionBuilder.BuildMap(graph);

            Assert.That(map.Rows, Has.Count.EqualTo(1));
            Assert.That(map.Rows[0].Kind, Is.EqualTo("Asset"));
            Assert.That(map.Rows[0].SourcePath, Is.EqualTo("Assets/Test.asset"));
        }

        [Test]
        public void BuildMap_DoesNotIncludeReflectedFieldsAsAssets()
        {
            AuthoringGraph graph = new AuthoringGraph();
            graph.Nodes.Add(new AuthoringGraphNode("field:Game.Component.profile", "profile", AuthoringGraphNodeKind.Field));
            graph.Nodes.Add(new AuthoringGraphNode("asset:test", "Test Asset", AuthoringGraphNodeKind.Asset));

            MapProjection map = AuthoringProjectionBuilder.BuildMap(graph);

            Assert.That(map.Rows, Has.Count.EqualTo(1));
            Assert.That(map.Rows[0].Id, Is.EqualTo("asset:test"));
        }

        [Test]
        public void BuildHygiene_CreatesLensPacketsFromTheSameRows()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["metadataGaps"] = "capabilityPath";
            graph.Nodes.Add(contract);
            graph.Nodes.Add(new AuthoringGraphNode("script:test", "Test Script", AuthoringGraphNodeKind.Script));
            graph.Nodes.Add(new AuthoringGraphNode("issue:test", "Missing Data", AuthoringGraphNodeKind.Issue));

            for (int i = 0; i < 9; i++)
                graph.Edges.Add(new AuthoringGraphEdge("script:test", "namespace:" + i, AuthoringGraphEdgeKind.NamespaceUsing));

            graph.Edges.Add(new AuthoringGraphEdge("assembly:a", "assembly:b", AuthoringGraphEdgeKind.AssemblyReference));

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows.Count, Is.EqualTo(4));
            Assert.That(hygiene.Lenses.Count, Is.EqualTo(7));
            Assert.That(FindLens(hygiene, HygieneLensKind.Overview).Rows.Count, Is.EqualTo(hygiene.Rows.Count));
            Assert.That(FindLens(hygiene, HygieneLensKind.Contracts).Rows.Count, Is.EqualTo(1));
            Assert.That(FindLens(hygiene, HygieneLensKind.Dependencies).Rows.Count, Is.EqualTo(2));
            Assert.That(FindLens(hygiene, HygieneLensKind.ProjectionIntegrity).Rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void BuildHygiene_ReportsDuplicateStableIdsWithSourceProvenance()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode a = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@A", "A", AuthoringGraphNodeKind.Contract);
            a.Metadata["stableId"] = "proof.npc-enemy-behavior";
            a.Metadata["sourceType"] = "Game.A";
            a.Metadata["sourcePath"] = "Packages/Game/A.cs";
            graph.Nodes.Add(a);

            AuthoringGraphNode b = new AuthoringGraphNode("contract:proof.npc-enemy-behavior@B", "B", AuthoringGraphNodeKind.Contract);
            b.Metadata["stableId"] = "proof.npc-enemy-behavior";
            b.Metadata["sourceType"] = "Game.B";
            b.Metadata["sourcePath"] = "Packages/Game/B.cs";
            graph.Nodes.Add(b);

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.IssueCode == "Contract.StableId.Duplicate"
                && row.Detail.Contains("Game.A")
                && row.Detail.Contains("Packages/Game/A.cs")
                && row.Detail.Contains("Game.B")
                && row.Detail.Contains("Packages/Game/B.cs")));
        }

        [Test]
        public void BuildHygiene_GroupsAssemblyReferenceRowsBySourceAssembly()
        {
            AuthoringGraph graph = new AuthoringGraph();
            graph.Edges.Add(new AuthoringGraphEdge("assembly:a", "assembly:b", AuthoringGraphEdgeKind.AssemblyReference));
            graph.Edges.Add(new AuthoringGraphEdge("assembly:a", "assembly:c", AuthoringGraphEdgeKind.AssemblyReference));
            graph.Edges.Add(new AuthoringGraphEdge("assembly:d", "assembly:e", AuthoringGraphEdgeKind.AssemblyReference));

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Exactly(2).Matches<HygieneRow>(row => row.IssueCode == "Assembly.Reference.Group"));
            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.OwnerId == "assembly:a"
                && row.Detail.Contains("2 reference")
                && row.Detail.Contains("assembly:b")
                && row.Detail.Contains("assembly:c")));
        }

        [Test]
        public void BuildHygiene_EmptyInputStillCreatesLensPackets()
        {
            HygieneProjection hygiene = HygieneProjectionBuilder.Build(null);

            Assert.That(hygiene.Rows.Count, Is.EqualTo(0));
            Assert.That(hygiene.Lenses.Count, Is.EqualTo(7));
            Assert.That(FindLens(hygiene, HygieneLensKind.Overview).Rows.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExportJson_MirrorsProjectionPackets()
        {
            FactsProjection facts = new FactsProjection { AssemblyCount = 2, IssueCount = 3 };
            MapProjection map = new MapProjection();
            map.Rows.Add(new MapRow { Id = "object:a", Label = "Object A", Kind = "SceneObject", SourcePath = "Assets/Test.unity", ComponentCount = 4, IssueCount = 1 });
            OverviewProjection overview = new OverviewProjection { Summary = "Summary A", NextAction = "Inspect A", Reason = "Reason.A", IssueCount = 1 };
            GuideProjection guide = new GuideProjection();
            guide.Rows.Add(new GuideRow { OwnerId = "issue:a", Title = "Issue A", Detail = "Detail A", NativeAction = "Action A", SuccessCheck = "Check A" });
            HygieneProjection hygiene = HygieneProjectionBuilder.Build(BuildGraphWithContractGap());

            string factsJson = ProjectionJsonExporter.ToFactsJson(facts, "Assets");
            string mapJson = ProjectionJsonExporter.ToMapJson(map, "Assets");
            string overviewJson = ProjectionJsonExporter.ToOverviewJson(overview, "Assets");
            string guideJson = ProjectionJsonExporter.ToGuideJson(guide, "Assets");
            string hygieneJson = HygieneJsonExporter.ToJson(hygiene, "Assets");

            Assert.That(factsJson, Does.Contain("\"assemblyCount\": 2"));
            Assert.That(factsJson, Does.Contain("\"issueCount\": 3"));
            Assert.That(mapJson, Does.Contain("\"label\": \"Object A\""));
            Assert.That(overviewJson, Does.Contain("\"nextAction\": \"Inspect A\""));
            Assert.That(guideJson, Does.Contain("\"successCheck\": \"Check A\""));
            Assert.That(hygieneJson, Does.Contain("\"lenses\""));
            Assert.That(hygieneJson, Does.Contain("\"kind\": \"Contracts\""));
            Assert.That(hygieneJson, Does.Contain("\"issueCode\": \"Contract.Metadata.Missing\""));
        }

        [Test]
        public void ExportIntentJson_MirrorsIntentProjection()
        {
            IntentProjection intent = new IntentProjection { SelectableCount = 1 };
            intent.Rows.Add(new IntentRow
            {
                ContractId = "contract:a",
                DisplayName = "Contract A",
                Category = "System",
                CapabilityPath = "System/A",
                Surface = "RuntimeComponent",
                Summary = "Summary A",
                Selectable = true
            });

            string json = ProjectionJsonExporter.ToIntentJson(intent, "Assets");

            Assert.That(json, Does.Contain("\"selectableCount\": 1"));
            Assert.That(json, Does.Contain("\"displayName\": \"Contract A\""));
            Assert.That(json, Does.Contain("\"capabilityPath\": \"System/A\""));
        }

        [Test]
        public void ExportGuideJson_MirrorsSelectedProofPath()
        {
            GuideProjection guide = new GuideProjection
            {
                SelectedContractId = "contract:a",
                SelectedDisplayName = "Contract A",
                ProofTarget = "Contract A",
                ProofReady = false
            };
            guide.Rows.Add(new GuideRow
            {
                Order = 1,
                Role = "SetupStep",
                OwnerId = "contract:a",
                Title = "Complete setup step",
                Detail = "Assign a profile.",
                BlocksProof = true
            });

            string json = ProjectionJsonExporter.ToGuideJson(guide, "Assets");

            Assert.That(json, Does.Contain("\"selectedContractId\": \"contract:a\""));
            Assert.That(json, Does.Contain("\"proofTarget\": \"Contract A\""));
            Assert.That(json, Does.Contain("\"role\": \"SetupStep\""));
            Assert.That(json, Does.Contain("\"blocksProof\": true"));
        }

        private static HygieneLensProjection FindLens(HygieneProjection hygiene, HygieneLensKind kind)
        {
            for (int i = 0; i < hygiene.Lenses.Count; i++)
            {
                if (hygiene.Lenses[i].Kind == kind)
                    return hygiene.Lenses[i];
            }

            Assert.Fail("Expected lens was not created: " + kind);
            return null;
        }

        private static AuthoringGraph BuildGraphWithContractGap()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:a", "Contract A", AuthoringGraphNodeKind.Contract);
            contract.Metadata["metadataGaps"] = "category";
            graph.Nodes.Add(contract);
            return graph;
        }
    }
}

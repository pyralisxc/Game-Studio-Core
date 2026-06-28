using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Hygiene;
using Pys.Authoring.Editor.Projections;
using Pys.Authoring.Editor.Scanning;

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
        public void BuildIntent_ProjectsGoalPatternsInsteadOfSetupInventory()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode session = Contract("contract:session", "Session", "setup.session", "Core", 10);
            session.Metadata["selectable"] = "true";
            session.Metadata["surface"] = AuthoringSurface.RequiredSetup.ToString();
            AuthoringGraphNode input = Contract("contract:input", "Input", "setup.input", "Input", 20);
            input.Metadata["selectable"] = "true";
            input.Metadata["surface"] = AuthoringSurface.Profile.ToString();
            input.Metadata["prerequisiteStableIds"] = "setup.session";
            AuthoringGraphNode combat = Contract("contract:combat", "Combat", "proof.combat", "Combat", 30);
            combat.Metadata["selectable"] = "true";
            combat.Metadata["surface"] = AuthoringSurface.RuntimeComponent.ToString();
            combat.Metadata["capabilityPath"] = "Combat/Proof";
            combat.Metadata["prerequisiteStableIds"] = "setup.input";
            combat.Metadata["proofTarget"] = "Combat proof";
            graph.Nodes.Add(session);
            graph.Nodes.Add(input);
            graph.Nodes.Add(combat);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);

            Assert.That(intent.Rows, Has.Count.EqualTo(1));
            Assert.That(intent.Rows[0].ContractId, Is.EqualTo("contract:combat"));
            Assert.That(intent.Rows[0].OrganizationPattern, Is.EqualTo("Proof target"));
            Assert.That(intent.Rows[0].DependencyCount, Is.EqualTo(1));
        }

        [Test]
        public void BuildIntent_UsesRouteTerminalWhenNoExplicitGoalExists()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode session = Contract("contract:session", "Session", "setup.session", "Core", 10);
            session.Metadata["selectable"] = "true";
            AuthoringGraphNode input = Contract("contract:input", "Input", "setup.input", "Input", 20);
            input.Metadata["selectable"] = "true";
            input.Metadata["prerequisiteStableIds"] = "setup.session";
            AuthoringGraphNode assembled = Contract("contract:assembled", "Assembled Setup", "setup.assembled", "Proof", 30);
            assembled.Metadata["selectable"] = "true";
            assembled.Metadata["prerequisiteStableIds"] = "setup.input";
            graph.Nodes.Add(session);
            graph.Nodes.Add(input);
            graph.Nodes.Add(assembled);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);

            Assert.That(intent.Rows, Has.Count.EqualTo(1));
            Assert.That(intent.Rows[0].ContractId, Is.EqualTo("contract:assembled"));
            Assert.That(intent.Rows[0].OrganizationPattern, Is.EqualTo("Route terminal"));
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
        public void ReflectiveRuntimeValidationObserver_NormalizesDefaultRuntimeValidationMethod()
        {
            RuntimeValidationComponent component = new RuntimeValidationComponent();
            List<AuthoringIssue> issues = new List<AuthoringIssue>();

            ReflectiveRuntimeValidationObserver.AddValidationIssues(component, null, issues);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].IssueCode, Is.EqualTo("Runtime.Health.Missing"));
            Assert.That(issues[0].Message, Is.EqualTo("Assign health profile."));
            Assert.That(issues[0].Severity, Is.EqualTo(AuthoringIssueSeverity.Required));
            Assert.That(issues[0].FieldPath, Is.EqualTo("healthProfile"));
            Assert.That(issues[0].TargetLabel, Is.EqualTo("Enemy"));
            Assert.That(issues[0].NativeAction, Is.EqualTo("Assign a health profile."));
            Assert.That(issues[0].SuccessCheck, Is.EqualTo("Health profile is assigned."));
            Assert.That(issues[0].ActionKind, Is.EqualTo(AuthoringActionKind.AssignField));
            Assert.That(issues[0].OwnerStableId, Is.EqualTo("proof.enemy-health"));
            Assert.That(issues[0].RelatedStableIds, Is.EqualTo(new[] { "setup.enemy" }));
        }

        [Test]
        public void ReflectiveRuntimeValidationObserver_UsesConfiguredMethodNames()
        {
            UnityCodebaseScanRequest request = new UnityCodebaseScanRequest("Assets");
            request.RuntimeValidationMethodNames.Clear();
            request.RuntimeValidationMethodNames.Add("CollectSetupIssues");

            IReadOnlyList<MethodInfo> methods = ReflectiveRuntimeValidationObserver.FindValidationMethods(typeof(ConfiguredValidationComponent), request.RuntimeValidationMethodNames);

            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].Name, Is.EqualTo("CollectSetupIssues"));
        }

        [Test]
        public void DependencyGraphProjection_RecordsReflectiveValidationProviderEvidence()
        {
            UnityTypeObservation observation = new UnityTypeObservation(typeof(RuntimeValidationComponent), "Assets/RuntimeValidationComponent.cs");
            observation.HasRuntimeValidationMethod = true;
            observation.RuntimeValidationMethods.Add("GetRuntimeValidationIssues");

            AuthoringGraph graph = DependencyGraphProjection.BuildTypeObservationGraph(new[] { observation });

            AuthoringGraphNode validator = FindFirstNode(graph, AuthoringGraphNodeKind.Validator);
            Assert.That(validator, Is.Not.Null);
            Assert.That(validator.Metadata["validationSource"], Is.EqualTo("ReflectiveRuntimeValidation"));
            Assert.That(validator.Metadata["methods"], Is.EqualTo("GetRuntimeValidationIssues"));
        }

        [Test]
        public void ObservedValidationEvidence_FlowsThroughGuideOverviewFactsAndHygiene()
        {
            UnityCodebaseScanResult scanResult = new UnityCodebaseScanResult();
            UnityTypeObservation typeObservation = new UnityTypeObservation(typeof(RuntimeValidationComponent), "Assets/RuntimeValidationComponent.cs");
            typeObservation.HasRuntimeValidationMethod = true;
            typeObservation.RuntimeValidationMethods.Add("GetRuntimeValidationIssues");
            typeObservation.Contracts.Add(new ResolvedAuthoringContract("proof.enemy-health", typeof(RuntimeValidationComponent).FullName)
            {
                DisplayName = "Enemy Health Proof",
                Surface = AuthoringSurface.Goal,
                ProofTarget = "Enemy health proof",
                Selectable = true
            });
            scanResult.Types.Add(typeObservation);

            UnityObjectObservation sceneObject = new UnityObjectObservation("scene:Test:Enemy", "Enemy", "Assets/Test.unity", "GameObject");
            ReflectiveRuntimeValidationObserver.AddValidationIssues(new RuntimeValidationComponent(), null, sceneObject.Issues);
            ReflectiveRuntimeValidationObserver.AddValidationIssues(new IncompleteRuntimeValidationComponent(), null, sceneObject.Issues);
            scanResult.SceneObjects.Add(sceneObject);

            AuthoringGraph graph = DependencyGraphProjection.Build(scanResult);
            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:proof.enemy-health");
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);
            FactsProjection facts = AuthoringProjectionBuilder.BuildFacts(graph);
            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.OwnerId.Contains("Runtime.Health.Missing")));
            Assert.That(overview.NextAction, Is.EqualTo("Assign a health profile."));
            Assert.That(facts.IssueCount, Is.EqualTo(2));
            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row => row.IssueCode == "Validation.Metadata.Incomplete"));
        }

        [Test]
        public void BuildOverview_ReportsIssueAsNextAction()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["stableId"] = "test";
            graph.Nodes.Add(contract);
            AuthoringGraphNode issue = new AuthoringGraphNode("issue:test", "Fix Me", AuthoringGraphNodeKind.Issue);
            issue.Metadata["issueCode"] = "Example.Issue";
            issue.Metadata["nativeAction"] = "Inspect the object.";
            issue.Metadata["actionKind"] = AuthoringActionKind.InspectObject.ToString();
            graph.Nodes.Add(issue);
            graph.Edges.Add(new AuthoringGraphEdge("contract:test", "issue:test", AuthoringGraphEdgeKind.ValidatorReports));

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:test");
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

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
        public void BuildGuide_NoSelectedIntentReportsNoIntentSelected()
        {
            AuthoringGraph graph = new AuthoringGraph();
            graph.Nodes.Add(new AuthoringGraphNode("contract:setup", "Setup", AuthoringGraphNodeKind.Contract));

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph);
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

            Assert.That(guide.SelectedDisplayName, Is.EqualTo("No intent selected"));
            Assert.That(guide.ProofReady, Is.False);
            Assert.That(overview.SelectedIntent, Is.EqualTo("No intent selected"));
            Assert.That(overview.Readiness, Is.EqualTo("No intent selected"));
            Assert.That(overview.Reason, Is.EqualTo("No intent selected"));
        }

        [Test]
        public void BuildGuide_OrdersSelectedContractDependencyClosure()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode session = Contract("contract:session", "Session", "setup.session", "Core", 10);
            session.Metadata["setupSteps"] = "Create session asset.";
            AuthoringGraphNode input = Contract("contract:input", "Input", "setup.input", "Input", 20);
            input.Metadata["prerequisiteStableIds"] = "setup.session";
            input.Metadata["setupSteps"] = "Assign input profile.";
            AuthoringGraphNode combat = Contract("contract:combat", "Combat", "proof.combat", "Combat", 30);
            combat.Metadata["prerequisiteStableIds"] = "setup.input";
            combat.Metadata["setupSteps"] = "Assign combat profile.";
            combat.Metadata["proofTarget"] = "Combat proof";
            combat.Metadata["successChecks"] = "Enter Play Mode.";
            graph.Nodes.Add(combat);
            graph.Nodes.Add(input);
            graph.Nodes.Add(session);

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:combat");

            Assert.That(guide.ProofTarget, Is.EqualTo("Combat proof"));
            Assert.That(guide.Rows[0].OwnerId, Is.EqualTo("contract:session"));
            Assert.That(guide.Rows[0].RouteStage, Is.EqualTo("Core"));
            Assert.That(guide.Rows[1].OwnerId, Is.EqualTo("contract:input"));
            Assert.That(guide.Rows[2].OwnerId, Is.EqualTo("contract:combat"));
            Assert.That(guide.Rows[3].Role, Is.EqualTo("ProofCheck"));
        }

        [Test]
        public void BuildGuide_IncludesValidatorIssuesForSelectedDependencyClosureOnly()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode selected = Contract("contract:combat", "Combat", "proof.combat", "Combat", 20);
            selected.Metadata["prerequisiteStableIds"] = "setup.input";
            AuthoringGraphNode dependency = Contract("contract:input", "Input", "setup.input", "Input", 10);
            AuthoringGraphNode unrelated = Contract("contract:rpg", "RPG", "feature.rpg", "RPG", 10);
            graph.Nodes.Add(selected);
            graph.Nodes.Add(dependency);
            graph.Nodes.Add(unrelated);

            AuthoringGraphNode dependencyIssue = new AuthoringGraphNode("issue:input", "Assign input asset", AuthoringGraphNodeKind.Issue);
            dependencyIssue.Metadata["issueCode"] = "Input.Missing";
            dependencyIssue.Metadata["nativeAction"] = "Assign the input asset.";
            dependencyIssue.Metadata["actionKind"] = AuthoringActionKind.AssignField.ToString();
            graph.Nodes.Add(dependencyIssue);
            graph.Edges.Add(new AuthoringGraphEdge("contract:input", "issue:input", AuthoringGraphEdgeKind.ValidatorReports));

            AuthoringGraphNode unrelatedIssue = new AuthoringGraphNode("issue:rpg", "Assign RPG asset", AuthoringGraphNodeKind.Issue);
            unrelatedIssue.Metadata["issueCode"] = "Rpg.Missing";
            graph.Nodes.Add(unrelatedIssue);
            graph.Edges.Add(new AuthoringGraphEdge("contract:rpg", "issue:rpg", AuthoringGraphEdgeKind.ValidatorReports));

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:combat");
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.OwnerId == "issue:input"));
            Assert.That(guide.Rows, Has.None.Matches<GuideRow>(row => row.OwnerId == "issue:rpg"));
            Assert.That(overview.NextAction, Is.EqualTo("Assign the input asset."));
            Assert.That(overview.Reason, Is.EqualTo("Input.Missing"));
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
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:test", "Test Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["stableId"] = "test";
            graph.Nodes.Add(contract);
            AuthoringGraphNode issue = new AuthoringGraphNode("issue:test", "Assign Thing", AuthoringGraphNodeKind.Issue);
            issue.Metadata["issueCode"] = "Example.Assign";
            issue.Metadata["nativeAction"] = "Assign a field.";
            issue.Metadata["successCheck"] = "Field is assigned.";
            issue.Metadata["actionKind"] = AuthoringActionKind.AssignField.ToString();
            graph.Nodes.Add(issue);
            graph.Edges.Add(new AuthoringGraphEdge("contract:test", "issue:test", AuthoringGraphEdgeKind.ValidatorReports));

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:test");

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

        private static AuthoringGraphNode FindFirstNode(AuthoringGraph graph, AuthoringGraphNodeKind kind)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i].Kind == kind)
                    return graph.Nodes[i];
            }

            return null;
        }

        private static AuthoringGraphNode Contract(string id, string label, string stableId, string routeStage, int routeOrder)
        {
            AuthoringGraphNode node = new AuthoringGraphNode(id, label, AuthoringGraphNodeKind.Contract);
            node.Metadata["stableId"] = stableId;
            node.Metadata["routeStage"] = routeStage;
            node.Metadata["routeOrder"] = routeOrder.ToString();
            node.Metadata["setupDomain"] = routeStage;
            node.Metadata["actionKind"] = AuthoringActionKind.InspectObject.ToString();
            return node;
        }

        private sealed class RuntimeValidationComponent
        {
            public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
            {
                yield return new RuntimeValidationIssue();
            }
        }

        private sealed class ConfiguredValidationComponent
        {
            public IEnumerable<RuntimeValidationIssue> CollectSetupIssues()
            {
                yield return new RuntimeValidationIssue();
            }

            public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
            {
                yield return new RuntimeValidationIssue();
            }
        }

        private sealed class IncompleteRuntimeValidationComponent
        {
            public IEnumerable<IncompleteRuntimeValidationIssue> GetRuntimeValidationIssues()
            {
                yield return new IncompleteRuntimeValidationIssue();
            }
        }

        private sealed class RuntimeValidationIssue
        {
            public string IssueCode { get; } = "Runtime.Health.Missing";
            public string Message { get; } = "Assign health profile.";
            public string Severity { get; } = "Required";
            public string FieldPath { get; } = "healthProfile";
            public string TargetLabel { get; } = "Enemy";
            public string NativeAction { get; } = "Assign a health profile.";
            public string SuccessCheck { get; } = "Health profile is assigned.";
            public string ActionKind { get; } = "AssignField";
            public string OwnerStableId { get; } = "proof.enemy-health";
            public string[] RelatedStableIds { get; } = { "setup.enemy" };
        }

        private sealed class IncompleteRuntimeValidationIssue
        {
            public string IssueCode { get; } = "Runtime.Metadata.Incomplete";
            public string Message { get; } = "Validation metadata is incomplete.";
            public string NativeAction { get; } = "Inspect validation metadata.";
        }
    }
}

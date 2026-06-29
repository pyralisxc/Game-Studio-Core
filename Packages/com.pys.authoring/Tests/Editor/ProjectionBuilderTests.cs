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
        public void BuildIntent_AddsBuiltInUnitySetupGuidesWhenNoTargetContractsExist()
        {
            AuthoringGraph graph = DependencyGraphProjection.Build(new UnityCodebaseScanResult());
            graph.Nodes.Add(new AuthoringGraphNode("asset:scene", "Sample Scene", AuthoringGraphNodeKind.Asset));

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);

            Assert.That(intent.Rows, Has.Some.Matches<IntentRow>(row =>
                row.ContractId == "contract:unity.setup.cinemachine-follow"
                && row.IntentSource == "BuiltInUnitySetup"
                && row.Priority == 100
                && row.CapabilityPath == "Unity/Cinemachine/Follow Camera"));
        }

        [Test]
        public void BuildFacts_DisplaysBuiltInUnitySetupGuidesAsFacts()
        {
            AuthoringGraph graph = DependencyGraphProjection.Build(new UnityCodebaseScanResult());

            FactsProjection facts = AuthoringProjectionBuilder.BuildFacts(graph);

            Assert.That(facts.Rows, Has.Some.Matches<FactRow>(row =>
                row.Label == "Set Up Timeline Sequence"
                && row.Detail.Contains("UnitySetupGuide")
                && row.Confidence == "BuiltInUnitySetup"));
        }

        [Test]
        public void BuildIntent_DoesNotMixBuiltInUnitySetupWithTargetContractsUnlessRequested()
        {
            AuthoringGraph graph = DependencyGraphProjection.Build(new UnityCodebaseScanResult());
            AuthoringGraphNode contract = Contract("contract:combat", "Combat", "proof.combat", "Combat", 10);
            contract.Metadata["selectable"] = "true";
            contract.Metadata["surface"] = AuthoringSurface.Goal.ToString();
            graph.Nodes.Add(contract);

            IntentProjection defaultIntent = AuthoringProjectionBuilder.BuildIntent(graph);
            IntentProjection requestedIntent = AuthoringProjectionBuilder.BuildIntent(graph, string.Empty, true);

            Assert.That(defaultIntent.Rows, Has.None.Matches<IntentRow>(row => row.IntentSource == "BuiltInUnitySetup"));
            Assert.That(requestedIntent.Rows[0].IntentSource, Is.EqualTo("TargetContract"));
            Assert.That(requestedIntent.Rows, Has.Some.Matches<IntentRow>(row => row.IntentSource == "BuiltInUnitySetup" && row.Priority == 100));
        }

        [Test]
        public void BuildGuide_CreatesNativeUnitySetupGuidePathFromGraphEvidence()
        {
            AuthoringGraph graph = DependencyGraphProjection.Build(new UnityCodebaseScanResult());

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:unity.setup.timeline", true);
            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);

            Assert.That(guide.SelectedDisplayName, Is.EqualTo("Set Up Timeline Sequence"));
            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.Role == "UnitySetupStep" && row.ActionKind == AuthoringActionKind.OpenWindow.ToString()));
            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.Role == "CompletionSignal" && row.BlocksProof == false));
            Assert.That(overview.NextAction, Does.Contain("Playable Director"));
            Assert.That(overview.NextActions, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(overview.NextActions[0].NativeAction, Does.Contain("Playable Director"));
        }

        [Test]
        public void BuildOverview_ExportsNextThreeBlockingGuideRows()
        {
            AuthoringGraph graph = new AuthoringGraph();
            GuideProjection guide = new GuideProjection { SelectedDisplayName = "Route" };
            guide.Rows.Add(new GuideRow { Title = "Step A", Detail = "A", NativeAction = "Do A", ActionKind = AuthoringActionKind.InspectObject.ToString(), ActionLabel = "Inspect Object", Role = "SetupStep", OwnerId = "a", BlocksProof = true });
            guide.Rows.Add(new GuideRow { Title = "Step B", Detail = "B", NativeAction = "Do B", ActionKind = AuthoringActionKind.AssignField.ToString(), ActionLabel = "Assign Field", Role = "Issue", OwnerId = "b", BlocksProof = true });
            guide.Rows.Add(new GuideRow { Title = "Step C", Detail = "C", NativeAction = "Do C", ActionKind = AuthoringActionKind.OpenWindow.ToString(), ActionLabel = "Open Window", Role = "SetupStep", OwnerId = "c", BlocksProof = true });
            guide.Rows.Add(new GuideRow { Title = "Step D", Detail = "D", NativeAction = "Do D", Role = "SetupStep", OwnerId = "d", BlocksProof = true });

            OverviewProjection overview = AuthoringProjectionBuilder.BuildOverview(graph, guide);
            string json = ProjectionJsonExporter.ToOverviewJson(overview, "Assets");

            Assert.That(overview.NextActions, Has.Count.EqualTo(3));
            Assert.That(overview.NextActions[0].NativeAction, Is.EqualTo("Do A"));
            Assert.That(overview.NextActions[2].ActionLabel, Is.EqualTo("Open Window"));
            Assert.That(json, Does.Contain("\"nextActions\""));
            Assert.That(json, Does.Contain("\"nativeAction\": \"Do A\""));
            Assert.That(json, Does.Not.Contain("\"nativeAction\": \"Do D\""));
        }

        [Test]
        public void BuildIntent_ProjectsCompositionAndReadinessMetadata()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = Contract("contract:actor", "Actor Route", "intent.actor", "Actor", 10);
            contract.Metadata["selectable"] = "true";
            contract.Metadata["surface"] = AuthoringSurface.Goal.ToString();
            contract.Metadata["successDescription"] = "Set up an actor route.";
            contract.Metadata["readinessHint"] = "Actor can enter Play Mode.";
            contract.Metadata["expectedEvidence"] = "scene.object:Actor\ncomponent:Controller";
            contract.Metadata["completionSignals"] = "Play Mode enters.";
            contract.Metadata["validationOwnerStableId"] = "validation.actor";
            contract.Metadata["intentToggles"] = "Combat\nCamera";
            contract.Metadata["intentLanes"] = "Sprite2D\nRigged3D";
            contract.Metadata["compatibleStableIds"] = "feature.inventory";
            contract.Metadata["supportingStableIds"] = "setup.camera";
            contract.Metadata["hoverExplanations"] = "Camera adds follow framing.";
            graph.Nodes.Add(contract);

            IntentProjection intent = AuthoringProjectionBuilder.BuildIntent(graph);
            string json = ProjectionJsonExporter.ToIntentJson(intent, "Assets");

            Assert.That(intent.Rows, Has.Count.EqualTo(1));
            Assert.That(intent.Rows[0].SuccessDescription, Is.EqualTo("Set up an actor route."));
            Assert.That(intent.Rows[0].ReadinessHint, Is.EqualTo("Actor can enter Play Mode."));
            Assert.That(intent.Rows[0].ExpectedEvidence, Does.Contain("component:Controller"));
            Assert.That(intent.Rows[0].CompletionSignals, Is.EqualTo("Play Mode enters."));
            Assert.That(intent.Rows[0].IntentToggles, Does.Contain("Combat"));
            Assert.That(intent.Rows[0].IntentLanes, Does.Contain("Sprite2D"));
            Assert.That(intent.Rows[0].CompatibleStableIds, Is.EqualTo("feature.inventory"));
            Assert.That(intent.Rows[0].SupportingStableIds, Is.EqualTo("setup.camera"));
            Assert.That(intent.Rows[0].HoverExplanations, Is.EqualTo("Camera adds follow framing."));
            Assert.That(json, Does.Contain("\"intentToggles\": \"Combat\\nCamera\""));
            Assert.That(json, Does.Contain("\"successDescription\": \"Set up an actor route.\""));
        }

        [Test]
        public void BuildGuide_UsesSuccessDescriptionBeforeFallbackProofTarget()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = Contract("contract:actor", "Actor Route", "intent.actor", "Actor", 10);
            contract.Metadata["successDescription"] = "Set up an actor route.";
            contract.Metadata["proofTarget"] = "Fallback proof wording";
            contract.Metadata["expectedEvidence"] = "Actor exists.";
            contract.Metadata["completionSignals"] = "Validation clears.";
            graph.Nodes.Add(contract);

            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:actor");

            Assert.That(guide.ProofTarget, Is.EqualTo("Set up an actor route."));
            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.Role == "ExpectedEvidence" && row.Detail == "Actor exists."));
            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row => row.Role == "CompletionSignal" && row.Detail == "Validation clears."));
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
            Assert.That(map.Rows[0].CanPing, Is.True);
            Assert.That(map.Rows[0].NavigationKind, Is.EqualTo("Asset"));
            Assert.That(map.Rows[0].NavigationLabel, Is.EqualTo("Ping Asset"));
        }

        [Test]
        public void BuildMap_ProjectsSceneObjectSelectionAction()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode sceneObject = new AuthoringGraphNode("scene:Assets/Test.unity:Root/Child", "Child", AuthoringGraphNodeKind.SceneObject);
            sceneObject.Metadata["sourcePath"] = "Assets/Test.unity";
            graph.Nodes.Add(sceneObject);

            MapProjection map = AuthoringProjectionBuilder.BuildMap(graph);

            Assert.That(map.Rows, Has.Count.EqualTo(1));
            Assert.That(map.Rows[0].CanSelect, Is.True);
            Assert.That(map.Rows[0].NavigationKind, Is.EqualTo("SceneObject"));
            Assert.That(map.Rows[0].NavigationLabel, Is.EqualTo("Select in Hierarchy"));
        }

        [Test]
        public void BuiltInUnitySetupContributor_AddsMissingPackageEvidence()
        {
            AuthoringGraph graph = new AuthoringGraph();
            System.Type contributorType = System.Type.GetType("Pys.Authoring.Editor.UnitySetup.BuiltInUnitySetupGraphContributor, Pys.Authoring.Editor");
            Assert.That(contributorType, Is.Not.Null);

            MethodInfo addTo = contributorType.GetMethod("AddTo", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(AuthoringGraph), typeof(Pys.Authoring.Editor.Vocabulary.AuthoringVocabularyDictionary), typeof(System.Func<string, bool>) }, null);
            Assert.That(addTo, Is.Not.Null);

            addTo.Invoke(null, new object[] { graph, null, new System.Func<string, bool>(packageName => packageName != "com.unity.timeline") });

            Assert.That(graph.Nodes, Has.Some.Matches<AuthoringGraphNode>(node =>
                node.Id == "contract:unity.setup.timeline"
                && node.Metadata.TryGetValue("availability", out string availability)
                && availability == "MissingPackage"
                && node.Metadata.TryGetValue("packageAvailability", out string packageAvailability)
                && packageAvailability.Contains("com.unity.timeline: Missing")));
            Assert.That(graph.Nodes, Has.Some.Matches<AuthoringGraphNode>(node =>
                node.Kind == AuthoringGraphNodeKind.Issue
                && node.Metadata.TryGetValue("issueCode", out string issueCode)
                && issueCode == "UnitySetup.Package.Missing"
                && node.Metadata.TryGetValue("ownerStableId", out string ownerStableId)
                && ownerStableId == "unity.setup.timeline"));
        }

        [Test]
        public void BuiltInUnitySetup_ReadinessEvidenceUsesObservedSceneComponentsAndAssets()
        {
            UnityCodebaseScanResult scanResult = new UnityCodebaseScanResult();
            UnityObjectObservation sceneCamera = new UnityObjectObservation("scene:Assets/Test.unity:Camera", "Camera", "Assets/Test.unity", "GameObject");
            sceneCamera.Components.Add("UnityEngine.Camera");
            sceneCamera.Components.Add("UnityEngine.AudioListener");
            sceneCamera.ComponentFields.Add("UnityEngine.Camera.enabled=true");
            sceneCamera.ComponentFields.Add("UnityEngine.AudioListener.enabled=true");
            scanResult.SceneObjects.Add(sceneCamera);
            scanResult.Assets.Add(new UnityAssetObservation("asset:Assets/Walk.anim", "Walk", "Assets/Walk.anim", "AnimationClip"));

            AuthoringGraph graph = DependencyGraphProjection.Build(scanResult);

            AuthoringGraphNode cameraGuide = FindNode(graph, "contract:unity.setup.camera");
            Assert.That(cameraGuide, Is.Not.Null);
            Assert.That(cameraGuide.Metadata["readinessState"], Is.EqualTo("Observed"));
            Assert.That(cameraGuide.Metadata["observedComponents"], Does.Contain("Camera"));
            Assert.That(cameraGuide.Metadata["observedComponents"], Does.Contain("AudioListener"));
            Assert.That(cameraGuide.Metadata["observedFields"], Does.Contain("Camera.enabled=true"));

            AuthoringGraphNode animationGuide = FindNode(graph, "contract:unity.setup.animation-clip");
            Assert.That(animationGuide, Is.Not.Null);
            Assert.That(animationGuide.Metadata["readinessState"], Is.EqualTo("Partial"));
            Assert.That(animationGuide.Metadata["observedAssets"], Does.Contain("AnimationClip"));
            Assert.That(animationGuide.Metadata["missingComponents"], Does.Contain("Animator"));
        }

        [Test]
        public void BuildGuide_ProjectsBuiltInUnityReadinessEvidence()
        {
            UnityCodebaseScanResult scanResult = new UnityCodebaseScanResult();
            UnityObjectObservation sceneCamera = new UnityObjectObservation("scene:Assets/Test.unity:Camera", "Camera", "Assets/Test.unity", "GameObject");
            sceneCamera.Components.Add("UnityEngine.Camera");
            sceneCamera.Components.Add("UnityEngine.AudioListener");
            sceneCamera.ComponentFields.Add("UnityEngine.Camera.enabled=true");
            sceneCamera.ComponentFields.Add("UnityEngine.AudioListener.enabled=true");
            scanResult.SceneObjects.Add(sceneCamera);

            AuthoringGraph graph = DependencyGraphProjection.Build(scanResult);
            GuideProjection guide = AuthoringProjectionBuilder.BuildGuide(graph, "contract:unity.setup.camera", true);
            string json = ProjectionJsonExporter.ToGuideJson(guide, "Assets");

            Assert.That(guide.Rows, Has.Some.Matches<GuideRow>(row =>
                row.Role == "UnityReadinessEvidence"
                && row.Detail.Contains("Observed")
                && row.BlocksProof == false));
            Assert.That(json, Does.Contain("\"role\": \"UnityReadinessEvidence\""));
            Assert.That(json, Does.Contain("Observed components"));
            Assert.That(json, Does.Contain("Observed fields"));
        }

        [Test]
        public void BuiltInUnitySetup_ReadinessEvidenceReportsMissingFieldAssignments()
        {
            UnityCodebaseScanResult scanResult = new UnityCodebaseScanResult();
            UnityObjectObservation audioObject = new UnityObjectObservation("scene:Assets/Test.unity:Audio", "Audio", "Assets/Test.unity", "GameObject");
            audioObject.Components.Add("UnityEngine.AudioSource");
            audioObject.Components.Add("UnityEngine.AudioListener");
            audioObject.ComponentFields.Add("UnityEngine.AudioSource.clip=Missing");
            audioObject.ComponentFields.Add("UnityEngine.AudioSource.enabled=true");
            audioObject.ComponentFields.Add("UnityEngine.AudioListener.enabled=true");
            scanResult.SceneObjects.Add(audioObject);

            AuthoringGraph graph = DependencyGraphProjection.Build(scanResult);
            AuthoringGraphNode audioGuide = FindNode(graph, "contract:unity.setup.audio-source");

            Assert.That(audioGuide, Is.Not.Null);
            Assert.That(audioGuide.Metadata["readinessState"], Is.EqualTo("Partial"));
            Assert.That(audioGuide.Metadata["observedFields"], Does.Contain("AudioSource.enabled=true"));
            Assert.That(audioGuide.Metadata["missingFields"], Does.Contain("AudioSource.clip=Assigned"));
            Assert.That(audioGuide.Metadata["readinessEvidenceSummary"], Does.Contain("Missing fields"));
            Assert.That(audioGuide.Metadata["readinessEvidenceSummary"], Does.Contain("Audio Clip Assigned: Assigned"));
            Assert.That(audioGuide.Metadata["readinessEvidenceSummary"], Does.Contain("Audio Source Enabled: Enabled"));
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

            Assert.That(hygiene.Lenses.Count, Is.EqualTo(9));
            Assert.That(FindLens(hygiene, HygieneLensKind.Overview).Rows.Count, Is.EqualTo(hygiene.Rows.Count));
            Assert.That(FindLens(hygiene, HygieneLensKind.Contracts).Rows.Count, Is.EqualTo(1));
            Assert.That(FindLens(hygiene, HygieneLensKind.Dependencies).Rows.Count, Is.EqualTo(2));
            Assert.That(FindLens(hygiene, HygieneLensKind.ValidationEvidence).Rows.Count, Is.EqualTo(1));
            Assert.That(FindLens(hygiene, HygieneLensKind.VisualDependencyGraph).Rows.Count, Is.GreaterThanOrEqualTo(2));
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
        public void BuildHygiene_ReportsGoalContractsMissingReadinessHints()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:goal", "Goal Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["stableId"] = "goal";
            contract.Metadata["surface"] = AuthoringSurface.Goal.ToString();
            graph.Nodes.Add(contract);

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.Lens == HygieneLensKind.Contracts
                && row.IssueCode == "Contract.ReadinessHints.Missing"
                && row.Recommendation.Contains("SuccessDescription")));
        }

        [Test]
        public void BuildHygiene_ReportsValidationOwnerMismatch()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:known", "Known Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["stableId"] = "known";
            graph.Nodes.Add(contract);

            AuthoringGraphNode issue = new AuthoringGraphNode("issue:unknown", "Unknown Owner", AuthoringGraphNodeKind.Issue);
            issue.Metadata["issueCode"] = "Unknown.Owner";
            issue.Metadata["nativeAction"] = "Inspect owner.";
            issue.Metadata["successCheck"] = "Owner is valid.";
            issue.Metadata["ownerStableId"] = "missing.owner";
            graph.Nodes.Add(issue);

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.Lens == HygieneLensKind.ValidationEvidence
                && row.IssueCode == "Validation.Owner.Unmatched"
                && row.Detail == "missing.owner"));
        }

        [Test]
        public void BuildHygiene_ReportsUnobservedExpectedEvidence()
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringGraphNode contract = new AuthoringGraphNode("contract:goal", "Goal Contract", AuthoringGraphNodeKind.Contract);
            contract.Metadata["stableId"] = "goal";
            contract.Metadata["expectedEvidence"] = "ReadyMarker";
            graph.Nodes.Add(contract);

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.Lens == HygieneLensKind.Ownership
                && row.IssueCode == "Honesty.ExpectedEvidence.Unobserved"
                && row.Detail == "ReadyMarker"));
        }

        [Test]
        public void BuildHygiene_CreatesVisualDependencyGraphRows()
        {
            AuthoringGraph graph = new AuthoringGraph();
            graph.Edges.Add(new AuthoringGraphEdge("type:a", "field:a.profile", AuthoringGraphEdgeKind.SerializedField));
            graph.Edges.Add(new AuthoringGraphEdge("type:b", "field:b.profile", AuthoringGraphEdgeKind.SerializedField));
            graph.Edges.Add(new AuthoringGraphEdge("contract:a", "issue:a", AuthoringGraphEdgeKind.ValidatorReports));

            HygieneProjection hygiene = HygieneProjectionBuilder.Build(graph);

            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.Lens == HygieneLensKind.VisualDependencyGraph
                && row.IssueCode == "Graph.EdgeKind.Group"
                && row.OwnerId == "graph:SerializedField"
                && row.Detail.Contains("2 edge")));
            Assert.That(hygiene.Rows, Has.Some.Matches<HygieneRow>(row =>
                row.Lens == HygieneLensKind.VisualDependencyGraph
                && row.IssueCode == "Graph.NodeKind.Group"
                && row.OwnerId == "graph:node:Type"
                && row.Detail.Contains("2 node")));
        }

        [Test]
        public void BuildHygiene_EmptyInputStillCreatesLensPackets()
        {
            HygieneProjection hygiene = HygieneProjectionBuilder.Build(null);

            Assert.That(hygiene.Rows.Count, Is.EqualTo(0));
            Assert.That(hygiene.Lenses.Count, Is.EqualTo(9));
            Assert.That(FindLens(hygiene, HygieneLensKind.Overview).Rows.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExportJson_MirrorsProjectionPackets()
        {
            FactsProjection facts = new FactsProjection { AssemblyCount = 2, IssueCount = 3 };
            MapProjection map = new MapProjection();
            map.Rows.Add(new MapRow { Id = "object:a", Label = "Object A", Kind = "SceneObject", SourcePath = "Assets/Test.unity", ComponentCount = 4, IssueCount = 1, CanSelect = true, NavigationKind = "SceneObject", NavigationLabel = "Select in Hierarchy" });
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
            Assert.That(mapJson, Does.Contain("\"canSelect\": true"));
            Assert.That(mapJson, Does.Contain("\"navigationLabel\": \"Select in Hierarchy\""));
            Assert.That(overviewJson, Does.Contain("\"nextAction\": \"Inspect A\""));
            Assert.That(guideJson, Does.Contain("\"successCheck\": \"Check A\""));
            Assert.That(hygieneJson, Does.Contain("\"lenses\""));
            Assert.That(hygieneJson, Does.Contain("\"kind\": \"Contracts\""));
            Assert.That(hygieneJson, Does.Contain("\"issueCode\": \"Contract.Metadata.Missing\""));
            Assert.That(hygieneJson, Does.Contain("\"sourceKind\": \"Contract\""));
            Assert.That(hygieneJson, Does.Contain("\"claim\": \"Contract should describe enough metadata for projections.\""));
            Assert.That(hygieneJson, Does.Contain("\"recommendation\": \"Complete the contract metadata on the declaring type.\""));

            IntentProjection unityIntent = AuthoringProjectionBuilder.BuildIntent(DependencyGraphProjection.Build(new UnityCodebaseScanResult()), string.Empty, true);
            string unityIntentJson = ProjectionJsonExporter.ToIntentJson(unityIntent, "Assets");
            Assert.That(unityIntentJson, Does.Contain("\"intentSource\": \"BuiltInUnitySetup\""));
            Assert.That(unityIntentJson, Does.Contain("\"priority\": 100"));
        }

        [Test]
        public void ExportIntentJson_MirrorsIntentProjection()
        {
            IntentProjection intent = new IntentProjection
            {
                SelectableCount = 1,
                SelectedContractId = "contract:a",
                SelectedDisplayName = "Contract A",
                SelectedFeatureToggles = "Combat\nCamera",
                SelectedLane = "Sprite2D",
                SelectedCompositionSummary = "Selected intent composition uses lane: Sprite2D; features: Combat, Camera."
            };
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
            Assert.That(json, Does.Contain("\"selectedFeatureToggles\": \"Combat\\nCamera\""));
            Assert.That(json, Does.Contain("\"selectedLane\": \"Sprite2D\""));
            Assert.That(json, Does.Contain("\"selectedCompositionSummary\": \"Selected intent composition uses lane: Sprite2D; features: Combat, Camera.\""));
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

        private static AuthoringGraphNode FindNode(AuthoringGraph graph, string id)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i].Id == id)
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

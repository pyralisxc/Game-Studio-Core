using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringToolkitTabRenderer
    {
        public static void DrawHygiene(
            VisualElement root,
            Object activeSetup,
            PyralisAuthoringIntentSelection intentSelection)
        {
            if (root == null)
                return;

            root.Clear();

            var hygieneView = new VisualElement();
            hygieneView.AddToClassList("hygiene-view");

            var title = new Label("PROJECT HYGIENE AUDIT");
            title.AddToClassList("section-title");
            hygieneView.Add(title);

            var list = new VisualElement { name = "globalHygieneList" };
            hygieneView.Add(list);

            root.Add(hygieneView);
            UpdateHygiene(list, activeSetup, intentSelection);
        }

        public static void DrawMap(
            VisualElement root,
            Object activeSetup,
            PyralisAuthoringRouteReport report)
        {
            if (root == null)
                return;

            root.Clear();

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(activeSetup, bootstrap);
            GameModeDefinition mode = PyralisAuthoringSetupContextResolver.GetSelectedMode(activeSetup, session);
            GameSetupProfile setupProfile = PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(activeSetup, mode);

            var mapView = new VisualElement();
            mapView.AddToClassList("map-view");

            var title = new Label("SETUP CHAIN MAP");
            title.AddToClassList("section-title");
            mapView.Add(title);

            var info = new Label("Use this map to understand how the active setup is connected. Select an object to see its details in the Inspector.");
            info.style.opacity = 0.7f;
            info.style.marginBottom = 10;
            info.style.whiteSpace = WhiteSpace.Normal;
            mapView.Add(info);

            var chainContainer = new VisualElement { name = "chainContainer" };
            chainContainer.AddToClassList("setup-chain-container");
            mapView.Add(chainContainer);

            AddChainLink(chainContainer, "BOOTSTRAP", bootstrap, "The scene entry point. Initializes services and starts the session.", "Select a GameplaySessionBootstrap or Gameplay Root object.");
            AddChainLink(chainContainer, "SESSION", session, "Defines participants, networking, and rules.", "Create or assign the first asset the scene root reads.");
            AddChainLink(chainContainer, "GAME MODE", mode, "The specific setup profile and win/loss rules.", "Create or assign the rules asset for this session.");
            AddChainLink(chainContainer, "SETUP PROFILE", setupProfile, "Combines capability ingredients before prefab or scene wiring starts.", "Create or assign the profile that combines game capability ingredients.");

            if (report != null && !string.IsNullOrEmpty(report.NextStep))
                DrawNextRecommendedAction(mapView, report.NextStep);

            root.Add(mapView);
        }

        private static void AddChainLink(VisualElement container, string step, Object target, string description, string missingMessage)
        {
            bool isConnected = target != null;
            var link = new VisualElement();
            link.AddToClassList("setup-link");
            link.style.marginBottom = 10;
            link.style.paddingLeft = 10;
            link.style.borderLeftWidth = 4;
            link.style.borderLeftColor = isConnected ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;

            var leftSide = new VisualElement();
            leftSide.style.flexDirection = FlexDirection.Row;

            var stepLabel = new Label(step);
            stepLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            stepLabel.style.width = 100;
            leftSide.Add(stepLabel);

            var statusLabel = new Label(isConnected ? $"CONNECTED: {target.name}" : "MISSING");
            statusLabel.style.color = isConnected ? new Color(0.8f, 0.8f, 0.8f) : new Color(1f, 0.6f, 0.6f);
            leftSide.Add(statusLabel);
            header.Add(leftSide);

            if (isConnected)
            {
                var selectButton = new Button(() => Selection.activeObject = target) { text = "SELECT" };
                selectButton.style.height = 18;
                selectButton.style.fontSize = 9;
                header.Add(selectButton);
            }

            link.Add(header);

            var descriptionLabel = new Label(isConnected ? description : missingMessage);
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.fontSize = 11;
            descriptionLabel.style.opacity = isConnected ? 0.7f : 1f;
            link.Add(descriptionLabel);

            container.Add(link);
        }

        private static void DrawNextRecommendedAction(VisualElement mapView, string nextStep)
        {
            var recommendation = new VisualElement();
            recommendation.style.marginTop = 15;
            recommendation.style.paddingTop = 10;
            recommendation.style.paddingBottom = 10;
            recommendation.style.paddingLeft = 10;
            recommendation.style.paddingRight = 10;
            recommendation.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            recommendation.style.borderLeftWidth = 4;
            recommendation.style.borderLeftColor = new Color(1f, 0.8f, 0f);

            var title = new Label("NEXT RECOMMENDED ACTION");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 10;
            title.style.opacity = 0.6f;
            recommendation.Add(title);

            var body = new Label(nextStep);
            body.style.whiteSpace = WhiteSpace.Normal;
            recommendation.Add(body);

            mapView.Add(recommendation);
        }

        private static void UpdateHygiene(
            VisualElement container,
            Object activeSetup,
            PyralisAuthoringIntentSelection intentSelection)
        {
            if (container == null)
                return;

            container.Clear();

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(activeSetup, bootstrap);

            var scanIssues = PyralisAssetHygieneScanner.Scan(session);
            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(intentSelection);

            if (model.HygieneIssues.Count == 0 && scanIssues.Count == 0)
            {
                DrawHygieneSuccess(container);
                return;
            }

            var header = new Label($"HYGIENE ALERTS ({model.HygieneIssues.Count + scanIssues.Count})");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 8;
            container.Add(header);

            foreach (PyralisAuthoringIssue issue in model.HygieneIssues)
            {
                var issueBox = new HelpBox(issue.Reason, GetHelpBoxType(issue.Severity));
                issueBox.style.marginBottom = 4;
                container.Add(issueBox);
            }

            if (scanIssues.Count == 0)
                return;

            var subHeader = new Label("ASSET CONFIGURATION ISSUES");
            subHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            subHeader.style.marginTop = 8;
            subHeader.style.marginBottom = 4;
            subHeader.style.opacity = 0.7f;
            container.Add(subHeader);

            foreach (PyralisHygieneIssue issue in scanIssues)
                DrawAssetIssue(container, issue);
        }

        private static void DrawHygieneSuccess(VisualElement container)
        {
            var success = new VisualElement();
            success.style.paddingTop = 20;
            success.style.alignItems = Align.Center;

            var label = new Label("Project hygiene: 100%") { name = "hygieneSuccess" };
            label.style.fontSize = 18;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = new Color(0.4f, 1f, 0.4f);
            success.Add(label);

            var subLabel = new Label("All active authoring contracts have high-fidelity metadata and proofs.") { name = "hygieneSubLabel" };
            subLabel.style.opacity = 0.7f;
            success.Add(subLabel);

            container.Add(success);
        }

        private static void DrawAssetIssue(VisualElement container, PyralisHygieneIssue issue)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 2;

            var button = new Button(() => { Selection.activeObject = issue.Asset; }) { text = "VIEW" };
            button.style.width = 50;
            row.Add(button);

            var message = new Label(issue.Message);
            message.style.marginLeft = 5;
            message.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(message);

            container.Add(row);
        }

        private static HelpBoxMessageType GetHelpBoxType(PyralisAuthoringIssueSeverity severity)
        {
            return severity switch
            {
                PyralisAuthoringIssueSeverity.Required => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Blocked => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Bug => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Recommended => HelpBoxMessageType.Warning,
                PyralisAuthoringIssueSeverity.Optional => HelpBoxMessageType.Info,
                PyralisAuthoringIssueSeverity.Info => HelpBoxMessageType.Info,
                _ => HelpBoxMessageType.None
            };
        }
    }
}

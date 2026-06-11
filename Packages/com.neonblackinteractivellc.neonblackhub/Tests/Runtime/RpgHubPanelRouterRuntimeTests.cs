using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgHubPanelRouterRuntimeTests
    {
        [Test]
        public void RpgHubPanelRouter_ShowInteractionResult_OpensMatchingPanel()
        {
            GameObject root = new GameObject("Root");
            try
            {
                HubInteractionHudPresenter hud = root.AddComponent<HubInteractionHudPresenter>();
                RpgHubPanelRouter router = root.AddComponent<RpgHubPanelRouter>();
                RpgPanelRoutePresenter dialoguePanel = CreatePanel(root.transform, PlayerPanelRoute.Dialogue);
                router.ConfigureForTests(hud, new[] { dialoguePanel });

                hud.ShowInteractionResult(Result(PlayerPanelRoute.Dialogue, "dialogue.elder"));

                Assert.That(dialoguePanel.IsOpen, Is.True);
                Assert.That(dialoguePanel.LastResult.DialogueGraphId, Is.EqualTo("dialogue.elder"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgHubPanelRouter_ShowInteractionResult_ClosesPreviousPanelWhenRouteChanges()
        {
            GameObject root = new GameObject("Root");
            try
            {
                HubInteractionHudPresenter hud = root.AddComponent<HubInteractionHudPresenter>();
                RpgHubPanelRouter router = root.AddComponent<RpgHubPanelRouter>();
                RpgPanelRoutePresenter questBoard = CreatePanel(root.transform, PlayerPanelRoute.QuestBoard);
                RpgPanelRoutePresenter vendor = CreatePanel(root.transform, PlayerPanelRoute.Vendor);
                router.ConfigureForTests(hud, new[] { questBoard, vendor });

                hud.ShowInteractionResult(Result(PlayerPanelRoute.QuestBoard, string.Empty));
                hud.ShowInteractionResult(Result(PlayerPanelRoute.Vendor, string.Empty));

                Assert.That(questBoard.IsOpen, Is.False);
                Assert.That(vendor.IsOpen, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgHubPanelRouter_GetRuntimeValidationIssues_ReportsMissingPresenters()
        {
            GameObject root = new GameObject("Root");
            try
            {
                RpgHubPanelRouter router = root.AddComponent<RpgHubPanelRouter>();

                string[] issues = ((IRuntimeValidationProvider)router).GetRuntimeValidationIssues().ToArray();

                Assert.That(issues.Any(issue => issue.Contains("route presenter")), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static RpgPanelRoutePresenter CreatePanel(Transform parent, PlayerPanelRoute route)
        {
            GameObject panelObject = new GameObject(route + " Panel");
            panelObject.transform.SetParent(parent);
            RpgPanelRoutePresenter presenter = panelObject.AddComponent<RpgPanelRoutePresenter>();
            presenter.ConfigureForTests(route, panelObject);
            panelObject.SetActive(false);
            return presenter;
        }

        private static HubInteractionResult Result(PlayerPanelRoute route, string dialogueGraphId)
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                route,
                string.Empty,
                dialogueGraphId,
                string.Empty,
                new[] { new HubNotificationPayload("Opened", route.ToString(), string.Empty, "info", 1f) });
        }
    }
}

using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Panel Route Presenter")]
    public sealed class RpgPanelRoutePresenter : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Route")]
        [SerializeField] private PlayerPanelRoute route = PlayerPanelRoute.Dialogue;

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI bodyLabel;
        [SerializeField] private TextMeshProUGUI contextLabel;

        [Header("Copy")]
        [SerializeField] private string titleOverride = string.Empty;
        [SerializeField] private string emptyBodyText = string.Empty;

        public PlayerPanelRoute Route => route;
        public bool IsOpen => panelRoot != null ? panelRoot.activeSelf : gameObject.activeSelf;
        public HubInteractionResult LastResult { get; private set; } = HubInteractionResult.Invalid("Panel has not been opened.");
        public event System.Action<HubInteractionResult> PanelOpened;

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;
        }

        public void ConfigureForTests(PlayerPanelRoute panelRoute, GameObject root)
        {
            route = panelRoute;
            panelRoot = root;
        }

        public bool CanPresent(PlayerPanelRoute panelRoute)
        {
            return route == panelRoute && route != PlayerPanelRoute.None;
        }

        public void Open(HubInteractionResult result)
        {
            LastResult = result;
            if (panelRoot == null)
                panelRoot = gameObject;

            panelRoot.SetActive(true);
            Render(result);
            PanelOpened?.Invoke(result);
        }

        public void Close()
        {
            if (panelRoot == null)
                panelRoot = gameObject;

            panelRoot.SetActive(false);
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (route == PlayerPanelRoute.None)
                yield return "`RpgPanelRoutePresenter` should use a concrete panel route.";

            if (panelRoot == null)
                yield return "`RpgPanelRoutePresenter` should reference Panel Root, or live directly on the panel root GameObject.";
        }

        private void Render(HubInteractionResult result)
        {
            if (titleLabel != null)
                titleLabel.text = string.IsNullOrWhiteSpace(titleOverride) ? RouteToTitle(route) : titleOverride;

            if (bodyLabel != null)
                bodyLabel.text = BuildBody(result);

            if (contextLabel != null)
                contextLabel.text = BuildContext(result);
        }

        private string BuildBody(HubInteractionResult result)
        {
            if (result.Notifications != null && result.Notifications.Length > 0 && !string.IsNullOrWhiteSpace(result.Notifications[0].Body))
                return result.Notifications[0].Body;

            if (!string.IsNullOrWhiteSpace(emptyBodyText))
                return emptyBodyText;

            switch (route)
            {
                case PlayerPanelRoute.Dialogue:
                    return string.IsNullOrWhiteSpace(result.DialogueGraphId) ? "Dialogue" : result.DialogueGraphId;
                case PlayerPanelRoute.QuestBoard:
                    return "Quest board";
                case PlayerPanelRoute.Vendor:
                    return "Vendor";
                case PlayerPanelRoute.Loadout:
                    return "Loadout";
                case PlayerPanelRoute.SkillTree:
                case PlayerPanelRoute.Trainer:
                    return "Skill tree";
                default:
                    return route.ToString();
            }
        }

        private static string BuildContext(HubInteractionResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.NpcId))
                return result.NpcId;

            if (!string.IsNullOrWhiteSpace(result.SceneId))
                return result.SceneId;

            if (!string.IsNullOrWhiteSpace(result.DialogueGraphId))
                return result.DialogueGraphId;

            return string.Empty;
        }

        private static string RouteToTitle(PlayerPanelRoute route)
        {
            switch (route)
            {
                case PlayerPanelRoute.QuestBoard:
                    return "Quest Board";
                case PlayerPanelRoute.SkillTree:
                    return "Skill Tree";
                default:
                    return route.ToString();
            }
        }
    }
}

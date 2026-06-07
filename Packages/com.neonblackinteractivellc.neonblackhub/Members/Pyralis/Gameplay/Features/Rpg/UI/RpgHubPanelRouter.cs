using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Hub Panel Router")]
    public sealed class RpgHubPanelRouter : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Source")]
        [SerializeField] private HubInteractionHudPresenter hudPresenter;

        [Header("Panels")]
        [SerializeField] private RpgPanelRoutePresenter[] routePresenters;
        [SerializeField] private bool closePanelsWhenRouteMissing = true;

        public HubInteractionResult LastResult { get; private set; } = HubInteractionResult.Invalid("No panel route has been shown yet.");

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            BindPresenter();
        }

        private void OnDisable()
        {
            UnbindPresenter();
        }

        public void ConfigureForTests(HubInteractionHudPresenter presenter, RpgPanelRoutePresenter[] presenters)
        {
            UnbindPresenter();
            hudPresenter = presenter;
            routePresenters = presenters;
            BindPresenter();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            LastResult = result;

            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute == PlayerPanelRoute.None)
            {
                if (closePanelsWhenRouteMissing)
                    CloseAllPanels();
                return false;
            }

            RpgPanelRoutePresenter presenter = FindPresenter(result.PanelRoute);
            if (presenter == null)
            {
                if (closePanelsWhenRouteMissing)
                    CloseAllPanels();
                return false;
            }

            CloseAllExcept(presenter);
            presenter.Open(result);
            return true;
        }

        public void CloseAllPanels()
        {
            RpgPanelRoutePresenter[] presenters = GetPresenters();
            for (int i = 0; i < presenters.Length; i++)
                presenters[i]?.Close();
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (hudPresenter == null)
                yield return "`RpgHubPanelRouter` should reference the Hub Interaction HUD Presenter that publishes selected interaction results.";

            if (GetPresenters().Length == 0)
                yield return "`RpgHubPanelRouter` should reference at least one route presenter.";
        }

        private void HandleInteractionResultShown(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void ResolveReferences()
        {
            if (hudPresenter == null)
                hudPresenter = GetComponentInParent<HubInteractionHudPresenter>() ?? GetComponentInChildren<HubInteractionHudPresenter>(true);

            if (routePresenters == null || routePresenters.Length == 0)
                routePresenters = GetComponentsInChildren<RpgPanelRoutePresenter>(true);
        }

        private void BindPresenter()
        {
            ResolveReferences();
            if (hudPresenter != null)
                hudPresenter.InteractionResultShown += HandleInteractionResultShown;
        }

        private void UnbindPresenter()
        {
            if (hudPresenter != null)
                hudPresenter.InteractionResultShown -= HandleInteractionResultShown;
        }

        private RpgPanelRoutePresenter FindPresenter(PlayerPanelRoute route)
        {
            RpgPanelRoutePresenter[] presenters = GetPresenters();
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] != null && presenters[i].CanPresent(route))
                    return presenters[i];
            }

            return null;
        }

        private RpgPanelRoutePresenter[] GetPresenters()
        {
            return routePresenters ?? System.Array.Empty<RpgPanelRoutePresenter>();
        }

        private void CloseAllExcept(RpgPanelRoutePresenter activePresenter)
        {
            RpgPanelRoutePresenter[] presenters = GetPresenters();
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] != null && presenters[i] != activePresenter)
                    presenters[i].Close();
            }
        }
    }
}

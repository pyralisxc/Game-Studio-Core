using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Input;
using NeonBlack.Gameplay.Modules.Scoring;
using UnityEngine;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D
{
    public partial class GameManager
    {
        [Header("Standalone Compatibility")]
        [SerializeField, Tooltip("Optional explicit 2D controller list for standalone scenes that do not use participant spawning. Participant-spawned scenes should leave this empty and use the roster.")]
        private Motor2D[] playerControllers;

        private readonly List<Motor2D> _trackedPlayerControllers = new List<Motor2D>(8);
        private readonly Dictionary<Motor2D, Vector3> _playerStartPositions = new Dictionary<Motor2D, Vector3>();
        private Motor2D _primaryPlayerController;

        public bool TryHandleHazardImpact(GameObject target, GameObject source, Vector3 hitPoint)
        {
            Motor2D deadPlayer = target != null ? target.GetComponentInParent<Motor2D>() : null;

            if (deadPlayer == null)
                return false;

            PlayerDied(deadPlayer);
            return true;
        }

        private bool AreAllTrackedPlayersDead()
        {
            bool foundAnyPlayer = false;
            for (int i = 0; i < _trackedPlayerControllers.Count; i++)
            {
                Motor2D playerController = _trackedPlayerControllers[i];
                if (playerController == null)
                    continue;

                foundAnyPlayer = true;
                if (!playerController.IsDead)
                    return false;
            }

            return foundAnyPlayer;
        }

        private void RefreshTrackedPlayers(bool includeInactive)
        {
            _trackedPlayerControllers.Clear();

            if (playerControllers != null && playerControllers.Length > 0)
            {
                for (int i = 0; i < playerControllers.Length; i++)
                    RegisterTrackedPlayer(playerControllers[i], includeInactive);
            }
            else
            {
                RegisterRosterPlayers(includeInactive);
            }

            _primaryPlayerController = _trackedPlayerControllers.Count > 0 ? _trackedPlayerControllers[0] : null;
        }

        private void RegisterRosterPlayers(bool includeInactive)
        {
            if (_participantRosterService == null)
                return;

            for (int i = 0; i < _participantRosterService.Participants.Count; i++)
            {
                ParticipantHandle participant = _participantRosterService.Participants[i];
                if (participant?.PawnInstance == null)
                    continue;

                if (!includeInactive && !participant.PawnInstance.activeInHierarchy)
                    continue;

                RegisterTrackedPlayer(participant.PawnInstance.GetComponent<Motor2D>(), includeInactive);
            }
        }

        private void RegisterTrackedPlayer(Motor2D controller, bool includeInactive)
        {
            if (controller == null || _trackedPlayerControllers.Contains(controller))
                return;

            if (!includeInactive && !controller.gameObject.activeInHierarchy)
                return;

            _trackedPlayerControllers.Add(controller);
            if (!_playerStartPositions.ContainsKey(controller))
                _playerStartPositions[controller] = controller.transform.position;

            IGameplayStateReader stateReader = ResolveGameplayStateReader();

            Pawn2DMovementComponent movement = controller.GetComponent<Pawn2DMovementComponent>();
            movement?.ConfigureRuntime(stateReader, _cameraBoundsProvider);

            PlayerInputHandler inputHandler = controller.GetComponent<PlayerInputHandler>();
            inputHandler?.ConfigureRuntime(stateReader);

            StillnessBonus2D stillnessBonus = controller.GetComponent<StillnessBonus2D>();
            stillnessBonus?.ConfigureRuntime(stateReader, scoreManager);
        }
    }
}

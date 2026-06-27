using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Modules.Encounters
{
    /// <summary>
    /// Defines a self-contained combat section. When the player enters the trigger:
    ///   - Optionally switches the camera profile
    ///   - Starts linked EnemySpawners
    ///   - Blocks the exit until all tracked enemies are dead
    ///   - Unlocks exit and fires OnCleared when the zone is finished
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public partial class ArenaZone : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Fired the first time the player enters the zone.")]
        public UnityEvent OnEntered;

        [Tooltip("Fired once all enemies are dead and the zone is cleared.")]
        public UnityEvent OnCleared;

        [Header("Tag")]
        [Tooltip("Tag used to identify the player GameObject.")]
        [SerializeField] private string playerTag = "Player";

        private bool _triggered;
        private bool _cleared;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !IsPlayer(other.gameObject))
                return;

            _triggered = true;

            SetExitBlockersActive(true);

            SwitchCamera(onEnterCameraProfile);
            ActivateSpawners();

            OnEntered?.Invoke();
            StartCoroutine(PollForClearRoutine());
        }

        private bool IsPlayer(GameObject go)
        {
            if (go.CompareTag(playerTag))
                return true;

            return ParticipantQueryUtility.TryResolveParticipant(go, out _);
        }

        private void OnZoneCleared()
        {
            SetExitBlockersActive(false);
            SwitchCamera(onClearCameraProfile);
            OnCleared?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                return;

            Gizmos.color = _cleared
                ? new Color(0f, 1f, 0f, 0.12f)
                : _triggered
                    ? new Color(1f, 0.4f, 0f, 0.18f)
                    : new Color(0f, 0.6f, 1f, 0.12f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = _cleared
                ? new Color(0f, 1f, 0f, 0.6f)
                : _triggered
                    ? new Color(1f, 0.4f, 0f, 0.6f)
                    : new Color(0f, 0.6f, 1f, 0.5f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
#endif
    }
}

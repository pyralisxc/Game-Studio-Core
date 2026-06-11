using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Spawning;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace NeonBlack.Gameplay.Features.Encounters
{
    /// <summary>
    /// Defines a self-contained combat section. When the player enters the trigger:
    ///   - Optionally switches the camera profile
    ///   - Starts linked EnemySpawners
    ///   - Blocks the exit until all tracked enemies are dead
    ///   - Unlocks exit and fires OnCleared when the zone is finished
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ArenaZone : MonoBehaviour
    {
        [Header("Runtime References")]
        [Tooltip("Optional explicit camera rig reference. When left empty, Pyralis injects the active shared camera rig.")]
        [SerializeField] private CinemachineCameraRigController cameraRigController;

        [Header("Spawners")]
        [Tooltip("EnemySpawner GameObjects to activate when the player enters.")]
        [SerializeField] private EnemySpawner[] enemySpawners;

        [Header("Exit Blockers")]
        [Tooltip("GameObjects (walls, gates, barriers) that block the exit.")]
        [SerializeField] private GameObject[] exitBlockers;

        [Header("Camera Profile")]
        [Tooltip("CameraRigProfile asset to switch to when the player enters. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onEnterCameraProfile;

        [Tooltip("CameraRigProfile asset to switch to when the zone is cleared. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onClearCameraProfile;

        [Tooltip("Blend duration in seconds for the camera profile transition.")]
        [SerializeField] private float cameraTransitionDuration = 0.5f;

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
        private readonly List<HealthComponent> _trackedEnemies = new List<HealthComponent>();

        [Inject]
        private void Construct(CinemachineCameraRigController injectedCameraRigController = null)
        {
            cameraRigController = injectedCameraRigController != null
                ? injectedCameraRigController
                : cameraRigController;
        }

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner == null)
                    continue;

                spawner.gameObject.SetActive(false);
                spawner.EnemySpawned += RegisterEnemy;
            }

            foreach (GameObject blocker in exitBlockers)
            {
                if (blocker != null)
                    blocker.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner != null)
                    spawner.EnemySpawned -= RegisterEnemy;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !IsPlayer(other.gameObject))
                return;

            _triggered = true;

            foreach (GameObject blocker in exitBlockers)
            {
                if (blocker != null)
                    blocker.SetActive(true);
            }

            SwitchCamera(onEnterCameraProfile);

            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner == null)
                    continue;

                RegisterTrackedSpawnerEnemies(spawner);
                spawner.gameObject.SetActive(true);
            }

            OnEntered?.Invoke();
            StartCoroutine(PollForClearRoutine());
        }

        private bool IsPlayer(GameObject go)
        {
            if (go.CompareTag(playerTag))
                return true;

            return ParticipantQueryUtility.TryResolveParticipant(go, out _);
        }

        private IEnumerator PollForClearRoutine()
        {
            yield return new WaitForSeconds(1.5f);

            while (!_cleared)
            {
                yield return new WaitForSeconds(0.5f);

                if (!AllSpawnersFinished())
                    continue;

                if (!AllTrackedEnemiesDead())
                    continue;

                _cleared = true;
                OnZoneCleared();
            }
        }

    private bool AllSpawnersFinished()
    {
        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (spawner != null && !spawner.IsFinished)
                return false;
        }

        return true;
    }

    private bool AllTrackedEnemiesDead()
    {
        for (int i = _trackedEnemies.Count - 1; i >= 0; i--)
        {
            HealthComponent enemy = _trackedEnemies[i];
            if (enemy == null || enemy.IsDead)
            {
                _trackedEnemies.RemoveAt(i);
                continue;
            }

            return false;
        }

        return true;
    }

    private void OnZoneCleared()
    {
        foreach (GameObject blocker in exitBlockers)
        {
            if (blocker != null)
                blocker.SetActive(false);
        }

        SwitchCamera(onClearCameraProfile);
        OnCleared?.Invoke();
    }

    private void SwitchCamera(CameraRigProfile profile)
    {
        if (profile == null)
            return;

        cameraRigController?.SwitchProfile(profile, cameraTransitionDuration);
    }

    private void RegisterTrackedSpawnerEnemies(EnemySpawner spawner)
    {
        if (spawner == null)
            return;

        IReadOnlyList<HealthComponent> trackedEnemies = spawner.TrackedEnemies;
        for (int i = 0; i < trackedEnemies.Count; i++)
            RegisterEnemy(trackedEnemies[i]);
    }

    /// <summary>Register an enemy that was spawned dynamically so the zone can track it.</summary>
    public void RegisterEnemy(HealthComponent enemy)
    {
        if (enemy != null && !_trackedEnemies.Contains(enemy))
            _trackedEnemies.Add(enemy);
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

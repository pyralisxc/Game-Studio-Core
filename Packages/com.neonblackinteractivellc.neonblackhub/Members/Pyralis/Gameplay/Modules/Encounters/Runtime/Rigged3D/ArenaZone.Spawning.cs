using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Encounters
{
    public partial class ArenaZone
    {
        [Header("Spawners")]
        [Tooltip("Spawner components to activate when the player enters. Each component must implement IEncounterSpawnSource.")]
        [SerializeField] private MonoBehaviour[] enemySpawners;

        [Header("Exit Blockers")]
        [Tooltip("GameObjects (walls, gates, barriers) that block the exit.")]
        [SerializeField] private GameObject[] exitBlockers;

        private readonly List<IActorHealthState> _trackedEnemies = new List<IActorHealthState>();

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            foreach (MonoBehaviour spawnerBehaviour in enemySpawners)
            {
                IEncounterSpawnSource spawner = ResolveSpawnSource(spawnerBehaviour);
                if (spawner == null)
                    continue;

                spawnerBehaviour.gameObject.SetActive(false);
                spawner.ActorSpawned += RegisterEnemy;
            }

            SetExitBlockersActive(false);
        }

        private void OnDestroy()
        {
            foreach (MonoBehaviour spawnerBehaviour in enemySpawners)
            {
                IEncounterSpawnSource spawner = ResolveSpawnSource(spawnerBehaviour);
                if (spawner != null)
                    spawner.ActorSpawned -= RegisterEnemy;
            }
        }

        private void ActivateSpawners()
        {
            foreach (MonoBehaviour spawnerBehaviour in enemySpawners)
            {
                IEncounterSpawnSource spawner = ResolveSpawnSource(spawnerBehaviour);
                if (spawner == null)
                    continue;

                RegisterTrackedSpawnerEnemies(spawner);
                spawnerBehaviour.gameObject.SetActive(true);
            }
        }

        private void SetExitBlockersActive(bool active)
        {
            foreach (GameObject blocker in exitBlockers)
            {
                if (blocker != null)
                    blocker.SetActive(active);
            }
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
            foreach (MonoBehaviour spawnerBehaviour in enemySpawners)
            {
                IEncounterSpawnSource spawner = ResolveSpawnSource(spawnerBehaviour);
                if (spawner != null && !spawner.IsFinished)
                    return false;
            }

            return true;
        }

        private bool AllTrackedEnemiesDead()
        {
            for (int i = _trackedEnemies.Count - 1; i >= 0; i--)
            {
                IActorHealthState enemy = _trackedEnemies[i];
                if (IsMissing(enemy) || enemy.IsDead)
                {
                    _trackedEnemies.RemoveAt(i);
                    continue;
                }

                return false;
            }

            return true;
        }

        private void RegisterTrackedSpawnerEnemies(IEncounterSpawnSource spawner)
        {
            if (spawner == null)
                return;

            IReadOnlyList<IActorHealthState> trackedEnemies = spawner.TrackedActors;
            for (int i = 0; i < trackedEnemies.Count; i++)
                RegisterEnemy(trackedEnemies[i]);
        }

        private static IEncounterSpawnSource ResolveSpawnSource(MonoBehaviour spawnerBehaviour)
        {
            return spawnerBehaviour as IEncounterSpawnSource;
        }

        /// <summary>Register an enemy that was spawned dynamically so the zone can track it.</summary>
        public void RegisterEnemy(IActorHealthState enemy)
        {
            if (!IsMissing(enemy) && !_trackedEnemies.Contains(enemy))
                _trackedEnemies.Add(enemy);
        }

        private static bool IsMissing(IActorHealthState enemy)
        {
            if (enemy == null)
                return true;

            return enemy is Object unityObject && unityObject == null;
        }
    }
}

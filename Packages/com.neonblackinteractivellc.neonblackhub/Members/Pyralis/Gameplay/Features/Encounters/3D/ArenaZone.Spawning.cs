using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Spawning;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Encounters
{
    public partial class ArenaZone
    {
        [Header("Spawners")]
        [Tooltip("EnemySpawner GameObjects to activate when the player enters.")]
        [SerializeField] private EnemySpawner[] enemySpawners;

        [Header("Exit Blockers")]
        [Tooltip("GameObjects (walls, gates, barriers) that block the exit.")]
        [SerializeField] private GameObject[] exitBlockers;

        private readonly List<HealthComponent> _trackedEnemies = new List<HealthComponent>();

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

            SetExitBlockersActive(false);
        }

        private void OnDestroy()
        {
            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner != null)
                    spawner.EnemySpawned -= RegisterEnemy;
            }
        }

        private void ActivateSpawners()
        {
            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner == null)
                    continue;

                RegisterTrackedSpawnerEnemies(spawner);
                spawner.gameObject.SetActive(true);
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
    }
}

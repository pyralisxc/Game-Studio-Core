using System.Collections;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AddComponentMenu("")]
    public sealed class ProjectilePoolHandle : MonoBehaviour
    {
        private ProjectileLauncherBase _owner;
        private GameObject _prefab;
        private Coroutine _returnRoutine;

        internal void Configure(ProjectileLauncherBase owner, GameObject prefab)
        {
            _owner = owner;
            _prefab = prefab;
        }

        internal void ScheduleReturn(float delay)
        {
            if (_returnRoutine != null)
                StopCoroutine(_returnRoutine);

            if (_owner != null && delay > 0f && gameObject.activeInHierarchy)
                _returnRoutine = StartCoroutine(ReturnAfter(delay));
        }

        public bool ReleaseToPool()
        {
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            if (_owner == null || _prefab == null)
                return false;

            _owner.ReturnToPool(_prefab, gameObject);
            return true;
        }

        private IEnumerator ReturnAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _returnRoutine = null;
            ReleaseToPool();
        }
    }
}

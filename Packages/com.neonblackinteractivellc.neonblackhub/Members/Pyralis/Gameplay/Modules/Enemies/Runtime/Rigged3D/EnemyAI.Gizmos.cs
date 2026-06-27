using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public partial class EnemyAI
    {
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (DetectionModule == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.AggroRange);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, DetectionModule.LeashRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, CombatModule != null ? CombatModule.MinAttackRange : 1f);
        }
#endif
    }
}

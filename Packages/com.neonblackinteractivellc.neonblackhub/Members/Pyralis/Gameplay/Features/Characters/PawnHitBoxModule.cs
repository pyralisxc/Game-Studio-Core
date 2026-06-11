using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.CombatSensors,
        Relevance = "Pawn module for managing and triggering melee hitboxes by zone name.",
        AssignmentFields = new[] { nameof(hitBoxZones) },
        FirstProof = "Verify hitboxes are correctly mirrored when the pawn flips direction.",
        ExpertAdvice = "Each HitBoxSlot maps a 'Zone Name' to a physical HitBox component. Ensure the zone names match your attack definitions.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
    public class PawnHitBoxModule : MonoBehaviour
{
        [SerializeField] private HitBoxSlot[] hitBoxZones;

        public HitBoxSlot[] HitBoxZones => hitBoxZones;

        private void Awake()
        {
            CacheHitBoxOffsets();
        }

        public void CacheHitBoxOffsets()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
            {
                slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.5f)
                    : 0.5f;
            }
        }

        public void SyncHitBoxSides(bool facingRight)
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
                slot.MirrorToSide(transform, facingRight);
        }

        public HitBox GetZoneByName(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName))
                return null;

            foreach (HitBoxSlot slot in hitBoxZones)
                if (slot.zoneName == zoneName)
                    return slot.hitBox;

            return null;
        }

        private struct ActiveHitBox
        {
            public HitBox Box;
            public float Damage;
            public float Knockback;
            public float DelayTimer;
            public float DurationTimer;
            public bool IsActive;

            public ActiveHitBox(HitBox box, float damage, float knockback, float delay, float duration)
            {
                Box = box;
                Damage = damage;
                Knockback = knockback;
                DelayTimer = delay;
                DurationTimer = duration;
                IsActive = true;
            }
        }

        private readonly List<ActiveHitBox> _activeHitBoxes = new List<ActiveHitBox>();

        public void Tick(float deltaTime)
        {
            for (int i = _activeHitBoxes.Count - 1; i >= 0; i--)
            {
                var hb = _activeHitBoxes[i];
                if (hb.DelayTimer > 0f)
                {
                    hb.DelayTimer -= deltaTime;
                    if (hb.DelayTimer <= 0f)
                    {
                        hb.Box.Fire(hb.Damage, hb.Knockback);
                    }
                }
                else if (hb.DurationTimer > 0f)
                {
                    hb.DurationTimer -= deltaTime;
                    if (hb.DurationTimer > 0f)
                    {
                        hb.Box.FireAdditive(hb.Damage, hb.Knockback);
                    }
                    else
                    {
                        hb.IsActive = false;
                    }
                }
                else
                {
                    hb.IsActive = false;
                }

                if (!hb.IsActive)
                {
                    _activeHitBoxes.RemoveAt(i);
                }
                else
                {
                    _activeHitBoxes[i] = hb;
                }
            }
        }

        public void ActivateHitBox(string zoneName, float damage, float knockback, float delay, float duration)
        {
            HitBox box = GetZoneByName(zoneName);
            if (box == null) return;

            _activeHitBoxes.Add(new ActiveHitBox(box, damage, knockback, delay, duration));
        }

        public void SetHitBoxZones(HitBoxSlot[] zones)
        {
            hitBoxZones = zones;
            CacheHitBoxOffsets();
        }
    }
}
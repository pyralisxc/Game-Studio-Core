using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// All mutable runtime state owned exclusively by <see cref="BrawlerMovementModel"/>.
    /// Exposed read-only to the MonoBehaviour layer via <see cref="BrawlerMovementModel.State"/>.
    /// </summary>
    public sealed class MovementState
    {
        // ── Velocity ──────────────────────────────────────────────────────── //
        public float VelocityY;   // vertical (gravity / jump)
        public float VelocityX;   // smoothed horizontal
        public float VelocityZ;   // smoothed depth

        // ── Jump feel ─────────────────────────────────────────────────────── //
        public float CoyoteCounter;
        public float JumpBufferCounter;
        public bool  JumpCut;
        public int   JumpsUsed;

        // ── Ground ────────────────────────────────────────────────────────── //
        public bool    IsGrounded;
        public bool    WasGrounded;
        public float   PreLandVelocityY;
        public Vector3 GroundNormal = Vector3.up;

        // ── Movement flags ────────────────────────────────────────────────── //
        public bool IsSprinting;
        public bool IsCrouching;
        public bool FacingRight = true;
        public bool PrevMoving;
        public bool IsMoving;

        // ── Slide / wall ──────────────────────────────────────────────────── //
        public bool IsSliding;
        public bool IsWallSliding;
        public bool HasWallContact;

        // ── Dodge ─────────────────────────────────────────────────────────── //
        public bool    IsDodging;
        public Vector2 DodgeDir;
        public float   DodgeElapsed;
        public float   DodgeTimer;

        // ── Power slide ───────────────────────────────────────────────────── //
        public bool  IsPowerSliding;
        public float PowerSlideElapsed;
        public float PowerSlideTimer;
        public float PowerSlideVelocityX;

        // ── Climb / hang ──────────────────────────────────────────────────── //
        public bool  IsClimbing;
        public bool  IsHanging;
        public float ClimbTimer;

        // ── Cooldowns ─────────────────────────────────────────────────────── //
        public float RollTimer;
        public float LandSlowTimer;

        // ── Acting (combat locks movement) ────────────────────────────────── //
        public bool IsActing;

        // ── One-frame trigger signals (read once per frame by the MB layer) ── //
        public bool  TriggerJump;
        public bool  TriggerDiveRoll;
        public bool  TriggerPowerSlide;
        public bool  TriggerMoveToIdle;
        public bool  TriggerJustLanded;
        public bool  TriggerKnockedBack;

        /// <summary>Downward speed captured on the landing frame — used by the MB for squash threshold.</summary>
        public float LandImpactSpeed;
    }
}

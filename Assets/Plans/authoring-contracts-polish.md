# Authoring Contracts Polish Plan

## Phase 1: Locomotion & Physics (The 'Grounded' Layer)
- Update **Motor2D** and **Motor3D** with Expert Advice (Rigidbody2D interpolation, CharacterController skin width) and First Proofs.
- Update **Pawn2DMovementComponent** and **Pawn3DMovementComponent** with AssignmentFields (Walk Speed, Jump Height) and First Proofs.

## Phase 2: Actor State & Presentation (The 'Visual' Layer)
- Update **HealthComponent** with Expert Advice (Faction filtering, UnityEvent wiring) and First Proofs.
- Update **ActorAnimationDriver** with Expert Advice (Parameter matching) and Native Setup instructions.

## Phase 3: Session & Game Rules (The 'Orchestrator' Layer)
- Update **SessionDefinition** with First Proof and Documentation URL.
- Update **GameModeDefinition** with AssignmentFields (Win/Loss conditions) and First Proof.
- Update **ParticipantDefinition** with First Proof and granular Native Setup.

## Phase 4: Combat & AI Logic (The 'Action' Layer)
- Update **PawnCombatBehaviour** (MeleeFlow) with Expert Advice on sequences and attack interrupts.
- Update **EnemyAI** (TacticsAggressive / Steering3D) with Expert Advice and Detection Proofs.
- Update **HitBox** (CombatSensors) to ensure FX sinks are in AssignmentFields.

## Phase 5: Camera & Final Validation
- Update **CinemachineCameraRigController** (Camera) with First Proof and Expert Advice.
- Regenerate Engine Documentation and verify all contracts in the Authoring Window.

# 2D Brawler Combo Animation Spec

This records the intended 2D brawler combo flow for the shared NeonBlack Gameplay stack before Unity-side controller authoring.

## Goal

Create a 2D brawler attack state machine where:

- pressing `Attack` from neutral triggers a default whiff jab
- the jab returns to `Idle` if it does not hit an enemy hurtbox
- if the attack hitbox intersects an enemy hurtbox:
  - trigger a three-frame hit pause
  - set `allowComboBranch = true`
  - advance `comboCounter`
- timed follow-up button presses can branch into the next combo attack
- the final combo attack is a finisher with high knockback
- the finisher resets the combo counter and returns to neutral

## Runtime State Machine

Primary animation states:

- `Idle`
- `Walk`
- `Attack_Jab1_Start`
- `Attack_Jab1_Active`
- `Attack_Jab1_Recovery`
- `Attack_Jab2_Start`
- `Attack_Jab2_Active`
- `Attack_Jab2_Recovery`
- `Attack_Finisher_Start`
- `Attack_Finisher_Active`
- `Attack_Finisher_Recovery`
- `HitPause`

High-level transitions:

```text
Idle/Walk
  -- AttackPressed -->
Attack_Jab1_Start
  --> Attack_Jab1_Active
  -- no hit -->
Attack_Jab1_Recovery
  --> Idle

Attack_Jab1_Active
  -- hit confirmed -->
HitPause (3 frames)
  --> Attack_Jab1_Recovery (allowComboBranch = true, comboCounter = 1)

Attack_Jab1_Recovery
  -- AttackPressed inside combo window and allowComboBranch -->
Attack_Jab2_Start
  --> Attack_Jab2_Active
  -- combo window expired -->
Idle (comboCounter = 0)

Attack_Jab2_Active
  -- hit confirmed -->
HitPause (3 frames)
  --> Attack_Jab2_Recovery (allowComboBranch = true, comboCounter = 2)

Attack_Jab2_Recovery
  -- AttackPressed inside combo window and allowComboBranch -->
Attack_Finisher_Start
  --> Attack_Finisher_Active
  -- combo window expired -->
Idle (comboCounter = 0)

Attack_Finisher_Active
  -- hit confirmed -->
HitPause (3 frames)
  --> Attack_Finisher_Recovery

Attack_Finisher_Recovery
  --> Idle (comboCounter = 0, allowComboBranch = false)
```

## Required Animator Parameters

Recommended parameter set for the data-driven animation layer:

- `AttackPressed` trigger or signal
- `ComboStep` int
- `AttackState` int or enum-backed int
- `HitConfirmed` trigger
- `IsInRecovery` bool
- `IsMoving` bool
- `Finisher` trigger

Signal-oriented mapping target:

- `AttackPrimary` for jab chain steps
- `Custom` key `ComboConfirm`
- `Custom` key `ComboFinisher`
- `Idle` / `Move` for locomotion return

## Gameplay Flags

Core runtime flags:

- `int comboCounter`
- `bool allowComboBranch`
- `bool attackQueued`
- `bool hitConfirmedThisSwing`
- `float comboWindowRemaining`
- `bool inHitPause`

Suggested meanings:

- `comboCounter`
  - `0` = neutral
  - `1` = jab one confirmed
  - `2` = jab two confirmed
- `allowComboBranch`
  - true only after a valid hit confirm
- `attackQueued`
  - stores a buffered follow-up press during recovery
- `hitConfirmedThisSwing`
  - prevents multi-confirm logic from the same swing

## Hit Confirm Rules

Each attack swing should:

1. enable the attack hitbox for its active frames
2. listen for a hurtbox intersection
3. on the first valid hit only:
   - mark `hitConfirmedThisSwing = true`
   - start three-frame hit pause
   - set `allowComboBranch = true`
   - increment `comboCounter`
4. ignore additional combo confirms until the next swing

Whiff behavior:

- if the active frames end with no hit:
  - `allowComboBranch = false`
  - continue into recovery
  - return to `Idle`
  - reset `comboCounter = 0`

## Suggested Script Skeleton

```csharp
public sealed class BrawlerComboController2D : MonoBehaviour
{
    public int ComboCounter { get; private set; }
    public bool AllowComboBranch { get; private set; }

    private bool _attackQueued;
    private bool _hitConfirmedThisSwing;
    private float _comboWindowRemaining;
    private int _currentAttackIndex;

    public void OnAttackPressed()
    {
        if (CanStartNeutralAttack())
        {
            StartAttack(0); // jab 1
            return;
        }

        if (AllowComboBranch && IsInsideComboWindow())
            _attackQueued = true;
    }

    public void OnAttackHitConfirmed()
    {
        if (_hitConfirmedThisSwing)
            return;

        _hitConfirmedThisSwing = true;
        AllowComboBranch = true;
        ComboCounter++;
        StartHitPauseFrames(3);
    }

    public void OnRecoveryWindowOpened()
    {
        if (_attackQueued && AllowComboBranch)
            StartNextComboAttack();
    }

    public void OnAttackSequenceEnded()
    {
        if (_currentAttackIndex >= 2)
            ResetCombo();
        else if (!_hitConfirmedThisSwing)
            ResetCombo();
    }

    private void StartNextComboAttack() { }
    private void StartAttack(int index) { }
    private void ResetCombo() { }
}
```

## Timing Notes

Recommended first-pass timings:

- jab startup: `4-6` frames
- jab active: `2-4` frames
- jab recovery: `8-12` frames
- combo input buffer: `6-10` frames
- hit pause: exactly `3` frames
- finisher recovery: longer than jab recovery

## Finisher Rules

The finisher should:

- apply the highest knockback in the sequence
- consume the combo chain
- clear `allowComboBranch`
- clear any queued attack
- reset `comboCounter = 0` on exit

## Integration Direction

When this is implemented in runtime code, prefer:

- `HitBox2D` or a dedicated 2D hurtbox surface for hit confirm events
- the shared `ActorAnimationDriver` for animation signals
- profile-driven attack definitions instead of hardcoded animator parameter strings
- a dedicated 2D combat controller instead of pushing combo logic into `Motor2D`

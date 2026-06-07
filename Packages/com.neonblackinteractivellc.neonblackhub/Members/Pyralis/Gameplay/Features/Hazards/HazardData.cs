using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
/// <summary>
/// ScriptableObject defining all properties for a single hazard type.
///
/// HazardType - the core movement pattern (3 types):
///  Slam - stationary shadow, warning, slam, retract.
///  Crossing - enters from one edge, travels to the opposite (H, V, or Diagonal).
///  Bouncy  - spawns at a random on-screen position, travels fixed-distance segments,
///  changing direction N times based on the selected BouncePatternType(s).
///
/// Crossing modifiers (stack freely on Crossing or Bouncy):
///  enableWavyPath  - sinusoidal lateral oscillation along the travel path.
///  CrossingVariant  - Normal (smooth) or Jump (hop arcs).
///  speedCurve - speed multiplier over normalized travel progress.
///  entryDelay  - pause at spawn edge before movement begins.
///  hitLingerDuration - pause after travel ends before pooling.
///  enableTargeting  - continuously steers toward the player during travel.
///
/// Slam modifiers:
///  enableTargeting  - shadow drifts toward the player during approach + warning.
///
/// Explosion modifier (any type via enableExplosion + ExplosionTrigger):
///  OnImpact  - Slam: fires immediately at slam activation. Crossing/Bouncy: fires on contact with Player or another Hazard.
///  OnProximity  - Crossing/Bouncy: detonates when within explosionProximityRadius of player.
///  OnTimeElapsed - fires after explosionTimeDelay seconds of active travel.
///  OnLastBounce - Bouncy: fires when the final wall bounce is consumed.
///  OnExit  - fires just before pooling.
///
/// Colliders are NOT configured here - add any Collider2D type(s) to the prefab and wire them
/// into the Hazard component's _hitColliders list. Multiple hitboxes are fully supported.
///
/// Setup: right-click Project > Create > NeonBlack > Hazards > Hazard Data.
/// </summary>
[CreateAssetMenu(fileName = "NewHazardData", menuName = "NeonBlack/Hazards/Hazard Data")]
public class HazardData : ScriptableObject
{
  // Enums

  public enum HazardType
  {
  Slam,  // stationary slam
  Crossing,  // enters from one edge, travels to the opposite
  Bouncy,  // spawns at a random position; travels fixed segments, changing direction N times
  }

  public enum CrossingAxis  { Horizontal, Vertical, Diagonal }
  public enum CrossingVariant { Normal, Jump }

  public enum ExplosionTrigger
  {
  OnImpact,  // Slam: detonates immediately at slam activation; Crossing/Bouncy: detonates on contact (Player or Hazard)
  OnProximity,  // Crossing/Bouncy: detonates when within explosionProximityRadius of the player
  OnTimeElapsed,  // Crossing/Bouncy: detonates after explosionTimeDelay seconds of travel
  OnLastBounce,  // Bouncy only: detonates when the final bounce segment is consumed
  OnExit,  // any: detonates just before pooling
  }

  public enum BouncePatternType
  {
  //  Enum INTEGER ORDER must never change - existing ScriptableObjects serialize the index.
  // Always append new entries at the bottom.
  FullyRandom,  // [0] any direction 0-360 - total chaos, fully unpredictable
  AimedAtPlayer,  // [1] re-aims TOWARD the player at each bounce point (not continuous - combine with enableTargeting for homing)
  Diagonal,  // [2] strict 45 family; flips one axis per bounce - billiard-ball physics
  Ricochet,  // [3] deflects ~90 left or right of current dir - clean wall-bounce feel, never continues forward or reverses
  FleeFromPlayer, // [4] re-aims AWAY from the player at each bounce point - runs from the player
  Zigzag,  // [5] alternates sharp left/right turns each bounce - predictable zigzag sweeps the arena
  Orbit,  // [6] consistent ~90 clockwise or counterclockwise deflection - circles the arena
  }

  public enum BouncePatternSelection { Random, Sequential }

  public enum CrossingCollectibleMode
  {
  None,  // no collectible spawning during travel
  PerDistance, // one burst every collectible spawn interval world units traveled
  PerSecond,  // one burst every collectible spawn interval seconds of travel
  }

  public enum BouncyCollectibleMode
  {
  None,  // no collectible spawning on bounces
  OnEachBounce, // spawn collectibleSpawnCount collectibles at every direction change
  OnLastBounce, // spawn collectibleSpawnCount collectibles only at the final direction change
  }

  // Identity

  [Header("Identity")]
  [Tooltip("Display name shown in debug logs.")]
  public string hazardName = "Hazard";
  [Tooltip("Optional shared impact payload for direct hits from this hazard.")]
  public HazardImpactProfile impactProfile;
  [Tooltip("Optional visual and popup feedback profile for activation, explosion, bounce, and aftermath moments.")]
  public HazardFeedbackProfile feedbackProfile;

  // Hazard Type

  [Header("Hazard Type")]
  [Tooltip("Controls how this hazard moves and behaves.")]
  public HazardType hazardType = HazardType.Slam;

  // Sprites

  [Header("Sprites")]
  [Tooltip("Translucent sprite shown during approach and warning phases.")]
  public Sprite shadowSprite;
  [Tooltip("Fully-formed sprite shown when the hazard is active.")]
  public Sprite fullyFormedSprite;
  [Tooltip("Multiply tint applied to the sprite while the hazard is fully active. White = no tint.")]
  public Color tintColor = Color.white;

  // Warning Outline

  [Header("Warning Outline")]
  [Tooltip("Color of the pulsing outline during the warning phase.")]
  public Color outlineColor = Color.red;

  // Timing

  [Header("Timing")]
  [Min(0f)]
  [Tooltip("Seconds the hazard stays active after slamming. 0 = use DifficultyManager value.")]
  public float slamDuration = 0.4f;
  [Min(0f)]
  [Tooltip("Seconds to fade out after retracting. 0 = use DifficultyManager value.")]
  public float retractDuration = 0.5f;

  // Spawn Rotation

  [Header("Spawn Rotation")]
  [Tooltip("Randomise the Z rotation when this hazard is placed.")]
  public bool randomRotationOnSpawn = false;
  [Tooltip("Fixed Z rotation (degrees) when randomRotationOnSpawn is false.")]
  public float fixedSpawnRotation = 0f;

  // Crossing Settings

  [Header("Crossing Settings")]
  [Tooltip("Axis of travel for crossing-style hazards.")]
  public CrossingAxis crossingAxis = CrossingAxis.Horizontal;
  [Min(0.1f)]
  [Tooltip("World units per second.")]
  public float moveSpeed = 4f;
  [Min(0f)]
  [Tooltip("Seconds the lane indicator is shown before the hazard moves. If DifficultyManager.warningFlashDuration is above 0.05s it overrides this value; set it to 0 in DifficultyManager to let this field drive the warning.")]
  public float crossingWarningDuration = 0.6f;
  [Tooltip("Rotate the sprite to face the direction of travel.")]
  public bool rotatesToFaceDirection = false;
  [Range(1f, 20f)]
  [Tooltip("Speed of the outline pulse during the warning phase (cycles per second).")]
  public float warningPulseRate = 4f;
  [Min(0f)]
  [Tooltip("Pause at the spawn edge after the warning ends, before movement starts. 0 = immediate.")]
  public float entryDelay = 0f;
  [Tooltip("Speed multiplier curve over normalized travel progress (0 to 1). Default flat 1 = constant speed.")]
  public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
  [Min(0f)]
  [Tooltip("Seconds to pause after travel or last bounce completes before pooling. 0 = immediate.")]
  public float hitLingerDuration = 0f;

  // Crossing Variant

  [Header("Crossing Variant")]
  [Tooltip("Normal = smooth travel. Jump = hop arcs across the path.")]
  public CrossingVariant crossingVariant = CrossingVariant.Normal;
  [Range(1, 20)]
  [Tooltip("How many complete hops across the travel distance.")]
  public int jumpCount = 4;
  [Range(1f, 3f)]
  [Tooltip("Scale multiplier at the apex of each hop.")]
  public float jumpPeakScale = 1.5f;
  [Range(0.5f, 1f)]
  [Tooltip("Scale multiplier at touch-down (squash on landing).")]
  public float jumpLandScale = 0.8f;

  // Wavy Path Modifier

  [Header("Wavy Path")]
  [Tooltip("Sinusoidal lateral oscillation along the travel path. Stacks with Jump variant and Targeting.")]
  public bool enableWavyPath = false;
  [Min(0f)]
  [Tooltip("Peak lateral offset in world units.")]
  public float waveAmplitude = 1.5f;
  [Range(0.1f, 10f)]
  [Tooltip("Full sine cycles across the travel distance. Lower values = wider, gentler waves; higher = rapid tight oscillation.")]
  public float waveFrequency = 2f;

  // Bouncy Settings

  [Header("Bouncy Settings")]
  [Range(1, 20)]
  [Tooltip("Number of direction changes before the hazard pools.")]
  public int bounceCount = 3;
  [Min(0.5f)]
  [Tooltip("World-unit distance traveled between each direction change.")]
  public float bounceDistance = 3f;
  [Range(1f, 3f)]
  [Tooltip("Peak scale multiplier at the midpoint of each bounce segment. 1 = no size change. 1.3 = 30% bigger at the arc apex.")]
  public float bounceScalePeak = 1.2f;
  [Tooltip("Which movement patterns are eligible for this hazard. Add multiple entries to mix patterns across spawns.")]
  public List<BouncePatternType> bouncePatterns = new List<BouncePatternType> { BouncePatternType.FullyRandom };
  [Tooltip("How to pick from the patterns list each time this hazard spawns. Random chooses one at random. Sequential cycles through the list in order.")]
  public BouncePatternSelection bouncePatternSelection = BouncePatternSelection.Random;
  [Tooltip("When enabled, hit colliders are active only near the start and end of each bounce segment. Mid-arc the collider is off so the player can walk under it. Ground Window tunes how far into each segment it stays active.")]
  public bool hitOnlyWhenGrounded = false;
  [Range(0.01f, 0.49f)]
  [Tooltip("Fraction of the segment (0-0.5) that counts as grounded at each end. 0.15 = active for the first 15% and last 15% of every bounce segment.")]
  public float groundedWindow = 0.15f;
  // Targeting Modifier (works with any HazardType)

  [Header("Targeting")]
  [Tooltip("Slam: shadow drifts toward the player during approach and warning. Crossing/Bouncy continuously steers toward the player during travel.")]
  public bool enableTargeting = false;
  [Range(0f, 1f)]
  [Tooltip("Turn rate. 0 = no steer. 1 = instantly snaps to player direction each frame.")]
  public float trackingStrength = 0.4f;
  [Min(0f)]
  [Tooltip("Stops adjusting course when within this world-unit radius of the player.")]
  public float lockOnRadius = 0.5f;

  // Explosion Modifier (attach detonation to any HazardType)

  [Header("Explosion Modifier")]
  [Tooltip("Attach an explosion phase to this hazard. Uses the ExplosionEffect child on the prefab.")]
  public bool enableExplosion = false;
  [Tooltip("What condition triggers the explosion: impact, proximity, elapsed time, last bounce, or exit.")]
  public ExplosionTrigger explosionTrigger = ExplosionTrigger.OnImpact;
  [Min(0f)]
  [Tooltip("(OnProximity) World-unit radius around the player that triggers detonation.")]
  public float explosionProximityRadius = 1.5f;
  [Min(0f)]
  [Tooltip("(OnTimeElapsed) Seconds of active travel before detonation.")]
  public float explosionTimeDelay = 1.5f;
  [Min(0f)]
  [Tooltip("Seconds the explosion child GameObject is active (its sprite + colliders).")]
  public float explosionDuration = 0.3f;
  [Tooltip("Scale multiplier applied to the explosion child GameObject at detonation.")]
  public float explosionSpriteScale = 1.8f;

  // Crumb Interactions

  [Header("Collectible Interactions")]
  [Tooltip("Remove collectibles within range while this hazard is active. For Bouncy hazards with Hit Only When Grounded enabled, collectibles are only swept in the landing windows.")]
  public bool destroysNearbyCollectibles = false;
  [Range(0.5f, 5f)]
  [Tooltip("Collectible sweep radius as a multiplier of the primary hit collider's half-extent. 1 = matches collider size, 2 = double the collider footprint, etc.")]
  public float collectibleDestroyRadiusScale = 1f;
  [Tooltip("(Slam) Spawn collectibles when the hazard slams active. (Crossing) Spawn collectibles when the hazard exits off-screen. For travel-interval collectible spawning on Crossing use Crossing Collectible Mode.")]
  public bool spawnsCollectibles = false;
  [Min(0)]
  [Tooltip("Number of collectibles to spawn for spawnsCollectibles, Crossing Collectible Mode, and Bouncy Collectible Mode.")]
  public int collectibleSpawnCount = 3;

  [Header("Crossing Collectible Travel")]
  [Tooltip("Controls whether collectibles are spawned during Crossing travel: none, per distance, or per second.")]
  public CrossingCollectibleMode crossingCollectibleMode = CrossingCollectibleMode.None;
  [Min(0.1f)]
  [Tooltip("Distance (world units) or time (seconds) between collectible bursts during Crossing travel. Ignored when Crossing Collectible Mode is None.")]
  public float collectibleSpawnInterval = 1f;

  [Header("Bouncy Collectible Bounces")]
  [Tooltip("Controls whether collectibles are spawned at Bouncy direction changes: none, every bounce, or only the final bounce.")]
  public BouncyCollectibleMode bouncyCollectibleMode = BouncyCollectibleMode.None;

  // Audio

  [Header("Audio")]
  [Tooltip("(Slam) One-shot clip played the moment the hazard slams active.")]
  public AudioClip slamImpactClip;
  [Tooltip("(Bouncy) One-shot clip played each time the hazard reflects off a wall.")]
  public AudioClip bounceClip;  [Tooltip("(Crossing) One-shot clip played the instant the hazard launches from the edge.")]
  public AudioClip crossingEntryClip;
  [Tooltip("(Crossing) Looped clip played while the hazard is traveling across the screen. Stops automatically on exit.")]
  public AudioClip crossingTravelClip;
  [Tooltip("(Crossing) One-shot clip played when the hazard reaches the far edge and exits.")]
  public AudioClip crossingExitClip;  [Tooltip("(Any with enableExplosion) One-shot clip played when the explosion activates.")]
  public AudioClip explosionClip;
  [Range(0f, 1f)]
  [Tooltip("Master volume for all audio clips on this hazard type.")]
  public float audioVolume = 1f;

  // Screen Shake

  [Header("Screen Shake")]
  [Tooltip("Trigger a camera shake when this hazard slams active or explodes. Uses the canonical CameraShake service.")]
  public bool enableScreenShake = false;
  [Range(0f, 0.5f)]
  [Tooltip("Duration of the camera shake in seconds.")]
  public float shakeDuration = 0.15f;
  [Range(0f, 0.5f)]
  [Tooltip("Peak displacement of the shake in world units (dampens to zero over duration).")]
  public float shakeMagnitude = 0.12f;

  // Split on Bounce (Bouncy only)

  [Header("Split on Bounce")]
  [Tooltip("(Bouncy only) On the first wall bounce, retire this hazard and spawn two smaller copies deflected left and right of the current travel direction.")]
  public bool splitOnFirstBounce = false;
  [Range(0.3f, 0.9f)]
  [Tooltip("Local scale of each child hazard relative to the parent at the moment of split.")]
  public float splitChildScale = 0.6f;
  [Range(10f, 90f)]
  [Tooltip("Degrees each child is deflected from the parent direction. One child gets +angle, the other -angle.")]
  public float splitAngle = 45f;

  // Runtime

  [System.NonSerialized] private int _nextPatternIndex;

  /// <summary>
  /// Picks the BouncePatternType for this hazard spawn, advancing the sequential index if needed.
  /// Called once per activation from TravelBouncy.
  /// </summary>
  public BouncePatternType PickBouncePattern()
  {
  if (bouncePatterns == null || bouncePatterns.Count == 0)
  return BouncePatternType.FullyRandom;
  if (bouncePatterns.Count == 1)
  return bouncePatterns[0];

  if (bouncePatternSelection == BouncePatternSelection.Sequential)
  {
  BouncePatternType result = bouncePatterns[_nextPatternIndex % bouncePatterns.Count];
  _nextPatternIndex++;
  return result;
  }
  return bouncePatterns[Random.Range(0, bouncePatterns.Count)];
  }

#if UNITY_EDITOR
  private void OnValidate()
  {
  if (enableExplosion)
  {
  if (explosionTrigger == ExplosionTrigger.OnLastBounce && hazardType != HazardType.Bouncy)
  Debug.LogWarning($"[HazardData] '{hazardName}': OnLastBounce only fires on Bouncy hazards.", this);

  if ((explosionTrigger == ExplosionTrigger.OnProximity ||
  explosionTrigger == ExplosionTrigger.OnTimeElapsed) &&
  hazardType == HazardType.Slam)
  Debug.LogWarning($"[HazardData] '{hazardName}': {explosionTrigger} is only evaluated during Crossing/Bouncy travel; it will never fire on a Slam hazard.", this);
  }

  if (speedCurve != null && speedCurve.length >= 2 &&
  speedCurve[0].value <= 0f &&
  speedCurve[speedCurve.length - 1].value <= 0f)
  Debug.LogWarning($"[HazardData] '{hazardName}': speedCurve evaluates to near-zero - the hazard will barely move.", this);

  if (splitOnFirstBounce && hazardType != HazardType.Bouncy)
  Debug.LogWarning($"[HazardData] '{hazardName}': splitOnFirstBounce only applies to Bouncy hazards and will be ignored at runtime. Change Hazard Type to Bouncy, or uncheck splitOnFirstBounce.", this);

  if (splitOnFirstBounce && enableExplosion && explosionTrigger == ExplosionTrigger.OnLastBounce)
  Debug.LogWarning($"[HazardData] '{hazardName}': splitOnFirstBounce and OnLastBounce explosion are both enabled. The split fires on the first bounce and exits early, so the OnLastBounce explosion will never trigger. Disable one or the other.", this);
  }
#endif
}
}

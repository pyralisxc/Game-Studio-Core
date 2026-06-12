using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Custom Inspector for HazardData ScriptableObjects.
/// Shows only the fields relevant to the selected HazardType, reducing Inspector noise.
///
/// Section visibility rules:
///   Identity / Sprites / Warning Outline / Collectible Interactions  — always visible
///   Timing                        — Slam only
///   Spawn Rotation                — all types
///   Crossing Settings + Variant   — Crossing, Bouncy
///     Jump sub-fields              — when CrossingVariant == Jump
///     Wavy sub-fields             — when enableWavyPath == true
///   Bouncy Settings               — Bouncy only
///   Explosion Modifier            — always visible (modifier for any type)
///   Targeting                     — always visible (modifier for any type)
/// </summary>
[CustomEditor(typeof(HazardData))]
public class HazardDataEditor : Editor
{
    // ── Serialized properties ─────────────────────────────────────────────
    SerializedProperty _pName, _pType;
    SerializedProperty _pImpactProfile, _pFeedbackProfile;
    SerializedProperty _pShadow, _pFull;
    SerializedProperty _pOutlineColor;
    SerializedProperty _pSlamDur, _pRetractDur;
    SerializedProperty _pRandRot, _pFixedRot;
    SerializedProperty _pAxis, _pSpeed, _pWarnDur, _pRotateFace;
    SerializedProperty _pVariant, _pJumpCount, _pJumpPeak, _pJumpLand;
    SerializedProperty _pWaveAmp, _pWaveFreq, _pWavyPath;
    SerializedProperty _pPulseRate, _pEntryDelay, _pSpeedCurve, _pHitLinger;
    SerializedProperty _pBounceCount;
    SerializedProperty _pBounceDistance, _pBouncePatterns, _pBouncePatternSelection, _pBounceScalePeak;
    SerializedProperty _pTint;
    SerializedProperty _pEnableExplosion, _pExplosionTrigger, _pExplProxRadius, _pExplTimeDelay;
    SerializedProperty _pExplDur, _pExplScale;
    SerializedProperty _pEnableTracking, _pTrackStrength, _pLockRadius;
    SerializedProperty _pDestroyCollectibles, _pDestroyRadius;
    SerializedProperty _pSpawnCollectibles, _pSpawnCount;
    SerializedProperty _pSlamClip, _pBounceClip, _pExplClip, _pAudioVolume;
    SerializedProperty _pCrossingEntryClip, _pCrossingTravelClip, _pCrossingExitClip;
    SerializedProperty _pEnableShake, _pShakeDuration, _pShakeMagnitude;
    SerializedProperty _pSplitOnBounce, _pSplitScale, _pSplitAngle;
    SerializedProperty _pHitOnlyGrounded, _pGroundedWindow;
    SerializedProperty _pCrossingCollectibleMode, _pCollectibleInterval, _pBouncyCollectibleMode;

    bool _foldMovement = true, _foldVariant = true, _foldExplosion = true, _foldTargeting = true;

    void OnEnable()
    {
        _pName          = serializedObject.FindProperty("hazardName");
        _pImpactProfile = serializedObject.FindProperty("impactProfile");
        _pFeedbackProfile = serializedObject.FindProperty("feedbackProfile");
        _pType          = serializedObject.FindProperty("hazardType");
        _pShadow        = serializedObject.FindProperty("shadowSprite");
        _pFull          = serializedObject.FindProperty("fullyFormedSprite");
        _pOutlineColor  = serializedObject.FindProperty("outlineColor");
        _pSlamDur       = serializedObject.FindProperty("slamDuration");
        _pRetractDur    = serializedObject.FindProperty("retractDuration");
        _pRandRot       = serializedObject.FindProperty("randomRotationOnSpawn");
        _pFixedRot      = serializedObject.FindProperty("fixedSpawnRotation");
        _pAxis          = serializedObject.FindProperty("crossingAxis");
        _pSpeed         = serializedObject.FindProperty("moveSpeed");
        _pWarnDur       = serializedObject.FindProperty("crossingWarningDuration");
        _pRotateFace    = serializedObject.FindProperty("rotatesToFaceDirection");
        _pVariant       = serializedObject.FindProperty("crossingVariant");
        _pJumpCount     = serializedObject.FindProperty("jumpCount");
        _pJumpPeak      = serializedObject.FindProperty("jumpPeakScale");
        _pJumpLand      = serializedObject.FindProperty("jumpLandScale");
        _pWaveAmp       = serializedObject.FindProperty("waveAmplitude");
        _pWaveFreq      = serializedObject.FindProperty("waveFrequency");
        _pWavyPath      = serializedObject.FindProperty("enableWavyPath");
        _pPulseRate     = serializedObject.FindProperty("warningPulseRate");
        _pEntryDelay    = serializedObject.FindProperty("entryDelay");
        _pSpeedCurve    = serializedObject.FindProperty("speedCurve");
        _pHitLinger     = serializedObject.FindProperty("hitLingerDuration");
        _pBounceCount   = serializedObject.FindProperty("bounceCount");
        _pBounceDistance         = serializedObject.FindProperty("bounceDistance");
        _pBouncePatterns         = serializedObject.FindProperty("bouncePatterns");
        _pBouncePatternSelection = serializedObject.FindProperty("bouncePatternSelection");
        _pBounceScalePeak        = serializedObject.FindProperty("bounceScalePeak");
        _pTint             = serializedObject.FindProperty("tintColor");
        _pEnableExplosion  = serializedObject.FindProperty("enableExplosion");
        _pExplosionTrigger = serializedObject.FindProperty("explosionTrigger");
        _pExplProxRadius   = serializedObject.FindProperty("explosionProximityRadius");
        _pExplTimeDelay    = serializedObject.FindProperty("explosionTimeDelay");
        _pExplDur       = serializedObject.FindProperty("explosionDuration");
        _pExplScale     = serializedObject.FindProperty("explosionSpriteScale");
        _pEnableTracking = serializedObject.FindProperty("enableTargeting");
        _pTrackStrength = serializedObject.FindProperty("trackingStrength");
        _pLockRadius    = serializedObject.FindProperty("lockOnRadius");
        _pDestroyCollectibles = serializedObject.FindProperty("destroysNearbyCollectibles");
        _pDestroyRadius = serializedObject.FindProperty("collectibleDestroyRadiusScale");
        _pSpawnCollectibles   = serializedObject.FindProperty("spawnsCollectibles");
        _pSpawnCount    = serializedObject.FindProperty("collectibleSpawnCount");

        _pSlamClip    = serializedObject.FindProperty("slamImpactClip");
        _pBounceClip  = serializedObject.FindProperty("bounceClip");
        _pExplClip    = serializedObject.FindProperty("explosionClip");
        _pAudioVolume = serializedObject.FindProperty("audioVolume");

        _pCrossingEntryClip  = serializedObject.FindProperty("crossingEntryClip");
        _pCrossingTravelClip = serializedObject.FindProperty("crossingTravelClip");
        _pCrossingExitClip   = serializedObject.FindProperty("crossingExitClip");

        _pEnableShake   = serializedObject.FindProperty("enableScreenShake");
        _pShakeDuration = serializedObject.FindProperty("shakeDuration");
        _pShakeMagnitude= serializedObject.FindProperty("shakeMagnitude");

        _pSplitOnBounce = serializedObject.FindProperty("splitOnFirstBounce");
        _pSplitScale    = serializedObject.FindProperty("splitChildScale");
        _pSplitAngle    = serializedObject.FindProperty("splitAngle");

        _pHitOnlyGrounded   = serializedObject.FindProperty("hitOnlyWhenGrounded");
        _pGroundedWindow    = serializedObject.FindProperty("groundedWindow");
        _pCrossingCollectibleMode = serializedObject.FindProperty("crossingCollectibleMode");
        _pCollectibleInterval     = serializedObject.FindProperty("collectibleSpawnInterval");
        _pBouncyCollectibleMode   = serializedObject.FindProperty("bouncyCollectibleMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
            "Guided Authoring: Hazard Data",
            "Hazard Data is the main authored shape for one hazard type: movement pattern, sprites, timing, targeting, explosion, collectibles, audio, shake, impact, and feedback.",
            whenToUse: new[]
            {
                "Use this for slam, crossing, bouncy, targeted, exploding, collectible-spawning, or split hazards.",
                "Create separate HazardData assets for each tuned hazard behavior."
            },
            createBefore: new[]
            {
                "Hazard prefab with the Hazard component and trigger colliders.",
                "HazardImpactProfile if contact damage/knockback/status should be shared.",
                "HazardFeedbackProfile if flashes or popups should be shared."
            },
            assignFirst: new[]
            {
                "Set Hazard Name and Hazard Type.",
                "Assign shadow/active sprites and any impact/feedback profiles.",
                "Configure the movement section for the selected type.",
                "Configure explosion, targeting, collectibles, audio, and shake only if needed."
            },
            safeToCustomize: new[]
            {
                "Colliders are configured on the prefab, not on this asset.",
                "Many modifier sections can stay disabled for simple hazards.",
                "DifficultyManager may override some warning/timing values depending on scene setup."
            },
            validation: new[]
            {
                "Prefab has Hazard runtime and trigger colliders wired.",
                "Selected hazard type has sensible movement values.",
                "Impossible modifier combinations are resolved."
            },
            manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Hazard_Difficulty_Setup.md")));

        var type    = (HazardData.HazardType)_pType.enumValueIndex;
        var variant = (HazardData.CrossingVariant)_pVariant.enumValueIndex;

        bool isSlam     = type == HazardData.HazardType.Slam;
        bool isCrossing = type == HazardData.HazardType.Crossing;
        bool isBouncy   = type == HazardData.HazardType.Bouncy;
        bool isJump     = variant == HazardData.CrossingVariant.Jump;

        // ── Identity ──────────────────────────────────────────────────────
        Section("Identity");
        Prop(_pName);
        Prop(_pImpactProfile, "Impact Profile", "Shared hazard payload for damage, knockback, targeting, and status effects.");
        Prop(_pFeedbackProfile, "Feedback Profile", "Optional authored aftermath profile for flash and popup feedback.");
        if (_pFeedbackProfile.objectReferenceValue != null)
        {
            EditorGUILayout.HelpBox(
                "Assign a HazardFeedbackRuntime to the hazard prefab when using a Feedback Profile. " +
                "Add a SpriteFlasher too if your profile uses flash presets.",
                MessageType.Info);
        }

        // ── Hazard Type ───────────────────────────────────────────────────
        Section("Hazard Type");
        Prop(_pType);

        // ── Sprites ───────────────────────────────────────────────────────
        Section("Sprites");
        Prop(_pShadow, "Shadow Sprite",       "Translucent sprite during approach / warning.");
        Prop(_pFull,   "Active Sprite",       "Sprite shown when the hazard is fully active.");
        Prop(_pTint,   "Active Tint",         "Multiply tint on the sprite while fully active. White = no tint.");

        // ── Warning Outline ───────────────────────────────────────────────
        Section("Warning Outline");
        Prop(_pOutlineColor, "Outline Color", "Pulsing outline tint during the warning phase.");

        // ── Colliders (informational) ─────────────────────────────────────
        Section("Colliders");
        EditorGUILayout.HelpBox(
            "Colliders are configured on the prefab, not here.\n\n" +
            "Add any Collider2D type(s) to the prefab root or children, mark them Is Trigger, " +
            "then drag them into the Hazard component's  Hitboxes  list.\n\n" +
            "Multiple colliders = multiple simultaneous hitboxes (e.g. body + weapon).",
            MessageType.Info);

        // ── Timing (Slam only) ────────────────────────────────────────────
        if (isSlam)
        {
            Section("Timing");
            Prop(_pSlamDur,    "Active Duration (s)", "How long the hazard stays slammed. 0 = use DifficultyManager.");
            Prop(_pRetractDur, "Retract (s)",         "Fade-out time after impact. 0 = use DifficultyManager.");
            Prop(_pPulseRate,  "Pulse Rate",          "How fast the warning outline pulses during the warning phase (cycles/sec).");
        }

        // ── Spawn Rotation (all types) ────────────────────────────────────
        Section("Spawn Rotation");
        Prop(_pRandRot, "Random Z Rotation", "Rotate to a random Z angle on spawn.");
        if (!_pRandRot.boolValue)
        {
            EditorGUI.indentLevel++;
            Prop(_pFixedRot, "Fixed Z Angle (°)", "Z rotation applied when Random is off.");
            EditorGUI.indentLevel--;
        }

        // ── Crossing Settings ─────────────────────────────────────────────
        if (isCrossing)
        {
            Section("Crossing Settings");
            _foldMovement = EditorGUILayout.Foldout(_foldMovement, "Movement", true, EditorStyles.foldoutHeader);
            if (_foldMovement)
            {
                EditorGUI.indentLevel++;
                Prop(_pAxis,       "Axis",              "H, V, or Diagonal travel direction.");
                Prop(_pSpeed,      "Speed (u/s)",       "World units per second.");
                Prop(_pSpeedCurve, "Speed Curve",       "Evaluated each frame (x = travel 0→1). Multiplied against base Speed — flat 1 = constant, ramp up = accelerate.");
                Prop(_pWarnDur,    "Warning (s)",       "Lane-indicator time before the hazard moves.");
                Prop(_pPulseRate,  "Pulse Rate",        "How fast the warning outline pulses (cycles/sec).");
                Prop(_pEntryDelay, "Entry Delay (s)",   "Pause at spawn edge after warning ends, before movement starts.");
                Prop(_pRotateFace, "Rotate to Travel",  "Spin the sprite to face its travel direction.");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            _foldVariant = EditorGUILayout.Foldout(_foldVariant, "Movement Variant", true, EditorStyles.foldoutHeader);
            if (_foldVariant)
            {
                EditorGUI.indentLevel++;
                Prop(_pVariant, "Variant", "Normal = slide. Jump = hop arcs.");
                if (isJump)
                {
                    EditorGUI.indentLevel++;
                    Prop(_pJumpCount, "Hop Count",     "Complete hops across the full travel distance.");
                    Prop(_pJumpPeak,  "Peak Scale",    "Sprite scale at the top of each hop.");
                    Prop(_pJumpLand,  "Land Scale",    "Sprite scale on touch-down (squash).");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(2);
                Prop(_pWavyPath, "Wavy Path", "Layered sinusoidal oscillation along the travel direction. Stacks with Jump and Targeting.");
                if (_pWavyPath.boolValue)
                {
                    EditorGUI.indentLevel++;
                    Prop(_pWaveAmp,  "Amplitude (u)", "Peak lateral offset in world units.");
                    Prop(_pWaveFreq, "Frequency",     "Sine cycles across the full travel distance.");
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
            Prop(_pHitLinger, "Exit Linger (s)", "Pause after travel ends before pooling. 0 = return immediately.");
        }


        // ── Bouncy ────────────────────────────────────────────────────────
        if (isBouncy)
        {
            Section("Bouncy Settings");
            Prop(_pSpeed,                "Speed (u/s)",       "World units per second.");
            Prop(_pSpeedCurve,           "Speed Curve",       "Evaluated each frame (x = bounce 0→1). Multiplied against base Speed.");
            Prop(_pBounceCount,          "Bounce Count",      "Number of direction changes before the hazard pools.");
            Prop(_pBounceDistance,       "Bounce Distance (u)", "World-unit distance traveled between each direction change.");
            Prop(_pBounceScalePeak,      "Bounce Scale Peak",  "Scale multiplier at the midpoint of each segment arc. 1 = no scaling, 1.3 = 30% bigger at apex.");
            Prop(_pWarnDur,              "Warning (s)",       "Shadow approach + warning flash duration before the hazard launches.");
            Prop(_pPulseRate,            "Pulse Rate",        "How fast the warning outline pulses (cycles/sec).");
            EditorGUILayout.Space(4);
            Prop(_pBouncePatterns,       "Bounce Patterns",
                "Add any combination of pattern types. Multiple entries mix behaviour across spawns.\n\n" +
                "Fully Random    — any direction 0-360\u00b0, total chaos.\n" +
                "Aimed At Player — re-aims TOWARD the player at each bounce point.\n" +
                "                  Combine with Enable Targeting for continuous homing.\n" +
                "Diagonal        — strict 45\u00b0 family, flips one axis per bounce (billiard physics).\n" +
                "Ricochet        — deflects ~90\u00b0 left or right, never continues forward or reverses.\n" +
                "Flee From Player— re-aims AWAY from the player at each bounce point.\n" +
                "Zigzag          — alternates sharp left/right turns, sweeps the arena predictably.\n" +
                "Orbit           — consistent 90\u00b0 CW or CCW turn, circles the arena.");
            Prop(_pBouncePatternSelection, "Pattern Selection", "Random: pick one at random each spawn. Sequential: cycle through the list in order.");
            EditorGUILayout.Space(4);
            Prop(_pRotateFace,           "Rotate to Travel",  "Spin the sprite to face its travel direction.");
            Prop(_pHitLinger,            "Exit Linger (s)",   "Pause after all bounces complete before pooling.");

            EditorGUILayout.Space(2);
            Prop(_pWavyPath, "Wavy Path", "Layered sinusoidal oscillation along the travel direction.");
            if (_pWavyPath.boolValue)
            {
                EditorGUI.indentLevel++;
                Prop(_pWaveAmp,  "Amplitude (u)", "Peak lateral offset in world units.");
                Prop(_pWaveFreq, "Frequency",     "Sine cycles per segment.");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
            Prop(_pHitOnlyGrounded, "Hit Only When Grounded",
                "Hitbox is active only in the landing windows at the start and end of each bounce arc. " +
                "Mid-arc the collider is off so the player can pass underneath.");
            if (_pHitOnlyGrounded.boolValue)
            {
                EditorGUI.indentLevel++;
                Prop(_pGroundedWindow, "Grounded Window",
                    "Fraction of the segment (0–0.5) that counts as grounded at each end. " +
                    "0.15 = active for the first and last 15% of every arc.");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
            Section("Split on Bounce");
            Prop(_pSplitOnBounce, "Split on First Bounce",
                "On the first direction change, retire this hazard and spawn two smaller copies deflected left and right.");
            if (_pSplitOnBounce.boolValue)
            {
                EditorGUI.indentLevel++;
                Prop(_pSplitScale, "Child Scale",     "Local scale of each child relative to the parent at split.");
                Prop(_pSplitAngle, "Split Angle (°)", "Degrees each child is deflected from the parent direction.");
                EditorGUI.indentLevel--;
            }
        }

        // ── Explosion Modifier (any type) ─────────────────────────────────────
        Section("Explosion Modifier");
        _foldExplosion = EditorGUILayout.Foldout(_foldExplosion, "Explosion", true, EditorStyles.foldoutHeader);
        if (_foldExplosion)
        {
            EditorGUI.indentLevel++;
            Prop(_pEnableExplosion, "Enable Explosion",
                "Attach a detonation phase to this hazard. Requires an ExplosionEffect child GO wired in the Hazard component.");
            if (_pEnableExplosion.boolValue)
            {
                EditorGUI.indentLevel++;
                Prop(_pExplosionTrigger, "Trigger",
                    "OnImpact: Slam — detonates at activation; Crossing/Bouncy — detonates on contact with Player or another Hazard.\n" +
                    "OnProximity: detonate within Proximity Radius of player.\n" +
                    "OnTimeElapsed: detonate after Time Delay seconds of active travel.\n" +
                    "OnLastBounce: Bouncy — on the final wall bounce.\n" +
                    "OnExit: detonate just before pooling.");

                var trigger = (HazardData.ExplosionTrigger)_pExplosionTrigger.enumValueIndex;
                if (trigger == HazardData.ExplosionTrigger.OnLastBounce && type != HazardData.HazardType.Bouncy)
                    EditorGUILayout.HelpBox("OnLastBounce only fires on Bouncy hazards.", MessageType.Warning);
                if (trigger == HazardData.ExplosionTrigger.OnImpact && isSlam)
                    EditorGUILayout.HelpBox("OnImpact fires at slam activation (before slamDuration wait).", MessageType.Info);
                if (trigger == HazardData.ExplosionTrigger.OnImpact && !isSlam)
                    EditorGUILayout.HelpBox("OnImpact fires on contact with the Player or another Hazard during travel.", MessageType.Info);
                if ((trigger == HazardData.ExplosionTrigger.OnProximity || trigger == HazardData.ExplosionTrigger.OnTimeElapsed) && isSlam)
                    EditorGUILayout.HelpBox("This trigger is only evaluated during Crossing/Bouncy travel — it will never fire on a Slam hazard.", MessageType.Warning);
                if (trigger == HazardData.ExplosionTrigger.OnProximity)
                    Prop(_pExplProxRadius, "Proximity Radius", "World-unit radius from the player that triggers detonation.");
                if (trigger == HazardData.ExplosionTrigger.OnTimeElapsed)
                    Prop(_pExplTimeDelay, "Time Delay (s)", "Seconds of active travel before detonation.");

                EditorGUILayout.Space(2);
                Prop(_pExplDur,   "Duration (s)",    "Seconds the explosion child GO stays active.");
                Prop(_pExplScale, "Scale Multiplier", "Scale applied to the explosion child at detonation.");
                EditorGUILayout.HelpBox(
                    "Explosion visuals + hitbox live on a child GO wired into\n" +
                    "Hazard._explosionEffect. Add child 'ExplosionEffect', set inactive,\n" +
                    "give it SpriteRenderer + Collider2D(s), wire it in the Hazard Inspector.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        // ── Targeting (always visible — modifier for any type) ─────────────
        Section("Targeting");
        _foldTargeting = EditorGUILayout.Foldout(_foldTargeting, "Targeting Modifier", true, EditorStyles.foldoutHeader);
        if (_foldTargeting)
        {
            EditorGUI.indentLevel++;
            Prop(_pEnableTracking, "Enable Targeting",
                "Slam: shadow drifts toward player during approach + warning.\n" +
                "Crossing/Bouncy: steers toward player during travel.");
            if (_pEnableTracking.boolValue)
            {
                EditorGUI.indentLevel++;
                if (isSlam)
                    Prop(_pSpeed, "Drift Speed (u/s)", "How fast the shadow drifts toward the player during approach + warning. Scaled by Track Strength.");
                Prop(_pTrackStrength, "Track Strength", "Steer rate per second (0 = off, 1 = hard lock).");
                Prop(_pLockRadius,    "Lock-On Radius", "Stops adjusting when within this radius of the player.");
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        // ── Crumb Interactions ─────────────────────────────────────────────
        Section("Collectible Interactions");
        Prop(_pDestroyCollectibles, "Destroy Collectibles",
            "Remove nearby collectibles while active. Bouncy: when Hit Only When Grounded is on, sweeping only happens in landing windows.");
        if (_pDestroyCollectibles.boolValue)
        {
            EditorGUI.indentLevel++;
            Prop(_pDestroyRadius, "Destroy Scale (\u00d7collider)", "Collectible sweep radius as a multiplier of the primary hit collider's half-extent.");
            EditorGUI.indentLevel--;
        }
        Prop(_pSpawnCollectibles, "Spawn Collectibles",
            "Slam: drop collectibles at activation. Crossing: drop collectibles on exit. Use Travel Collectible Mode for per-distance/per-second spawning during Crossing travel.");
        if (_pSpawnCollectibles.boolValue)
        {
            EditorGUI.indentLevel++;
            Prop(_pSpawnCount, "Spawn Count", "Collectibles spawned per burst (shared by Spawn Collectibles, Travel Collectible Mode, and Bounce Collectible Mode).");
            EditorGUI.indentLevel--;
        }

        if (isCrossing)
        {
            EditorGUILayout.Space(2);
            Prop(_pCrossingCollectibleMode, "Travel Collectible Mode",
                "None: no collectibles during travel.\nPerDistance: one burst every Collectible Interval world units.\nPerSecond: one burst every Collectible Interval seconds.");
            var collectibleMode = (HazardData.CrossingCollectibleMode)_pCrossingCollectibleMode.enumValueIndex;
            if (collectibleMode != HazardData.CrossingCollectibleMode.None)
            {
                EditorGUI.indentLevel++;
                Prop(_pCollectibleInterval, "Collectible Interval",
                    collectibleMode == HazardData.CrossingCollectibleMode.PerDistance
                        ? "World units between each collectible burst."
                        : "Seconds between each collectible burst.");
                EditorGUI.indentLevel--;
            }
        }

        if (isBouncy)
        {
            EditorGUILayout.Space(2);
            Prop(_pBouncyCollectibleMode, "Bounce Collectible Mode",
                "None: no collectibles on bounces.\nOnEachBounce: spawn Spawn Count collectibles at every direction change.\nOnLastBounce: spawn collectibles only at the final direction change.");
        }
        // ── Audio ──────────────────────────────────────────────────────────────
        Section("Audio");
        if (isSlam)
            Prop(_pSlamClip, "Slam Impact Clip", "Played at the moment the hazard slams active.");
        if (isBouncy)
            Prop(_pBounceClip, "Bounce Clip", "Played each time the hazard reflects off a wall.");
        if (isCrossing)
        {
            Prop(_pCrossingEntryClip,  "Entry Clip",  "One-shot played the instant the hazard launches from the edge.");
            Prop(_pCrossingTravelClip, "Travel Clip", "Looped while the hazard crosses the screen. Stops automatically on exit.");
            Prop(_pCrossingExitClip,   "Exit Clip",   "One-shot played when the hazard reaches the far edge.");
        }
        if (_pEnableExplosion.boolValue)
            Prop(_pExplClip, "Explosion Clip", "Played when the explosion activates.");
        Prop(_pAudioVolume, "Volume", "Master volume for all clips on this hazard type.");

        // ── Screen Shake ──────────────────────────────────────────────────────
        Section("Screen Shake");
        Prop(_pEnableShake, "Enable Shake",
            "Slam: shake on impact.\nCrossing: shake on entry launch.\nBouncy: shake on launch.\nUses the canonical CameraShake service.");
        if (_pEnableShake.boolValue)
        {
            EditorGUI.indentLevel++;
            Prop(_pShakeDuration,  "Duration (s)",  "How long the shake lasts.");
            Prop(_pShakeMagnitude, "Magnitude (u)", "Peak world-unit displacement at shake start.");
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static void Prop(SerializedProperty p, string label = null, string tooltip = null)
    {
        if (label != null)
            EditorGUILayout.PropertyField(p, new GUIContent(label, tooltip ?? p.tooltip));
        else
            EditorGUILayout.PropertyField(p);
    }

    static void Section(string text)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        Rect r = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(r, new Color(0.45f, 0.45f, 0.45f, 0.5f));
        EditorGUILayout.Space(2);
    }
}

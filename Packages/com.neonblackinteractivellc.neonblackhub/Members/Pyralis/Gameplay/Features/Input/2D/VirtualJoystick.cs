using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Dynamic floating virtual joystick.
/// The joystick spawns wherever the thumb first touches inside the activation zone,
/// then tracks finger movement and disappears on lift.
/// Direction is always relative to the spawn point, so screen position doesn't affect input.
///
/// Setup:
///   1. In the Canvas create an empty Panel sized to the joystick side of the screen.
///      This is the Activation Zone; set its Image alpha to 0 (invisible but raycast-able).
///      Add UIOrientationHandler to reposition it per orientation.
///      Drag into "Activation Zone" field below.
///   2. Create a separate GameObject in the Canvas (NOT inside the panel).
///      Attach this VirtualJoystick script to it. This is the Joystick Container.
///   3. Add a child Image for the background circle.
///      Add a grandchild Image for the knob.
///      Wire both into the fields below.
///   4. Wire the parent Canvas into the "Canvas" field.
///
/// The container starts invisible (alpha 0) and pops into view at the touch point.
/// </summary>
public class VirtualJoystick : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The invisible Panel that defines where touches activate the joystick.\n" +
        "Resize and reposition this panel to change the joystick zone.\n" +
        "Add UIOrientationHandler to it for portrait/landscape variants.")]
    private RectTransform _activationZone;
    [SerializeField, Tooltip("RectTransform of the joystick background circle (child of this GameObject).\n" +
        "Set Anchor and Pivot to centre (0.5, 0.5). Raycast Target can be OFF; input is handled by raw touch.")]
    private RectTransform _background;
    [SerializeField, Tooltip("RectTransform of the movable knob (child of Background).\n" +
        "Anchor and Pivot both centre (0.5, 0.5). Raycast Target OFF.")]
    private RectTransform _knob;
    [SerializeField, Tooltip("The Canvas that contains this joystick. Required to convert screen positions to canvas local coords.")]
    private Canvas _canvas;

    [Header("Settings")]
    [SerializeField, Tooltip("How far in UI pixels the knob can travel from the spawn centre before it is clamped.\n" +
        "Match this to the visual radius of your background circle art.\n" +
        "Too small = unresponsive. Too large = knob flies outside the art.")]
    private float _maxKnobRadius = 60f;

    // Runtime state

    private CanvasGroup  _canvasGroup;
    private RectTransform _containerRect;
    private Vector2      _inputDirection;
    private int          _activeTouchId = -1;

    /// <summary>Normalised movement direction. Zero when the joystick is not held.</summary>
    public Vector2 Direction => _inputDirection;

    /// <summary>True while a finger is actively holding the joystick.</summary>
    public bool IsActive => _activeTouchId != -1;

    /// <summary>
    /// Reassigns which screen panel activates this joystick.
    /// Called by PlayerInputHandler when the Swap Controls setting changes.
    /// If a touch is in progress it is cancelled first to prevent a locked state.
    /// </summary>
    public void SetActivationZone(RectTransform zone)
    {
        Hide(); // cancel any in-flight touch before the zone changes
        _activationZone = zone;
    }

    // Unity Lifecycle

    private void Awake()
    {
        _containerRect = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Hide();

        if (_activationZone == null)
            Debug.LogWarning("[VirtualJoystick] No Activation Zone assigned. The joystick will never activate.");
        if (_canvas == null)
            Debug.LogWarning("[VirtualJoystick] No Canvas assigned. Joystick cannot convert screen coordinates.");
    }

    private void OnDisable()
    {
        // If disabled while a finger is held (e.g. game-over fires mid-swipe, settings panel
        // opens), clear the touch lock so IsActive and Direction report correctly on re-enable.
        Hide();
    }

    private void Update()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            UpdateTouch(touchscreen);
            return;
        }

#if UNITY_EDITOR
        UpdateMouse();
#endif
    }

    private void UpdateTouch(Touchscreen touchscreen)
    {
        bool activeFound = false;

        foreach (var touch in touchscreen.touches)
        {
            var phase = touch.phase.ReadValue();
            int id    = touch.touchId.ReadValue();
            Vector2 screenPos = touch.position.ReadValue();

            if (_activeTouchId == -1 && phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (_activationZone != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(_activationZone, screenPos, GetEventCamera()))
                {
                    _activeTouchId = id;
                    SpawnAt(screenPos);
                }
            }

            if (id == _activeTouchId)
            {
                if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    Hide();
                    // Use continue (not return) so remaining touches in this frame are
                    // the active touch lifted would otherwise be missed entirely.
                    continue;
                }
                UpdateKnob(screenPos);
                activeFound = true;
            }
        }

        // Safety net: if our tracked touch vanished without an Ended/Canceled event
        // (e.g. system interrupted, finger lifted between frames), clear the lock.
        if (_activeTouchId != -1 && !activeFound)
            Hide();
    }

#if UNITY_EDITOR
    private void UpdateMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        const int mouseId = 0;
        Vector2 screenPos = mouse.position.ReadValue();

        if (_activeTouchId == -1 && mouse.leftButton.wasPressedThisFrame)
        {
            if (_activationZone != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_activationZone, screenPos, GetEventCamera()))
            {
                _activeTouchId = mouseId;
                SpawnAt(screenPos);
            }
        }

        if (_activeTouchId == mouseId)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
                Hide();
            else
                UpdateKnob(screenPos);
        }
    }
#endif

    // Private Implementation

    private void SpawnAt(Vector2 screenPos)
    {
        if (_canvas == null) return;

        // Enforce centre pivot before positioning so anchoredPosition maps to the
        // visual centre of the joystick, not a corner/edge. A wrong pivot is the
        // most common cause of the joystick appearing above/below the touch point.
        _containerRect.pivot = new Vector2(0.5f, 0.5f);
        if (_background != null) _background.pivot = new Vector2(0.5f, 0.5f);
        if (_knob != null)       _knob.pivot       = new Vector2(0.5f, 0.5f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(), screenPos, GetEventCamera(), out Vector2 local);

        _containerRect.anchoredPosition = local;

        if (_background != null) _background.anchoredPosition = Vector2.zero;
        if (_knob != null)       _knob.anchoredPosition       = Vector2.zero;

        _inputDirection = Vector2.zero;
        _canvasGroup.alpha          = 1f;
        _canvasGroup.interactable   = false;
    }

    private void UpdateKnob(Vector2 screenPos)
    {
        if (_background == null || _knob == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background, screenPos, GetEventCamera(), out Vector2 local);

        // Subtract rect.center so this is pivot-agnostic (works for any pivot setting)
        local -= _background.rect.center;

        Vector2 clamped = Vector2.ClampMagnitude(local, _maxKnobRadius);
        _knob.anchoredPosition = clamped;
        _inputDirection        = clamped / _maxKnobRadius;
    }

    private void Hide()
    {
        _activeTouchId  = -1;
        _inputDirection = Vector2.zero;
        if (_knob != null) _knob.anchoredPosition = Vector2.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }

    private Camera GetEventCamera()
    {
        return _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _canvas.worldCamera
            : null;
    }
}
}

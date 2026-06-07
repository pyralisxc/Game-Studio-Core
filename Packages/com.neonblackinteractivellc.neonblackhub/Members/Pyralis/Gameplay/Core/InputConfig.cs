using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Wraps InputActionAsset so each game adapter can supply its own bindings.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Core/Input Config", fileName = "InputConfig")]
    public class InputConfig : ScriptableObject
    {
        public InputActionAsset actions;
    }
}

using UnityEngine;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Supported neutral input adapter for <see cref="Motor2D"/>.
/// Existing scenes may still use <see cref="PlayerInputHandler"/> as a migration shell.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Runtime 2D/Motor 2D Input Adapter")]
public class Motor2DInputAdapter : PlayerInputHandler
{
}
}

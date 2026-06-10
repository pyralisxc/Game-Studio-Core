using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using Unity.Netcode;
using UnityEngine;

namespace NeonBlack.Gameplay.Networking.Characters
{
    [RequireComponent(typeof(Motor3D))]
    public class NetworkMotor3D : NetworkBehaviour
    {
        private Motor3D _motor;
        private Pawn3DMovementComponent _movement;
        private Pawn3DInputModule _inputModule;

        private NetworkVariable<MovementStateSnapshot> _serverState = new NetworkVariable<MovementStateSnapshot>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly List<NetworkMovementInput> _inputBuffer = new List<NetworkMovementInput>();
        private uint _currentTick;
        private const int BufferSize = 1024;

        private void Awake()
        {
            _motor = GetComponent<Motor3D>();
            _movement = GetComponent<Pawn3DMovementComponent>();
            _inputModule = GetComponent<Pawn3DInputModule>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _serverState.Value = CaptureSnapshot(0);
            }

            if (IsClient && IsOwner)
            {
                _serverState.OnValueChanged += OnServerStateChanged;
            }
        }

        private void FixedUpdate()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                ProcessLocalInput();
            }

            if (IsServer)
            {
                _serverState.Value = CaptureSnapshot(_currentTick);
            }
            
            _currentTick++;
        }

        private void ProcessLocalInput()
        {
            if (_inputModule == null) return;

            // In a networked setup, we'd want a more deterministic SampleInput.
            // For this foundation, we'll leverage the existing module.
            var fi = _inputModule.CollectFrameInput();
            
            var input = new NetworkMovementInput
            {
                Move = fi.Move,
                SprintHeld = fi.SprintHeld,
                JumpPressed = fi.JumpPressed,
                JumpReleased = fi.JumpReleased,
                Tick = _currentTick
            };

            _inputBuffer.Add(input);
            if (_inputBuffer.Count > BufferSize) _inputBuffer.RemoveAt(0);

            SubmitInputServerRpc(input);
        }

        [ServerRpc]
        private void SubmitInputServerRpc(NetworkMovementInput input)
        {
            // Server-side validation and application would go here.
        }

        private void OnServerStateChanged(MovementStateSnapshot oldState, MovementStateSnapshot newState)
        {
            if (IsOwner && !IsServer)
            {
                Reconcile(newState);
            }
        }

        private void Reconcile(MovementStateSnapshot serverState)
        {
            _inputBuffer.RemoveAll(i => i.Tick <= serverState.Tick);

            // Basic reconciliation: Snap to server position.
            // A more advanced version would re-run the simulation.
            transform.position = serverState.Position;
        }

        private MovementStateSnapshot CaptureSnapshot(uint tick)
        {
            var state = _movement.State;
            return new MovementStateSnapshot
            {
                VelocityX = state.VelocityX,
                VelocityY = state.VelocityY,
                VelocityZ = state.VelocityZ,
                Position = transform.position,
                IsGrounded = state.IsGrounded,
                IsActing = state.IsActing,
                JumpsUsed = state.JumpsUsed,
                Tick = tick
            };
        }
    }
}
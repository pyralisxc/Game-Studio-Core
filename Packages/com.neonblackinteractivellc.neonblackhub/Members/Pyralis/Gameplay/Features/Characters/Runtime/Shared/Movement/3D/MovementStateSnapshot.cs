using System;
using Unity.Netcode;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    // Snapshot of the movement state for networking.
    [Serializable]
    public struct MovementStateSnapshot : INetworkSerializable
    {
        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;
        public Vector3 Position;
        public bool IsGrounded;
        public bool IsActing;
        public int JumpsUsed;
        public uint Tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref VelocityX);
            serializer.SerializeValue(ref VelocityY);
            serializer.SerializeValue(ref VelocityZ);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref IsGrounded);
            serializer.SerializeValue(ref IsActing);
            serializer.SerializeValue(ref JumpsUsed);
            serializer.SerializeValue(ref Tick);
        }
    }

    [Serializable]
    public struct NetworkMovementInput : INetworkSerializable
    {
        public Vector2 Move;
        public bool SprintHeld;
        public bool JumpPressed;
        public bool JumpReleased;
        public uint Tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Move);
            serializer.SerializeValue(ref SprintHeld);
            serializer.SerializeValue(ref JumpPressed);
            serializer.SerializeValue(ref JumpReleased);
            serializer.SerializeValue(ref Tick);
        }
    }
}
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorFacingMirrorTarget
    {
        void MirrorToSide(Transform root, bool faceRight);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    public interface ICharacterController
    {
        void UpdateRotation(ref Quaternion currentRotation, float deltaTime);
        void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);
        void BeforeCharacterUpdate(float deltaTime);
        void PostGroundingUpdate(float deltaTime);
        void AfterCharacterUpdate(float deltaTime);
        bool IsColliderValidForCollisions(Collider collider);
        void OnGroundHit(Collider collider, Vector3 normal, Vector3 point, ref HitStabilityReport hitStabilityReport);
        void OnDiscreteCollisionDetected(Collider collider);
    }
}
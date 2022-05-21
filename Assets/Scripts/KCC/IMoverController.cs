using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    public interface IMoverController
    {
        void UpdateMovement(out Vector3 targetPosition, out Quaternion targetRotation, float deltaTime);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    [Serializable]
    public struct PhysicsMoverState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }
    
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsMover : MonoBehaviour
    {
        public Rigidbody rigidbody;
        public bool moveWithPhysics = true;

        [NonSerialized]
        public IMoverController moverController;
        [NonSerialized]
        public Vector3 latestInterpolationPosition;
        [NonSerialized]
        public Quaternion latestInterpolationRotation;
        [NonSerialized]
        public Vector3 positionDeltaFromInterpolation;
        [NonSerialized]
        public Quaternion rotationDeltaFromInterpolation;

        public int indexInCharacterSystem { get; set; }
        public Vector3 velocity { get; protected set; }
        public Vector3 angularVelocity { get; protected set; }
        public Vector3 initialTickPosition { get; set; }
        public Quaternion initialTickRotation { get; set; }
        public Vector3 initialSimulationPosition { get; private set; }
        public Quaternion initialSimulationRotation { get; private set; }

        public Vector3 transientPosition
        {
            get { return _transientPosition; }
            private set { _transientPosition = value; }
        }
        private Vector3 _transientPosition;
        public Quaternion transientRotation
        {
            get { return _transientRotation; }
            private set { _transientRotation = value; }
        }
        private Quaternion _transientRotation;

        private void OnEnable()
        {
            KinematicCharacterSystem.Instance.RegisterPhysicsMover(this);
        }
        private void OnDisable()
        {
            KinematicCharacterSystem.Instance.UnregisterPhysicsMover(this);
        }
        private void Awake()
        {
            transientPosition = rigidbody.position;
            transientRotation = rigidbody.rotation;
            initialSimulationPosition = rigidbody.position;
            initialSimulationRotation = rigidbody.rotation;
            latestInterpolationPosition = rigidbody.position;
            latestInterpolationRotation = rigidbody.rotation;
        }
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            rigidbody.position = position;
            initialSimulationPosition = position;
            transientPosition = position;
        }
        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            rigidbody.rotation = rotation;
            initialSimulationRotation = rotation;
            transientRotation = rotation;
        }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            rigidbody.position = position;
            rigidbody.rotation = rotation;
            initialSimulationPosition = position;
            initialSimulationRotation = rotation;
            transientPosition = position;
            transientRotation = rotation;
        }
        public PhysicsMoverState GetState()
        {
            var state = new PhysicsMoverState();
            state.position = transientPosition;
            state.rotation = transientRotation;
            state.velocity = velocity;
            state.angularVelocity = angularVelocity;
            return state;
        }
        public void SetState(PhysicsMoverState state)
        {
            SetPositionAndRotation(state.position, state.rotation);
            velocity = state.velocity;
            angularVelocity = state.angularVelocity;
        }
        public void UpdateVelocity(float deltaTime)
        {
            initialSimulationPosition = transientPosition;
            initialSimulationRotation = transientRotation;

            moverController.UpdateMovement(out _transientPosition, out _transientRotation, deltaTime);

            if (deltaTime > 0f)
            {
                velocity = (transientPosition - initialSimulationPosition) / deltaTime;
                var rotationFromCurrentToTarget = transientRotation * Quaternion.Inverse(initialSimulationRotation);
                angularVelocity = (Mathf.Deg2Rad * rotationFromCurrentToTarget.eulerAngles) / deltaTime;
            }
        }
    }
}

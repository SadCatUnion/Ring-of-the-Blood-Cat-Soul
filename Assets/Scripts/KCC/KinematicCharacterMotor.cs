using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    public enum RigidbodyInteractionType
    {
        None,
        Kinematic,
        SimulatedDynamic,
    }
    public enum StepHandlingMethod
    {
        None,
        Standard,
        Extra,
    }
    public struct OverlapResult
    {
        public Vector3 normal;
        public Collider collider;
        public OverlapResult(Vector3 normal, Collider collider)
        {
            this.normal = normal;
            this.collider = collider;
        }
    }
    public struct CharacterGroundingReport
    {
        public bool foundAnyGround;
        public bool isStableOnGround;
        public bool snappingPrevented;
        public Vector3 groundNormal;
        public Vector3 innerGroundNormal;
        public Vector3 outerGroundNormal;

        public Collider groundCollider;
        public Vector3 groundPoint;

        public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
        {
            foundAnyGround      = transientGroundingReport.foundAnyGround;
            isStableOnGround    = transientGroundingReport.isStableOnGround;
            snappingPrevented   = transientGroundingReport.snappingPrevented;
            groundNormal        = transientGroundingReport.groundNormal;
            innerGroundNormal   = transientGroundingReport.innerGroundNormal;
            outerGroundNormal   = transientGroundingReport.outerGroundNormal;

            groundCollider      = null;
            groundPoint         = Vector3.zero;
        }
    }
    public struct CharacterTransientGroundingReport
    {
        public bool foundAnyGround;
        public bool isStableOnGround;
        public bool snappingPrevented;
        public Vector3 groundNormal;
        public Vector3 innerGroundNormal;
        public Vector3 outerGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            foundAnyGround      = groundingReport.foundAnyGround;
            isStableOnGround    = groundingReport.isStableOnGround;
            snappingPrevented   = groundingReport.snappingPrevented;
            groundNormal        = groundingReport.groundNormal;
            innerGroundNormal   = groundingReport.innerGroundNormal;
            outerGroundNormal   = groundingReport.outerGroundNormal;
        }
    }

    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterMotor : MonoBehaviour
    {
        [Header("Components")]
        public CapsuleCollider capsuleCollider;

        [Header("Capsule Settings")]
        [SerializeField]
        private PhysicMaterial capsulePhysicsMaterial;
        [SerializeField]
        private float capsuleYOffset = 1f;
        [SerializeField]
        private float capsuleRadius = 0.5f;
        [SerializeField]
        private float capsuleHeight = 2f;

        [Header("Grounding Settings")]
        public float groundDetectionExtraDistance = 0f;
        [Range(0f, 89f)]
        public float maxSlopeAngle = 60f;
        public LayerMask groundLayers = -1;
        public bool discreteCollisionEvents = false;

        [Header("Step Settings")]
        public StepHandlingMethod stepHandling = StepHandlingMethod.Standard;
        public float maxStepHeight = 0.5f;
        public bool allowSteppingWithoutStableGrounding = false;
        public float minRequiredStepDepth = 0.1f;

        [Header("Ledge Settings")]
        public bool ledgeHandling = true;
        public float maxDistanceFromLedge = 0.5f;
        public float maxVelocityForLedgeSnap = 0f;
        [Range(1f, 180f)]
        public float maxDenivelationAngle = 180f;

        [Header("Rigidbody Interaction Settings")]
        [Header("Constraints Settings")]
        public bool hasPlanarConstraint = false;
        public Vector3 planarConstraintAxis = Vector3.forward;

        [Header("Other Settings")]
        public int maxMovementIterations = 5;
        public int maxDecollisionIterations = 1;
        public bool checkMovementInitialOverlaps = true;
        public bool killVelocityWhenExceedMaxMovementIterations = true;
        public bool killRemainingMovementWhenExceedMaxMovementIterations = true;

        [NonSerialized]
        public CharacterGroundingReport groundingStatus = new CharacterGroundingReport();
        [NonSerialized]
        public CharacterTransientGroundingReport lastGroundingStatus = new CharacterTransientGroundingReport();
        [NonSerialized]
        public LayerMask collidableLayers = -1;

        public Vector3 transientPosition { get { return _transientPosition; } }
        public Quaternion transientRotation
        {
            get { return _transientRotation; }
            private set
            {
                _transientRotation  = value;
                _characterUp        = _transientRotation * Vector3.up;
                _characterForward   = _transientRotation * Vector3.forward;
                _characterRight     = _transientRotation * Vector3.right;
            }
        }
        public Vector3 velocity { get { return baseVelocity + _attachedRigidbodyVelocity; } }
        public Vector3 characterUp { get { return _characterUp; } }
        public Vector3 characterForward { get { return _characterForward; } }
        public Vector3 characterRight { get { return _characterRight; } }

        private Vector3 _transientPosition;
        private Quaternion _transientRotation;
        private Vector3 _characterUp;
        private Vector3 _characterForward;
        private Vector3 _characterRight;
        private Vector3 _initialSimulationPosition;
        private Quaternion _initialSimulationRotation;
        private Rigidbody _attachedRigidbody;
        private Vector3 _attachedRigidbodyVelocity;
        private Vector3 _characterTransformToCapsuleTop;
        private Vector3 _characterTransformToCapsuleTopHemi;
        private Vector3 _characterTransformToCapsuleCenter;
        private Vector3 _characterTransformToCapsuleBottomHemi;
        private Vector3 _characterTransformToCapsuleBottom;
        private int _overlapsCount;
        private List<OverlapResult> _overlaps = new List<OverlapResult>(maxRigidbodyOverlapsCount);

        [NonSerialized]
        public ICharacterController characterController;
        [NonSerialized]
        public bool lastMovementIterationFoundAnyGround;
        [NonSerialized]
        public int indexInCharacterSystem;
        [NonSerialized]
        public Vector3 initialTickPosition;
        [NonSerialized]
        public Quaternion initialTickRotation;
        [NonSerialized]
        public Rigidbody attachedRigidbodyOverride;
        [NonSerialized]
        public Vector3 baseVelocity;


        // Private
        
        private bool _solveMovementCollision = true;
        private bool _solveGrounding = true;
        private bool _movePositionDirty = false;
        private Vector3 _movePositionTarget = Vector3.zero;
        private bool _moveRotationDirty = false;
        private Quaternion _moveRotationTarget = Quaternion.identity;
        


        

        // Constants
        public const int maxRigidbodyOverlapsCount = 16;

        private void OnEnable()
        {
            KinematicCharacterSystem.Instance.RegisterCharactorMotor(this);
        }
        private void OnDisable()
        {
            KinematicCharacterSystem.Instance.UnregisterCharactorMotor(this);
        }

        public void SetCapsuleCollisionActive(bool value)
        {
            capsuleCollider.isTrigger = !value;
        }
        public void SetMovementCollisionSolvingActive(bool value)
        {
            _solveMovementCollision = value;
        }
        public void SetGroundSolvingActive(bool value)
        {
            _solveGrounding = value;
        }

        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            transform.position = position;
            _initialSimulationPosition = position;
            _transientPosition = position;
            if (bypassInterpolation)
            {
                initialTickPosition = position;
            }
        }
        public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
        {
            transform.rotation = rotation;
            _initialSimulationRotation = rotation;
            transientRotation = rotation;
            if (bypassInterpolation)
            {
                initialTickRotation = rotation;
            }
        }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
        {
            transform.SetPositionAndRotation(position, rotation);
            _initialSimulationPosition = position;
            _initialSimulationRotation = rotation;
            _transientPosition = position;
            transientRotation = rotation;
            if (bypassInterpolation)
            {
                initialTickPosition = position;
                initialTickRotation = rotation;
            }
        }

        public void MoveCharacter(Vector3 toPosition)
        {
            _movePositionDirty = true;
            _movePositionTarget = toPosition;
        }
        public void RotateCharacter(Quaternion toRotation)
        {
            _moveRotationDirty = true;
            _moveRotationTarget = toRotation;
        }
    }
}
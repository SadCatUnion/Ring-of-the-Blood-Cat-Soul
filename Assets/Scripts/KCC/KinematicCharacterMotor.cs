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
    [Serializable]
    public struct KinematicCharacterMotorState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 baseVelocity;

        public bool mustUnground;
        public float mustUngroundTime;
        public bool lastMovementIterationFoundAnyGround;
        public CharacterTransientGroundingReport groundingStatus;

        public Rigidbody attachedRigidbody;
        public Vector3 attachedRigidbodyVelocity;
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

        // todo: don't need these
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
        public float maxStableSlopeAngle = 60f;
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
        public bool interactiveRigidbodyHandling = true;

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

        public Vector3 transientPosition
        {
            get { return transientPosition; }
            private set { transientPosition = value; }
        }
        public Quaternion transientRotation
        {
            get { return _transientRotation; }
            private set
            {
                _transientRotation = value;
                characterUp        = _transientRotation * Vector3.up;
                characterForward   = _transientRotation * Vector3.forward;
                characterRight     = _transientRotation * Vector3.right;
            }
        }
        private Quaternion _transientRotation;
        public Vector3 velocity
        {
            get { return baseVelocity + attachedRigidbodyVelocity; }
        }
        public Vector3 characterUp
        {
            get { return characterUp; }
            private set { characterUp = value; }
        }
        public Vector3 characterForward
        {
            get { return characterForward; }
            private set { characterForward = value; }
        }
        public Vector3 characterRight
        {
            get { return characterRight; }
            private set { characterRight = value; }
        }
        public Vector3 initialSimulationPosition
        {
            get { return initialSimulationPosition; }
            private set { initialSimulationPosition = value; }
        }
        public Quaternion initialSimulationRotation
        {
            get { return initialSimulationRotation; }
            private set { initialSimulationRotation = value; }
        }
        public Rigidbody attachedRigidbody
        {
            get { return attachedRigidbody; }
            private set { attachedRigidbody = value; }
        }
        public Vector3 attachedRigidbodyVelocity
        {
            get { return attachedRigidbodyVelocity; }
            private set { attachedRigidbodyVelocity = value; }
        }
        public Vector3 characterTransformToCapsuleTop
        {
            get { return characterTransformToCapsuleTop; }
            private set { characterTransformToCapsuleTop = value; }
        }
        public Vector3 characterTransformToCapsuleTopHemi
        {
            get { return characterTransformToCapsuleTopHemi; }
            private set { characterTransformToCapsuleTopHemi = value; }
        }
        // public Vector3 characterTransformToCapsuleCenter
        // {
        //     get { return characterTransformToCapsuleCenter; }
        //     private set { characterTransformToCapsuleCenter = value; }
        // }
        public Vector3 characterTransformToCapsuleBottomHemi
        {
            get { return characterTransformToCapsuleBottomHemi; }
            private set { characterTransformToCapsuleBottomHemi = value; }
        }
        public Vector3 characterTransformToCapsuleBottom
        {
            get { return characterTransformToCapsuleBottom; }
            private set { characterTransformToCapsuleBottom = value; }
        }
        public int overlapsCount
        {
            get { return overlapsCount; }
            private set { overlapsCount = value; }
        }
        public readonly OverlapResult[] overlaps = new OverlapResult[maxRigidbodyOverlapsCount];


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
        private Collider[] probedColliders = new Collider[maxCollisionBudget];
        private List<Rigidbody> rigidbodiesPushedThisMove = new List<Rigidbody>(16);
        private bool solveMovementCollision = true;
        private bool solveGrounding = true;
        private bool movePositionDirty = false;
        private Vector3 movePositionTarget = Vector3.zero;
        private bool moveRotationDirty = false;
        private Quaternion moveRotationTarget = Quaternion.identity;
        private bool lastSolvedOverlapNormalDirty = false;
        private Vector3 lastSolvedOverlapNormal = Vector3.forward;
        private int rigidbodyProjectionHitCount = 0;
        private bool mustUnground = false;
        private float mustUngroundTimeCounter = 0f;
        

        // Constants
        public const int maxCollisionBudget = 16;
        public const int maxGroundingSweepIterations = 2;
        public const int maxSteppingSweepIterations = 3;
        public const int maxRigidbodyOverlapsCount = 16;
        public const float collisionOffset = 0.01f;
        public const float minGroundProbingDistance = 0.005f;
        public const float minVelocityMagnitude = 0.01f;

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
            solveMovementCollision = value;
        }
        public void SetGroundSolvingActive(bool value)
        {
            solveGrounding = value;
        }

        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            transform.position = position;
            initialSimulationPosition = position;
            transientPosition = position;
            if (bypassInterpolation)
            {
                initialTickPosition = position;
            }
        }
        public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
        {
            transform.rotation = rotation;
            initialSimulationRotation = rotation;
            transientRotation = rotation;
            if (bypassInterpolation)
            {
                initialTickRotation = rotation;
            }
        }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
        {
            transform.SetPositionAndRotation(position, rotation);
            initialSimulationPosition = position;
            initialSimulationRotation = rotation;
            transientPosition = position;
            transientRotation = rotation;
            if (bypassInterpolation)
            {
                initialTickPosition = position;
                initialTickRotation = rotation;
            }
        }

        public void MoveCharacter(Vector3 toPosition)
        {
            movePositionDirty = true;
            movePositionTarget = toPosition;
        }
        public void RotateCharacter(Quaternion toRotation)
        {
            moveRotationDirty = true;
            moveRotationTarget = toRotation;
        }

        public KinematicCharacterMotorState GetState()
        {
            KinematicCharacterMotorState state = new KinematicCharacterMotorState();
            state.position = transientPosition;
            state.rotation = transientRotation;
            state.baseVelocity = baseVelocity;
            state.attachedRigidbodyVelocity = attachedRigidbodyVelocity;
            state.mustUnground = mustUnground;
            state.mustUngroundTime = mustUngroundTimeCounter;
            state.lastMovementIterationFoundAnyGround = lastMovementIterationFoundAnyGround;
            state.groundingStatus.CopyFrom(groundingStatus);
            state.attachedRigidbody = attachedRigidbody;
            return state;
        }
        public void SetState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
        {
            SetPositionAndRotation(state.position, state.rotation, bypassInterpolation);
            baseVelocity = state.baseVelocity;
            attachedRigidbodyVelocity = state.attachedRigidbodyVelocity;
            mustUnground = state.mustUnground;
            mustUngroundTimeCounter = state.mustUngroundTime;
            lastMovementIterationFoundAnyGround = state.lastMovementIterationFoundAnyGround;
            groundingStatus.CopyFrom(state.groundingStatus);
            attachedRigidbody = state.attachedRigidbody;
        }

        public void SetCapsuleDimension(float radius, float height, float yOffset)
        {
            height = Mathf.Max(height, (radius * 2f) + 0.01f);

            capsuleRadius = radius;
            capsuleHeight = height;
            capsuleYOffset = yOffset;

            capsuleCollider.radius = capsuleRadius;
            capsuleCollider.height = capsuleHeight;
            // capsuleCollider.height = Mathf.Clamp(capsuleHeight, capsuleRadius * 2f, capsuleHeight);
            capsuleCollider.center = new Vector3(0f, capsuleYOffset, 0f);

            characterTransformToCapsuleTop = capsuleCollider.center + 0.5f * capsuleCollider.height * Vector3.up;
            characterTransformToCapsuleTopHemi = characterTransformToCapsuleTop + capsuleCollider.radius * Vector3.down;
            characterTransformToCapsuleBottom = capsuleCollider.center + 0.5f * capsuleCollider.height * Vector3.down;
            characterTransformToCapsuleBottomHemi = characterTransformToCapsuleBottom + capsuleCollider.radius * Vector3.up;
        }

        private void Awake()
        {
            transientPosition = transform.position;
            transientRotation = transform.rotation;

            collidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
                {
                    collidableLayers |= (1 << i);
                }
            }

            SetCapsuleDimension(capsuleRadius, capsuleHeight, capsuleYOffset);    
        }

        public void UpdatePhase1(float deltaTime)
        {
            if (float.IsNaN(baseVelocity.x) || float.IsNaN(baseVelocity.x) || float.IsNaN(baseVelocity.z))
            {
                baseVelocity = Vector3.zero;
            }
            if (float.IsNaN(attachedRigidbodyVelocity.x) || float.IsNaN(attachedRigidbodyVelocity.x) || float.IsNaN(attachedRigidbodyVelocity.z))
            {
                attachedRigidbodyVelocity = Vector3.zero;
            }
            
#if UNITY_EDITOR
            if (!Mathf.Approximately(transform.lossyScale.x, 1f) || !Mathf.Approximately(transform.lossyScale.y, 1f) || !Mathf.Approximately(transform.lossyScale.z, 1f))
            {
                Debug.LogError("Character's lossy scale is not (1,1,1). This is not allowed. Make sure the character's transform and all of its parents have a (1,1,1) scale.", this.gameObject);
            }
#endif
            rigidbodiesPushedThisMove.Clear();

            characterController.BeforeCharacterUpdate(deltaTime);

            transientPosition = transform.position;
            transientRotation = transform.rotation;
            initialSimulationPosition = transientPosition;
            initialSimulationRotation = transientRotation;
            rigidbodyProjectionHitCount = 0;
            overlapsCount = 0;
            lastSolvedOverlapNormalDirty = false;

            if (movePositionDirty)
            {
                if (solveMovementCollision)
                {
                    Vector3 tmpVelocity = GetVelocityFromMovement(movePositionTarget - transientPosition, deltaTime);
                    if (InternalCharacterMove(ref tmpVelocity, deltaTime))
                    {
                        if (interactiveRigidbodyHandling)
                        {
                            ProcessVelocityForRigidbodyHits(ref tmpVelocity, deltaTime);
                        }
                    }
                }
                else
                {
                    transientPosition = movePositionTarget;
                }
                movePositionDirty = false;
            }

            lastGroundingStatus.CopyFrom(groundingStatus);
            groundingStatus = new CharacterGroundingReport();
            groundingStatus.groundNormal = characterUp;

            if (solveMovementCollision)
            {
                Vector3 resolutionDirection = Vector3.up;
                float resolutionDistance = 0f;
                int iterationsMade = 0;
                bool overlapSolved = false;
                while (iterationsMade < maxDecollisionIterations && !overlapSolved)
                {
                    int nbOverlaps = CharacterCollisionOverlap(transientPosition, transientRotation, probedColliders);
                    if (nbOverlaps > 0)
                    {

                    }
                    else
                    {
                        overlapSolved = true;
                    }
                    iterationsMade++;
                }
            }

            if (solveGrounding)
            {
                if (MustUnground())
                {

                }
                else
                {

                }
                
            }
            lastMovementIterationFoundAnyGround = false;
            if (mustUngroundTimeCounter > 0f)
            {
                mustUngroundTimeCounter -= deltaTime;
            }
            mustUnground = false;

            if (solveGrounding)
            {
                characterController.PostGroundingUpdate(deltaTime);
            }

            if (interactiveRigidbodyHandling)
            {

            }
        }
        public void UpdatePhase2(float deltaTime)
        {
            characterController.UpdateRotation(ref _transientRotation, deltaTime);
            //transientRotation = _transientRotation;

            if (moveRotationDirty)
            {
                transientRotation = moveRotationTarget;
                moveRotationDirty = false;
            }

            if (solveMovementCollision && interactiveRigidbodyHandling)
            {

            }

            characterController.UpdateVelocity(ref baseVelocity, deltaTime);

            if (baseVelocity.magnitude < minVelocityMagnitude)
            {
                baseVelocity = Vector3.zero;
            }

            if (baseVelocity.sqrMagnitude > 0f)
            {
                if (solveMovementCollision)
                {
                    InternalCharacterMove(ref baseVelocity, deltaTime);
                }
                else
                {
                    transientPosition += deltaTime * baseVelocity;
                }
            }

            if (interactiveRigidbodyHandling)
            {
                ProcessVelocityForRigidbodyHits(ref baseVelocity, deltaTime);
            }

            if (hasPlanarConstraint)
            {
                transientPosition = initialSimulationPosition + Vector3.ProjectOnPlane(transientPosition - initialSimulationPosition, planarConstraintAxis.normalized);
            }

            if (discreteCollisionEvents)
            {
                int nbOverlaps = CharacterCollisionOverlap(transientPosition, transientRotation, probedColliders, 2f * collisionOffset);
                for (int i = 0; i < nbOverlaps; i++)
                {
                    characterController.OnDiscreteCollisionDetected(probedColliders[i]);
                }
            }

            characterController.AfterCharacterUpdate(deltaTime);
        }
        // IsStableOnPlane / IsStableOnSlope
        private bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(characterUp, normal) <= maxStableSlopeAngle;
        }
        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref CharacterGroundingReport groundingReport)
        {
            probingDistance = Mathf.Max(probingDistance, minGroundProbingDistance);
            int groundSweepsMade = 0;
            RaycastHit groundSweepHit = new RaycastHit();
            bool groundSweepingIsOver = false;
            Vector3 groundSweepPosition = probingPosition;
            Vector3 groundSweepDirection = atRotation * Vector3.down;
            float groundProbeDistanceRemaining = probingDistance;
            while (groundProbeDistanceRemaining > 0f && groundSweepsMade <= maxGroundingSweepIterations && !groundSweepingIsOver)
            {
                if (CharacterGroundSweep(groundSweepPosition, atRotation, groundSweepDirection, groundProbeDistanceRemaining, out groundSweepHit))
                {

                }
                else
                {
                    groundSweepingIsOver = true;
                }
                groundSweepsMade++;
            }
        }
        public bool MustUnground()
        {
            return mustUnground || mustUngroundTimeCounter > 0f;
        }
        private bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
        {
            return true;
        }
        private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {}
        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;
            return movement / deltaTime;
        }
        public int CharacterCollisionOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            return 1;
        }
        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = new RaycastHit();

            bool foundValidHit = false;
            return foundValidHit;
        }
    }
}
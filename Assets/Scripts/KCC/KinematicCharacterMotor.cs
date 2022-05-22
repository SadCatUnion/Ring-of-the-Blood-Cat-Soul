using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    #region Enum
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
    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }
    #endregion

    #region Struct
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
    public struct HitStabilityReport
    {
        public bool isStable;

        public bool foundInnerNormal;
        public Vector3 innerNormal;
        public bool foundOuterNormal;
        public Vector3 outerNormal;

        public bool validStepDetected;
        public Collider steppedCollider;

        public bool ledgeDetected;
        public bool isOnEmptySideOfLedge;
        public float distanceFromLedge;
        public bool isMovingTowardsEmptySideOfLedge;
        public Vector3 ledgeGroundNormal;
        public Vector3 ledgeRightDirection;
        public Vector3 ledgeFacingDirection;
    }
    public struct RigidbodyProjectionHit
    {
        public Rigidbody rigidbody;
        public Vector3 hitPoint;
        public Vector3 effectiveHitNormal;
        public Vector3 hitVelocity;
        public bool stableOnHit;
    }
    #endregion
    
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
        public LayerMask stableGroundLayers = -1;
        public bool discreteCollisionEvents = false;

        [Header("Step Settings")]
        public StepHandlingMethod stepHandling = StepHandlingMethod.Standard;
        public float maxStepHeight = 0.5f;
        public bool allowSteppingWithoutStableGrounding = false;
        public float minRequiredStepDepth = 0.1f;

        [Header("Ledge Settings")]
        public bool ledgeAndDeviationHandling = true;
        public float maxDistanceFromLedge = 0.5f;
        public float maxVelocityForLedgeSnap = 0f;
        [Range(1f, 180f)]
        public float maxDeviationAngle = 180f;

        [Header("Rigidbody Interaction Settings")]
        public bool interactiveRigidbodyHandling = true;
        public RigidbodyInteractionType rigidbodyInteractionType;

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

        public Vector3 transientPosition { get; private set; }
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
        public Vector3 characterUp { get; private set; }
        public Vector3 characterForward { get; private set; }
        public Vector3 characterRight { get; private set; }
        public Vector3 initialSimulationPosition { get; private set; }
        public Quaternion initialSimulationRotation { get; private set; }
        public Rigidbody attachedRigidbody { get; private set; }
        public Vector3 attachedRigidbodyVelocity { get; private set; }
        public Vector3 characterTransformToCapsuleTop { get; private set; }
        public Vector3 characterTransformToCapsuleTopHemi { get; private set; }
        // public Vector3 characterTransformToCapsuleCenter { get; private set; }
        public Vector3 characterTransformToCapsuleBottomHemi { get; private set; }
        public Vector3 characterTransformToCapsuleBottom { get; private set; }
        public int overlapsCount { get; private set; }
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


        #region Private
        private RaycastHit[] characterHits = new RaycastHit[maxHitsBudget];
        private Collider[] probedColliders = new Collider[maxCollisionBudget];
        private List<Rigidbody> rigidbodiesPushedThisMove = new List<Rigidbody>(16);
        private RigidbodyProjectionHit[] rigidbodyProjectionHits = new RigidbodyProjectionHit[maxRigidbodyOverlapsCount];
        private bool solveMovementCollision = true;
        private bool solveGrounding = true;
        private bool movePositionDirty = false;
        private Vector3 movePositionTarget = Vector3.zero;
        private bool moveRotationDirty = false;
        private Quaternion moveRotationTarget = Quaternion.identity;
        private bool lastSolvedOverlapNormalDirty = false;
        private Vector3 lastSolvedOverlapNormal = Vector3.forward;
        private int rigidbodyProjectionHitCount = 0;
        private bool isMovingFromAttachedRigidbody = false;
        private bool mustUnground = false;
        private float mustUngroundTimeCounter = 0f;
        #endregion

        #region Constants
        public const int maxHitsBudget = 16;
        public const int maxCollisionBudget = 16;
        public const int maxGroundingSweepIterations = 2;
        public const int maxSteppingSweepIterations = 3;
        public const int maxRigidbodyOverlapsCount = 16;
        public const float collisionOffset = 0.01f;
        public const float minGroundProbingDistance = 0.005f;
        public const float groundProbingBackstepDistance = 0.1f;
        public const float sweepProbingBackstepDistance = 0.002f;
        public const float minVelocityMagnitude = 0.01f;
        public const float steppingForwardDistance = 0.03f;
        public const float correlationForVerticalObstruction = 0.01f;
        #endregion

        private void OnEnable()
        {
            KinematicCharacterSystem.Instance.RegisterCharactorMotor(this);
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
                int nbOverlaps = CharacterCollisionsOverlap(transientPosition, transientRotation, probedColliders, 2f * collisionOffset);
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
        private bool IsStableWithSpecialClass(ref HitStabilityReport hitStabilityReport, Vector3 velocity)
        {
            if (ledgeAndDeviationHandling)
            {
                if (hitStabilityReport.ledgeDetected)
                {
                    if (hitStabilityReport.isMovingTowardsEmptySideOfLedge)
                    {
                        var velocityOnLedgeNormal = Vector3.Project(velocity, hitStabilityReport.ledgeFacingDirection);
                        if (velocityOnLedgeNormal.magnitude >= maxVelocityForLedgeSnap)
                        {
                            return false;
                        }
                    }
                    if (hitStabilityReport.isOnEmptySideOfLedge && hitStabilityReport.distanceFromLedge > maxDistanceFromLedge)
                    {
                        return false;
                    }
                }
                
                if (lastGroundingStatus.foundAnyGround && hitStabilityReport.innerNormal.sqrMagnitude != 0f && hitStabilityReport.outerNormal.sqrMagnitude != 0f)
                {
                    var deviationAngle = Vector3.Angle(hitStabilityReport.innerNormal, hitStabilityReport.outerNormal);
                    if (deviationAngle > maxDeviationAngle)
                    {
                        return false;
                    }
                    else
                    {
                        deviationAngle = Vector3.Angle(lastGroundingStatus.innerGroundNormal, hitStabilityReport.outerNormal);
                        if (deviationAngle > maxDeviationAngle)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
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
                    var targetPosition = groundSweepPosition + groundSweepDirection * groundSweepHit.distance;
                    var groundHitStabilityReport = new HitStabilityReport();
                    EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, transientRotation, baseVelocity, ref groundHitStabilityReport);
                
                    groundingReport.foundAnyGround = true;
                    groundingReport.groundNormal = groundSweepHit.normal;
                    groundingReport.innerGroundNormal = groundHitStabilityReport.innerNormal;
                    groundingReport.outerGroundNormal = groundHitStabilityReport.outerNormal;
                    groundingReport.groundCollider = groundSweepHit.collider;
                    groundingReport.groundPoint = groundSweepHit.point;
                    groundingReport.snappingPrevented = false;

                    if (groundHitStabilityReport.isStable)
                    {
                        groundingReport.snappingPrevented = !IsStableWithSpecialClass(ref groundHitStabilityReport, baseVelocity);
                        groundingReport.isStableOnGround = true;
                        if (!groundingReport.snappingPrevented)
                        {
                            probingPosition = groundSweepPosition + (groundSweepHit.distance - collisionOffset) * groundSweepDirection;
                        }
                        characterController.OnGroundHit(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, ref groundHitStabilityReport);
                        groundSweepingIsOver = true;
                    }
                    else
                    {
                        var sweepMovement = groundSweepHit.distance * groundSweepDirection + Mathf.Max(collisionOffset, groundSweepHit.distance) * (atRotation * Vector3.up);
                        groundSweepPosition += sweepMovement;

                        groundProbeDistanceRemaining = Mathf.Min(groundProbeDistanceRemaining, Mathf.Max(groundProbeDistanceRemaining - sweepMovement.magnitude, 0f));

                        groundSweepDirection = Vector3.ProjectOnPlane(groundSweepDirection, groundSweepHit.normal).normalized;
                    }
                }
                else
                {
                    groundSweepingIsOver = true;
                }
                groundSweepsMade++;
            }
        }
        public void ForceUnground(float time = 0.1f)
        {
            mustUnground = true;
            mustUngroundTimeCounter = time;
        }
        public bool MustUnground()
        {
            return mustUnground || mustUngroundTimeCounter > 0f;
        }
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 normal)
        {
            var directionRight = Vector3.Cross(direction, characterUp);
            return Vector3.Cross(normal, directionRight).normalized;
        }
        private bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            var wasCompleted = true;
            var remainingMovementDirection = transientVelocity.normalized;
            var remainingMovementMagnitude = transientVelocity.magnitude * deltaTime;
            var originalVelocityDirection = remainingMovementDirection;
            var sweepsMade = 0;
            var hitSomethingThisSweepIteration = true;
            var tmpMovedPosition = transientPosition;
            var previousHitIsStable = false;
            var previousVelocity = Vector3.zero;
            var previousObstructionNormal = Vector3.zero;
            var sweepState = MovementSweepState.Initial;

            for (int i = 0; i < overlapsCount; i++)
            {
                var overlapNormal = overlaps[i].normal;
                if (Vector3.Dot(remainingMovementDirection, overlapNormal) < 0f)
                {
                    var stableOnHit = IsStableOnNormal(overlapNormal) && !MustUnground();
                    var velocityBeforeProjection = transientVelocity;
                    var obstructionNormal = GetObstructionNormal(overlapNormal, stableOnHit);
                    InternalHandleVelocityProjection(
                        stableOnHit,
                        overlapNormal,
                        obstructionNormal,
                        originalVelocityDirection,
                        ref sweepState,
                        previousHitIsStable,
                        previousVelocity,
                        previousObstructionNormal,
                        ref transientVelocity,
                        ref remainingMovementMagnitude,
                        ref remainingMovementDirection
                    );
                    previousHitIsStable = stableOnHit;
                    previousVelocity = velocityBeforeProjection;
                    previousObstructionNormal = obstructionNormal;
                }
            }

            while (remainingMovementMagnitude > 0f && sweepsMade <= maxMovementIterations && hitSomethingThisSweepIteration)
            {
                var foundClosestHit = false;
                Vector3 closestSweepHitPoint = default;
                Vector3 closestSweepHitNormal = default;
                var closestSweepHitDistance = 0f;
                Collider closestSweepHitCollider = null;

                if (checkMovementInitialOverlaps)
                {
                    var overlapCount = CharacterCollisionsOverlap(
                        tmpMovedPosition,
                        transientRotation,
                        probedColliders
                    );
                    if (overlapCount > 0)
                    {
                        closestSweepHitDistance = 0f;
                        var mostObstructingOverlapNormalDotProduct = 2f;
                        for (int i = 0; i < overlapCount; i++)
                        {
                            var tmpCollider = probedColliders[i];
                            if (Physics.ComputePenetration(
                                capsuleCollider,
                                tmpMovedPosition,
                                transientRotation,
                                tmpCollider,
                                tmpCollider.transform.position,
                                tmpCollider.transform.rotation,
                                out var resolutionDirection,
                                out var resolutionDistance
                            ))
                            {
                                var dotProduct = Vector3.Dot(remainingMovementDirection, resolutionDirection);
                                if (dotProduct < 0f && dotProduct < mostObstructingOverlapNormalDotProduct)
                                {
                                    mostObstructingOverlapNormalDotProduct = dotProduct;
                                    closestSweepHitNormal = resolutionDirection;
                                    closestSweepHitCollider = tmpCollider;
                                    closestSweepHitPoint = tmpMovedPosition + transientRotation * capsuleCollider.center + resolutionDirection * resolutionDistance;
                                    if (!foundClosestHit)
                                    {
                                        foundClosestHit = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!foundClosestHit && CharacterCollisionsSweep(
                    tmpMovedPosition,
                    transientRotation,
                    remainingMovementDirection,
                    remainingMovementMagnitude + collisionOffset,
                    out var closestSweepHit,
                    characterHits
                ) > 0)
                {
                    closestSweepHitNormal = closestSweepHit.normal;
                    closestSweepHitDistance = closestSweepHit.distance;
                    closestSweepHitCollider = closestSweepHit.collider;
                    closestSweepHitPoint = closestSweepHit.point;
                    foundClosestHit = true;
                }
                
                if (foundClosestHit)
                {
                    var sweepMovement = remainingMovementDirection * Mathf.Max(0f, closestSweepHitDistance - collisionOffset);
                    tmpMovedPosition += sweepMovement;
                    remainingMovementMagnitude -= sweepMovement.magnitude;

                    var moveHitStabilityReport = new HitStabilityReport();
                    EvaluateHitStability(
                        closestSweepHitCollider,
                        closestSweepHitNormal,
                        closestSweepHitPoint,
                        tmpMovedPosition,
                        transientRotation,
                        transientVelocity,
                        ref moveHitStabilityReport
                    );

                    var foundValidStepHit = false;
                    if (solveGrounding && stepHandling != StepHandlingMethod.None && moveHitStabilityReport.validStepDetected)
                    {
                        var obstructionCorrelation = Mathf.Abs(Vector3.Dot(closestSweepHitNormal, characterUp));
                        if (obstructionCorrelation <= correlationForVerticalObstruction)
                        {
                            var stepForwardDirection = Vector3.ProjectOnPlane(-closestSweepHitNormal, characterUp).normalized;
                            var stepCastStartPoint = tmpMovedPosition + stepForwardDirection * steppingForwardDistance + characterUp * maxStepHeight;
                            var stepHitCount = CharacterCollisionsSweep(
                                stepCastStartPoint,
                                transientRotation,
                                -characterUp,
                                maxStepHeight,
                                out var closestStepHit,
                                characterHits,
                                0f,
                                true
                            );
                            for (int i = 0; i < stepHitCount; i++)
                            {
                                var hit = characterHits[i];
                                if (hit.collider == moveHitStabilityReport.steppedCollider)
                                {
                                    var endStepPosition = stepCastStartPoint + (-characterUp * (hit.distance - collisionOffset));
                                    tmpMovedPosition = endStepPosition;
                                    foundValidStepHit = true;

                                    transientVelocity = Vector3.ProjectOnPlane(transientVelocity, characterUp);
                                    remainingMovementDirection = transientVelocity.normalized;

                                    break;
                                }
                            }
                        }
                    }

                    if (!foundValidStepHit)
                    {
                        var obstructionNormal = GetObstructionNormal(closestSweepHitNormal, moveHitStabilityReport.isStable);

                        characterController.OnMovementHit(closestSweepHitCollider, closestSweepHitNormal, closestSweepHitPoint, ref moveHitStabilityReport);

                        if (interactiveRigidbodyHandling && closestSweepHitCollider.attachedRigidbody)
                        {
                            StoreRigidbodyHit(
                                closestSweepHitCollider.attachedRigidbody,
                                transientVelocity,
                                closestSweepHitPoint,
                                obstructionNormal,
                                moveHitStabilityReport
                            );
                        }

                        var stableOnHit = moveHitStabilityReport.isStable && !MustUnground();
                        var velocityBeforeProjection = transientVelocity;

                        InternalHandleVelocityProjection(
                            stableOnHit,
                            closestSweepHitNormal,
                            obstructionNormal,
                            originalVelocityDirection,
                            ref sweepState,
                            previousHitIsStable,
                            previousVelocity,
                            previousObstructionNormal,
                            ref transientVelocity,
                            ref remainingMovementMagnitude,
                            ref remainingMovementDirection
                        );
                        previousHitIsStable = stableOnHit;
                        previousVelocity = velocityBeforeProjection;
                        previousObstructionNormal = obstructionNormal;
                    }
                    
                }
                else
                {
                    hitSomethingThisSweepIteration = false;
                }

                sweepsMade++;
                if (sweepsMade > maxMovementIterations)
                {
                    if (killRemainingMovementWhenExceedMaxMovementIterations)
                    {
                        remainingMovementMagnitude = 0f;
                    }
                    if (killVelocityWhenExceedMaxMovementIterations)
                    {
                        transientVelocity = Vector3.zero;
                    }
                    wasCompleted = false;
                }
            }

            tmpMovedPosition += remainingMovementDirection * remainingMovementMagnitude;
            transientPosition = tmpMovedPosition;

            return wasCompleted;
        }
        private Vector3 GetObstructionNormal(Vector3 normal, bool stableOnHit)
        {
            var obstructionNormal = normal;
            if (groundingStatus.isStableOnGround && !MustUnground() && !stableOnHit)
            {
                var obstructionLeftAlongGround = Vector3.Cross(groundingStatus.groundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, characterUp).normalized;
            }
            if (obstructionNormal.sqrMagnitude == 0f)
            {
                obstructionNormal = normal;
            }
            return obstructionNormal;
        }
        private void StoreRigidbodyHit(Rigidbody rigidbody, Vector3 velocity, Vector3 point, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
        {
            if (rigidbodyProjectionHitCount < rigidbodyProjectionHits.Length)
            {
                if (!rigidbody.GetComponent<KinematicCharacterMotor>())
                {
                    var rigidbodyProjectionHit = new RigidbodyProjectionHit();
                    rigidbodyProjectionHit.rigidbody = rigidbody;
                    rigidbodyProjectionHit.hitPoint = point;
                    rigidbodyProjectionHit.effectiveHitNormal = obstructionNormal;
                    rigidbodyProjectionHit.hitVelocity = velocity;
                    rigidbodyProjectionHit.stableOnHit = hitStabilityReport.isStable;

                    rigidbodyProjectionHits[rigidbodyProjectionHitCount] = rigidbodyProjectionHit;
                    rigidbodyProjectionHitCount++;
                }
            }
        }
        private void InternalHandleVelocityProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalDirection, ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal, ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
        {
            if (transientVelocity.sqrMagnitude <= 0f)
            {
                return;
            }
            var velocityBeforeProjection = transientVelocity;
            if (stableOnHit)
            {
                lastMovementIterationFoundAnyGround = true;
                HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
            }
            else
            {
                switch (sweepState)
                {
                    case MovementSweepState.Initial:
                        HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                        sweepState = MovementSweepState.AfterFirstHit;
                        break;
                    case MovementSweepState.AfterFirstHit:
                        EvaluateCrease(
                            transientVelocity,
                            previousVelocity,
                            obstructionNormal,
                            previousObstructionNormal,
                            stableOnHit,
                            previousHitIsStable,
                            groundingStatus.isStableOnGround && !MustUnground(),
                            out bool foundCrease,
                            out Vector3 creaseDirection
                        );
                        if (foundCrease)
                        {
                            if (groundingStatus.isStableOnGround && !MustUnground())
                            {
                                transientVelocity = Vector3.zero;
                                sweepState = MovementSweepState.FoundBlockingCorner;
                            }
                            else
                            {
                                transientVelocity = Vector3.Project(transientVelocity, creaseDirection);
                                sweepState = MovementSweepState.FoundBlockingCrease;
                            }
                        }
                        else
                        {
                            HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                        }
                        break;
                    case MovementSweepState.FoundBlockingCrease:
                        transientVelocity = Vector3.zero;
                        sweepState = MovementSweepState.FoundBlockingCorner;
                        break;
                }
            }
            var newVelocityFactor = transientVelocity.magnitude / velocityBeforeProjection.magnitude;
            remainingMovementMagnitude *= newVelocityFactor;
            remainingMovementDirection = transientVelocity.normalized;
        }
        private void EvaluateCrease(Vector3 currentVelocity, Vector3 previousVelocity, Vector3 currentHitNormal, Vector3 previousHitNormal, bool currentHitIsStable, bool previousHitIsStable, bool characterIsStable, out bool isValidCrease, out Vector3 creaseDirection)
        {
            isValidCrease = false;
            creaseDirection = default;
            if (!characterIsStable || !currentHitIsStable || !previousHitIsStable)
            {
                var tmpBlockingCreaseDirection = Vector3.Cross(currentHitNormal, previousHitNormal).normalized;
                var dotPlanes = Vector3.Dot(currentHitNormal, previousHitNormal);
                var isVelocityConstrainedByCrease = false;
                if (dotPlanes < 0.999f)
                {
                    var normalAOnCreasePlane = Vector3.ProjectOnPlane(currentHitNormal, tmpBlockingCreaseDirection).normalized;
                    var normalBOnCreasePlane = Vector3.ProjectOnPlane(previousHitNormal, tmpBlockingCreaseDirection).normalized;
                    var dotPlanesOnCreasePlane =  Vector3.Dot(normalAOnCreasePlane, normalBOnCreasePlane);

                    var enteringVelocityDirectionOnCreasePlane = Vector3.ProjectOnPlane(previousVelocity, tmpBlockingCreaseDirection).normalized;
                    if (dotPlanesOnCreasePlane <= Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalAOnCreasePlane) + 0.001f &&
                        dotPlanesOnCreasePlane <= Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalBOnCreasePlane) + 0.001f)
                    {
                        isVelocityConstrainedByCrease = true;
                    }
                }
                if (isVelocityConstrainedByCrease)
                {
                    if (Vector3.Dot(tmpBlockingCreaseDirection, currentVelocity) < 0f)
                    {
                        tmpBlockingCreaseDirection = -tmpBlockingCreaseDirection;
                    }
                    isValidCrease = true;
                    creaseDirection = tmpBlockingCreaseDirection;
                }
            }
        }
        public virtual void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
        {
            if (groundingStatus.isStableOnGround && !MustUnground())
            {
                if (stableOnHit)
                {
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                else
                {
                    var obstructionRightAlongGround = Vector3.Cross(obstructionNormal, groundingStatus.groundNormal).normalized;
                    var obstructionUpAlongGround = Vector3.Cross(obstructionRightAlongGround, obstructionNormal).normalized;
                    velocity = GetDirectionTangentToSurface(velocity, obstructionUpAlongGround) * velocity.magnitude;
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
            else
            {
                if (stableOnHit)
                {
                    velocity = Vector3.ProjectOnPlane(velocity, characterUp);
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                else
                {
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
        }
        public virtual void HandleSimulateRigidbodyInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
        {}
        private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {}
        public void GetVelocityFromRigidbodyMovement(Rigidbody rigidbody, Vector3 atPoint, float deltaTime, out Vector3 linearVelocity, out Vector3 angularVelocity)
        {
            if (deltaTime > 0f)
            {
                linearVelocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                if (rigidbody.isKinematic)
                {

                }
                if (angularVelocity != Vector3.zero)
                {
                    var centerOfRotation = rigidbody.transform.TransformPoint(rigidbody.centerOfMass);
                    var centerOfRotationToPoint = atPoint - centerOfRotation;
                    var rotationFromRigidbody = Quaternion.Euler(Mathf.Rad2Deg * angularVelocity * deltaTime);
                    var finalPointPosition = centerOfRotation + (rotationFromRigidbody * centerOfRotationToPoint);
                    linearVelocity += (finalPointPosition - atPoint) / deltaTime;
                }
            }
            else
            {
                linearVelocity = default;
                angularVelocity = default;
            }
        }
        private Rigidbody GetInteractiveRigidbody(Collider collider)
        {
            Rigidbody colliderAttachedRigdibody = collider.attachedRigidbody;
            if (colliderAttachedRigdibody)
            {
                if (colliderAttachedRigdibody.gameObject.GetComponent<PhysicsMover>() || !colliderAttachedRigdibody.isKinematic)
                {
                    return colliderAttachedRigdibody;
                }
            }
            return null;
        }
        public Vector3 GetVelocityFromPosition(Vector3 from, Vector3 to, float deltaTime)
        {
            return GetVelocityFromMovement(to - from, deltaTime);
        }
        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;
            return movement / deltaTime;
        }
        private void ConstraintVectorToPlane(ref Vector3 vector, Vector3 plane)
        {
            if (vector.x > 0 != plane.x > 0)
            {
                vector.x = 0;
            }
            if (vector.y > 0 != plane.y > 0)
            {
                vector.y = 0;
            }
            if (vector.z > 0 != plane.z > 0)
            {
                vector.z = 0;
            }
        }
        private bool CheckIfColliderValidForCollisions(Collider collider)
        {
            if (collider == capsuleCollider)
            {
                return false;
            }

            var colliderAttachedRigidbody = collider.attachedRigidbody;
            
            if (colliderAttachedRigidbody)
            {
                bool isRigidbodyKinematic = colliderAttachedRigidbody.isKinematic;

                if (isMovingFromAttachedRigidbody && (!isRigidbodyKinematic || colliderAttachedRigidbody == attachedRigidbody))
                {
                    return false;
                }
                if (rigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !isRigidbodyKinematic)
                {
                    colliderAttachedRigidbody.WakeUp();
                    return false;
                }
            }

            if (!characterController.IsColliderValidForCollisions(collider))
            {
                return false;
            }
            
            return true;
        }
        public void EvaluateHitStability(Collider collider, Vector3 normal, Vector3 point, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStabilityReport hitStabilityReport)
        {

        }
        public int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            LayerMask layerMask = collidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                layerMask &= stableGroundLayers;
            }
            Vector3 bottom = position + (rotation * characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * characterTransformToCapsuleTopHemi);
            if (inflate > 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            var unfilteredHitCount = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                capsuleCollider.radius + inflate,
                overlappedColliders,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
            var hitCount = unfilteredHitCount;
            for (int i = unfilteredHitCount - 1; i >= 0 ; i--)
            {
                if (!CheckIfColliderValidForCollisions(overlappedColliders[i]))
                {
                    hitCount--;
                    if (i < hitCount)
                    {
                        overlappedColliders[i] = overlappedColliders[hitCount];
                    }
                }
            }
            return hitCount;
        }
        public bool CharacterOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, float inflate = 0f)
        {
            Vector3 bottom = position + rotation * characterTransformToCapsuleBottomHemi;
            Vector3 top = position + rotation * characterTransformToCapsuleTopHemi;
            if (inflate > 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                capsuleCollider.radius + inflate,
                overlappedColliders,
                layerMask,
                queryTriggerInteraction
            );
            foreach (var collider in overlappedColliders)
            {
                if (collider != capsuleCollider)
                {
                    return true;
                }
            }
            return false;
        }
        public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            var layerMask = collidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                layerMask &= stableGroundLayers;
            }
            Vector3 bottom = position + (rotation * characterTransformToCapsuleBottomHemi) - (direction * sweepProbingBackstepDistance);
            Vector3 top = position + (rotation * characterTransformToCapsuleTopHemi) - (direction * sweepProbingBackstepDistance);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            var unfilteredHitCount = Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                capsuleCollider.radius + inflate,
                direction,
                hits,
                distance + sweepProbingBackstepDistance,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
            var hitCount = unfilteredHitCount;

            closestHit = new RaycastHit();
            float closestHitDistance = Mathf.Infinity;

            for (int i = unfilteredHitCount - 1; i >= 0 ; i--)
            {
                hits[i].distance -= sweepProbingBackstepDistance;
                var hit = hits[i];
                var hitDistance = hit.distance;
                if (hitDistance <= 0f || !CheckIfColliderValidForCollisions(hit.collider))
                {
                    hitCount--;
                    if (i < hitCount)
                    {
                        hits[i] = hits[hitCount];
                    }
                }
                else
                {
                    if (hitDistance < closestHitDistance)
                    {
                        closestHit = hit;
                        closestHitDistance = hitDistance;
                    }
                }
            }
            return hitCount;
        }
        public bool CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, float inflate = 0f)
        {
            Vector3 bottom = position + (rotation * characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * characterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                capsuleCollider.radius + inflate,
                direction,
                hits,
                distance,
                layerMask,
                queryTriggerInteraction
            );

            closestHit = new RaycastHit();

            float closestHitDistance = Mathf.Infinity;

            bool foundValidHit = false;

            foreach (var hit in hits)
            {
                float hitDistance = hit.distance;

                if (hitDistance > 0f && hit.collider != capsuleCollider)
                {
                    if (hitDistance < closestHitDistance)
                    {
                        closestHit = hit;
                        closestHitDistance = hitDistance;

                        if (!foundValidHit)
                        {
                            foundValidHit = true;
                        }
                    }
                }
            }
            return foundValidHit;
        }
        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            Physics.CapsuleCastNonAlloc(
                position + (rotation * characterTransformToCapsuleBottomHemi) - (direction * groundProbingBackstepDistance),
                position + (rotation * characterTransformToCapsuleTopHemi) - (direction * groundProbingBackstepDistance),
                capsuleCollider.radius,
                direction,
                characterHits,
                distance + groundProbingBackstepDistance,
                collidableLayers & stableGroundLayers,
                QueryTriggerInteraction.Ignore
            );

            closestHit = new RaycastHit();
            
            float closestHitDistance = Mathf.Infinity;

            bool foundValidHit = false;

            foreach (var hit in characterHits)
            {
                float hitDistance = hit.distance;

                if (hitDistance > 0f && CheckIfColliderValidForCollisions(hit.collider))
                {
                    if (hitDistance < closestHitDistance)
                    {
                        closestHit = hit;
                        closestHit.distance -= groundProbingBackstepDistance;
                        closestHitDistance = hitDistance;

                        if (!foundValidHit)
                        {
                            foundValidHit = true;
                        }
                    }
                }
            }
            return foundValidHit;
        }
        public bool CharacterCollisionsRaycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, bool acceptOnlyStableGroundLayer = false)
        {
            LayerMask layerMask = collidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                layerMask &= stableGroundLayers;
            }

            Physics.RaycastNonAlloc(
                origin,
                direction,
                hits,
                distance,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
            
            closestHit = new RaycastHit();

            float closestHitDistance = Mathf.Infinity;

            bool foundValidHit = false;

            foreach (var hit in hits)
            {
                float hitDistance = hit.distance;

                if (hitDistance > 0f && CheckIfColliderValidForCollisions(hit.collider))
                {
                    if (hitDistance < closestHitDistance)
                    {
                        closestHit = hit;
                        closestHitDistance = hitDistance;

                        if (!foundValidHit)
                        {
                            foundValidHit = true;
                        }
                    }
                }
            }
            return foundValidHit;
        }
    }
}
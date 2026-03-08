using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace
{
    public enum MimicState
    {
        Patrol,
        Wait,
        Investigate,
        Search,
        Chase
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class MimicPatrol : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private bool loopPatrol = true;
        [SerializeField] private float waitTimeAtWaypoint = 1.5f;
        [SerializeField] private float arriveDistance = 0.25f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float turnSpeed = 240f;

        [Header("Hearing")]
        [SerializeField] private float hearingRange = 12f;
        [SerializeField] private float minNoiseLoudness = 0.2f;
        [SerializeField] private float investigateWaitTime = 1f;

        [Header("Search")]
        [SerializeField] private float searchDuration = 6f;
        [SerializeField] private float searchRadius = 4f;
        [SerializeField] private int maxSearchPoints = 3;

        [Header("Vision / Chase")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private float viewDistance = 12f;
        [Range(1f, 179f)]
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField] private float chaseSpeed = 3.4f;
        [SerializeField] private float loseSightGraceTime = 1.25f;
        [SerializeField] private float mimicEyeHeight = 1.2f;
        [SerializeField] private float playerEyeHeight = 1.2f;

        [Header("Debug")]
        [SerializeField] private bool drawPathGizmos = true;
        [SerializeField] private bool logStateChanges = false;

        private NavMeshAgent agent;
        private Mimic mimic;
        private MimicState currentState = MimicState.Patrol;
        private int waypointIndex;
        private float waitTimer;
        private float searchTimer;
        private float loseSightTimer;
        private Vector3 investigateTarget;
        private Vector3 lastKnownPlayerPosition;
        private readonly Queue<Vector3> pendingSearchPoints = new Queue<Vector3>();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            mimic = GetComponent<Mimic>();

            agent.speed = moveSpeed;
            agent.angularSpeed = turnSpeed;
            agent.stoppingDistance = arriveDistance;
            agent.autoBraking = true;
            agent.updateUpAxis = true;
            agent.updateRotation = true;
        }

        private void Start()
        {
            if (playerTarget == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerTarget = playerObject.transform;
                }
            }

            if (waypoints.Count == 0)
            {
                Debug.LogWarning("MimicPatrol: no hay waypoints asignados.");
                enabled = false;
                return;
            }

            waypointIndex = Mathf.Clamp(waypointIndex, 0, waypoints.Count - 1);
            SetDestinationToCurrentWaypoint();
            currentState = MimicState.Patrol;
        }

        private void OnEnable()
        {
            MimicNoiseSystem.NoiseEmitted += OnNoiseHeard;
        }

        private void OnDisable()
        {
            MimicNoiseSystem.NoiseEmitted -= OnNoiseHeard;
        }

        private void Update()
        {
            agent.angularSpeed = turnSpeed;

            // Keep compatibility with the current Mimic leg logic.
            if (mimic != null)
            {
                Vector3 planarVelocity = agent.velocity;
                planarVelocity.y = 0f;
                mimic.velocity = planarVelocity;
            }

            HandleVision();
            TickState();
        }

        private void HandleVision()
        {
            if (CanSeePlayer())
            {
                lastKnownPlayerPosition = playerTarget.position;
                loseSightTimer = 0f;
                ChangeState(MimicState.Chase);
                return;
            }

            if (currentState == MimicState.Chase)
            {
                loseSightTimer += Time.deltaTime;
                if (loseSightTimer >= loseSightGraceTime)
                {
                    investigateTarget = lastKnownPlayerPosition;
                    agent.speed = moveSpeed;
                    agent.SetDestination(investigateTarget);
                    waitTimer = 0f;
                    ChangeState(MimicState.Investigate);
                }
            }
        }

        private bool CanSeePlayer()
        {
            if (playerTarget == null)
            {
                return false;
            }

            Vector3 toPlayer = playerTarget.position - transform.position;
            float distance = toPlayer.magnitude;
            if (distance > viewDistance)
            {
                return false;
            }

            Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatToPlayer.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            float angle = Vector3.Angle(transform.forward, flatToPlayer.normalized);
            if (angle > viewAngle * 0.5f)
            {
                return false;
            }

            Vector3 origin = GetMimicEyePosition();
            Vector3 target = GetPlayerViewPosition();
            Vector3 direction = (target - origin).normalized;
            float rayDistance = Vector3.Distance(origin, target);

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                rayDistance,
                lineOfSightMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits.Length == 0)
            {
                return false;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                {
                    continue;
                }

                return hitTransform == playerTarget || hitTransform.IsChildOf(playerTarget);
            }

            return false;
        }

        private Vector3 GetMimicEyePosition()
        {
            float eyeHeight = mimicEyeHeight;
            if (agent != null)
            {
                eyeHeight = Mathf.Max(eyeHeight, agent.baseOffset + 0.6f);
            }
            return transform.position + Vector3.up * eyeHeight;
        }

        private Vector3 GetPlayerViewPosition()
        {
            CharacterController playerController = playerTarget.GetComponent<CharacterController>();
            if (playerController != null)
            {
                return playerTarget.position + playerController.center + Vector3.up * (playerController.height * 0.15f);
            }

            Collider playerCollider = playerTarget.GetComponent<Collider>();
            if (playerCollider != null)
            {
                return playerCollider.bounds.center;
            }

            return playerTarget.position + Vector3.up * playerEyeHeight;
        }

        private void TickState()
        {
            if (currentState == MimicState.Chase)
            {
                TickChase();
                return;
            }

            if (currentState == MimicState.Patrol)
            {
                TickPatrol();
                return;
            }

            if (currentState == MimicState.Wait)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTimeAtWaypoint)
                {
                    MoveToNextWaypoint();
                }
                return;
            }

            if (currentState == MimicState.Investigate)
            {
                TickInvestigate();
                return;
            }

            if (currentState == MimicState.Search)
            {
                TickSearch();
            }
        }

        private void TickChase()
        {
            agent.speed = chaseSpeed;
            if (playerTarget != null)
            {
                agent.SetDestination(playerTarget.position);
            }
        }

        private void TickPatrol()
        {
            agent.speed = moveSpeed;
            if (ReachedDestination())
            {
                ChangeState(MimicState.Wait);
                waitTimer = 0f;
                agent.ResetPath();
            }
        }

        private void TickInvestigate()
        {
            agent.speed = moveSpeed;
            if (ReachedDestination())
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= investigateWaitTime)
                {
                    StartSearch();
                }
            }
        }

        private void TickSearch()
        {
            agent.speed = moveSpeed;
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchDuration)
            {
                ReturnToPatrol();
                return;
            }

            if (!ReachedDestination())
            {
                return;
            }

            if (pendingSearchPoints.Count > 0)
            {
                agent.SetDestination(pendingSearchPoints.Dequeue());
            }
            else
            {
                ReturnToPatrol();
            }
        }

        private bool ReachedDestination()
        {
            if (agent.pathPending)
            {
                return false;
            }

            if (agent.remainingDistance > arriveDistance)
            {
                return false;
            }

            return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
        }

        private void OnNoiseHeard(MimicNoiseEvent noiseEvent)
        {
            if (!enabled)
            {
                return;
            }

            if (noiseEvent.loudness < minNoiseLoudness)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, noiseEvent.position);
            float effectiveHearingRange = hearingRange * Mathf.Clamp(noiseEvent.loudness, 0.1f, 3f);
            if (distance > effectiveHearingRange)
            {
                return;
            }

            investigateTarget = noiseEvent.position;
            agent.SetDestination(investigateTarget);
            waitTimer = 0f;
            ChangeState(MimicState.Investigate);
        }

        private void StartSearch()
        {
            pendingSearchPoints.Clear();
            for (int i = 0; i < maxSearchPoints; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * searchRadius;
                randomOffset.y = 0f;
                Vector3 candidate = investigateTarget + randomOffset;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    pendingSearchPoints.Enqueue(hit.position);
                }
            }

            searchTimer = 0f;
            ChangeState(MimicState.Search);

            if (pendingSearchPoints.Count > 0)
            {
                agent.SetDestination(pendingSearchPoints.Dequeue());
            }
            else
            {
                ReturnToPatrol();
            }
        }

        private void ReturnToPatrol()
        {
            ChangeState(MimicState.Patrol);
            loseSightTimer = 0f;
            SetDestinationToCurrentWaypoint();
        }

        private void ChangeState(MimicState newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
            if (logStateChanges)
            {
                Debug.Log("Mimic state -> " + currentState);
            }
        }

        private void MoveToNextWaypoint()
        {
            if (waypoints.Count == 0)
            {
                return;
            }

            if (loopPatrol)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Count;
            }
            else
            {
                if (waypointIndex < waypoints.Count - 1)
                {
                    waypointIndex++;
                }
                else
                {
                    return;
                }
            }

            SetDestinationToCurrentWaypoint();
            ChangeState(MimicState.Patrol);
        }

        private void SetDestinationToCurrentWaypoint()
        {
            Transform target = waypoints[waypointIndex];
            if (target == null)
            {
                Debug.LogWarning("MimicPatrol: waypoint nulo en indice " + waypointIndex);
                return;
            }

            agent.SetDestination(target.position);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawPathGizmos || waypoints == null || waypoints.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count; i++)
            {
                Transform current = waypoints[i];
                if (current == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(current.position, 0.2f);

                int nextIndex = i + 1;
                if (nextIndex >= waypoints.Count)
                {
                    if (!loopPatrol)
                    {
                        continue;
                    }
                    nextIndex = 0;
                }

                Transform next = waypoints[nextIndex];
                if (next != null)
                {
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MimicSpace.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MimicMotorNavMesh : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private bool loopPatrol = true;
        [SerializeField] private float waitAtWaypoint = 1.2f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2.1f;
        [SerializeField] private float investigateSpeed = 2.5f;
        [SerializeField] private float chaseSpeed = 3.6f;
        [SerializeField] private float turnSpeed = 260f;
        [SerializeField] private float arriveDistance = 0.3f;
        [SerializeField] private float navSampleDistance = 2f;

        private NavMeshAgent agent;
        private Mimic mimic;
        private int waypointIndex;
        private float waitTimer;
        private bool waitingAtWaypoint;

        public Vector3 Position => transform.position;
        public bool HasWaypoints => waypoints.Count > 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            mimic = GetComponent<Mimic>();

            agent.angularSpeed = turnSpeed;
            agent.stoppingDistance = arriveDistance;
            agent.autoBraking = true;
            agent.updateRotation = true;
        }

        private void Update()
        {
            if (mimic != null)
            {
                Vector3 planarVelocity = agent.velocity;
                planarVelocity.y = 0f;
                mimic.velocity = planarVelocity;
            }
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            waitingAtWaypoint = false;
            agent.speed = speed;

            Vector3 finalDestination = destination;
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, navSampleDistance, NavMesh.AllAreas))
            {
                finalDestination = hit.position;
            }

            agent.SetDestination(finalDestination);
        }

        public bool ReachedDestination()
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

        public void Stop()
        {
            agent.ResetPath();
        }

        public void TickPatrol()
        {
            if (!HasWaypoints)
            {
                return;
            }

            if (!agent.hasPath && !waitingAtWaypoint)
            {
                MoveTo(waypoints[waypointIndex].position, patrolSpeed);
            }

            if (!ReachedDestination())
            {
                return;
            }

            if (!waitingAtWaypoint)
            {
                waitingAtWaypoint = true;
                waitTimer = 0f;
                Stop();
                return;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer < waitAtWaypoint)
            {
                return;
            }

            waitingAtWaypoint = false;
            AdvanceWaypoint();
            MoveTo(waypoints[waypointIndex].position, patrolSpeed);
        }

        public void MoveInvestigate(Vector3 destination)
        {
            MoveTo(destination, investigateSpeed);
        }

        public void MoveChase(Vector3 destination)
        {
            MoveTo(destination, chaseSpeed);
        }

        public bool TryGetRandomReachablePoint(Vector3 center, float radius, out Vector3 point)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 random = center + Random.insideUnitSphere * radius;
                random.y = center.y;
                if (NavMesh.SamplePosition(random, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = center;
            return false;
        }

        private void AdvanceWaypoint()
        {
            if (waypoints.Count == 0)
            {
                return;
            }

            if (loopPatrol)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Count;
                return;
            }

            waypointIndex = Mathf.Min(waypointIndex + 1, waypoints.Count - 1);
        }
    }
}

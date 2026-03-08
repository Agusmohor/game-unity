using System.Collections.Generic;
using UnityEngine;

namespace MimicSpace.AI
{
    [RequireComponent(typeof(MimicPerception))]
    [RequireComponent(typeof(MimicMemory))]
    [RequireComponent(typeof(MimicMotorNavMesh))]
    public class MimicBrain : MonoBehaviour
    {
        [Header("State Thresholds")]
        [SerializeField] private float investigateNoiseScoreThreshold = 0.2f;
        [SerializeField] private float suspiciousZoneThreshold = 10f;
        [SerializeField] private float chaseEvidenceThreshold = 40f;
        [SerializeField] private float loseVisualGrace = 1.4f;
        [SerializeField] private float minimumChaseDuration = 2.5f;

        [Header("Search")]
        [SerializeField] private int searchPointsCount = 4;
        [SerializeField] private float searchRadius = 5f;
        [SerializeField] private float searchPointWait = 0.8f;
        [SerializeField] private int maxSearchCyclesAfterVisual = 3;
        [SerializeField] private float postVisualSearchMemorySeconds = 25f;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = false;

        private MimicPerception perception;
        private MimicMemory memory;
        private MimicMotorNavMesh motor;

        private MimicBrainState state = MimicBrainState.Patrol;
        private float lastVisualTime = -999f;
        private float chaseStartedTime = -999f;
        private float lastConfirmedVisualTime = -999f;
        private float searchWaitTimer;
        private int searchCyclesAfterVisual;
        private readonly Queue<Vector3> searchQueue = new Queue<Vector3>();
        private Vector3 investigateTarget;

        private void Awake()
        {
            perception = GetComponent<MimicPerception>();
            memory = GetComponent<MimicMemory>();
            motor = GetComponent<MimicMotorNavMesh>();
        }

        private void OnEnable()
        {
            perception.PlayerSeen += OnPlayerSeen;
            perception.NoiseHeard += OnNoiseHeard;
        }

        private void OnDisable()
        {
            perception.PlayerSeen -= OnPlayerSeen;
            perception.NoiseHeard -= OnNoiseHeard;
        }

        private void Update()
        {
            memory.Tick(perception.HasVisualContact, Time.deltaTime);

            if (ShouldChaseNow())
            {
                ChangeState(MimicBrainState.Chase);
            }

            if (state == MimicBrainState.Patrol)
            {
                TickPatrol();
                return;
            }

            if (state == MimicBrainState.Investigate)
            {
                TickInvestigate();
                return;
            }

            if (state == MimicBrainState.Search)
            {
                TickSearch();
                return;
            }

            TickChase();
        }

        private void TickPatrol()
        {
            if (!motor.HasWaypoints)
            {
                return;
            }

            if (TryPromoteToInvestigate())
            {
                return;
            }

            motor.TickPatrol();
        }

        private void TickInvestigate()
        {
            if (ShouldChaseNow())
            {
                ChangeState(MimicBrainState.Chase);
                return;
            }

            if (!motor.ReachedDestination())
            {
                return;
            }

            BuildSearchQueue(investigateTarget);
            ChangeState(MimicBrainState.Search);
        }

        private void TickSearch()
        {
            if (ShouldChaseNow())
            {
                ChangeState(MimicBrainState.Chase);
                return;
            }

            if (!motor.ReachedDestination())
            {
                return;
            }

            searchWaitTimer += Time.deltaTime;
            if (searchWaitTimer < searchPointWait)
            {
                return;
            }

            searchWaitTimer = 0f;
            if (searchQueue.Count > 0)
            {
                motor.MoveInvestigate(searchQueue.Dequeue());
                return;
            }

            if (ShouldContinueSearchingLastKnownZone())
            {
                searchCyclesAfterVisual++;
                BuildSearchQueue(memory.LastKnownPlayerPosition);
                return;
            }

            if (TryPromoteToInvestigate())
            {
                return;
            }

            ChangeState(MimicBrainState.Patrol);
        }

        private void TickChase()
        {
            if (perception.HasVisualContact)
            {
                motor.MoveChase(perception.CurrentObservedPlayerPosition);
                return;
            }

            if (Time.time - chaseStartedTime < minimumChaseDuration)
            {
                motor.MoveChase(memory.LastKnownPlayerPosition);
                return;
            }

            if (Time.time - lastVisualTime <= loseVisualGrace && memory.HasRecentVisual())
            {
                motor.MoveChase(memory.LastKnownPlayerPosition);
                return;
            }

            investigateTarget = memory.LastKnownPlayerPosition;
            motor.MoveInvestigate(investigateTarget);
            searchCyclesAfterVisual = 0;
            ChangeState(MimicBrainState.Investigate);
        }

        private bool ShouldChaseNow()
        {
            if (state == MimicBrainState.Chase && Time.time - chaseStartedTime < minimumChaseDuration)
            {
                return true;
            }

            if (perception.HasVisualContact)
            {
                return true;
            }

            if (memory.Evidence < chaseEvidenceThreshold)
            {
                return false;
            }

            return memory.HasRecentVisual();
        }

        private bool TryPromoteToInvestigate()
        {
            if (memory.TryGetBestRecentNoise(out Vector3 noisePos, out float noiseScore)
                && noiseScore >= investigateNoiseScoreThreshold)
            {
                investigateTarget = noisePos;
                motor.MoveInvestigate(investigateTarget);
                ChangeState(MimicBrainState.Investigate);
                return true;
            }

            if (memory.TryGetMostSuspiciousZone(out Vector3 zonePos, out float suspicion)
                && suspicion >= suspiciousZoneThreshold)
            {
                investigateTarget = zonePos;
                motor.MoveInvestigate(investigateTarget);
                ChangeState(MimicBrainState.Investigate);
                return true;
            }

            return false;
        }

        private void BuildSearchQueue(Vector3 anchor)
        {
            searchQueue.Clear();
            searchWaitTimer = 0f;

            for (int i = 0; i < searchPointsCount; i++)
            {
                if (motor.TryGetRandomReachablePoint(anchor, searchRadius, out Vector3 p))
                {
                    searchQueue.Enqueue(p);
                }
            }

            if (searchQueue.Count > 0)
            {
                motor.MoveInvestigate(searchQueue.Dequeue());
            }
        }

        private void OnPlayerSeen(Vector3 playerPos)
        {
            memory.RegisterPlayerSeen(playerPos);
            lastVisualTime = Time.time;
            lastConfirmedVisualTime = Time.time;
            searchCyclesAfterVisual = 0;
        }

        private void OnNoiseHeard(Vector3 noisePos, float loudness)
        {
            memory.RegisterNoise(noisePos, loudness);
        }

        private void ChangeState(MimicBrainState newState)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;
            if (state == MimicBrainState.Chase)
            {
                chaseStartedTime = Time.time;
            }

            if (logStateChanges)
            {
                Debug.Log("MimicBrain state -> " + state);
            }
        }

        private bool ShouldContinueSearchingLastKnownZone()
        {
            if (searchCyclesAfterVisual >= maxSearchCyclesAfterVisual)
            {
                return false;
            }

            if (Time.time - lastConfirmedVisualTime > postVisualSearchMemorySeconds)
            {
                return false;
            }

            return true;
        }
    }
}

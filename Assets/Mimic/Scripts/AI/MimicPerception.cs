using System;
using UnityEngine;

namespace MimicSpace.AI
{
    public class MimicPerception : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform playerTarget;

        [Header("Vision")]
        [SerializeField] private float viewDistance = 16f;
        [Range(1f, 179f)]
        [SerializeField] private float viewAngle = 110f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField] private float mimicEyeHeight = 1.2f;
        [SerializeField] private float playerEyeHeight = 1.2f;
        [SerializeField] private float closeRangeIgnoreAngle = 3f;

        [Header("Hearing")]
        [SerializeField] private float hearingRange = 14f;
        [SerializeField] private float minNoiseLoudness = 0.05f;

        public event Action<Vector3> PlayerSeen;
        public event Action<Vector3, float> NoiseHeard;

        public bool HasVisualContact { get; private set; }
        public Vector3 CurrentObservedPlayerPosition { get; private set; }
        public Transform PlayerTarget => playerTarget;

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
        }

        private void OnEnable()
        {
            MimicNoiseSystem.NoiseEmitted += OnNoiseEmitted;
        }

        private void OnDisable()
        {
            MimicNoiseSystem.NoiseEmitted -= OnNoiseEmitted;
        }

        private void Update()
        {
            UpdateVision();
        }

        private void UpdateVision()
        {
            HasVisualContact = false;
            if (playerTarget == null)
            {
                return;
            }

            Vector3 toPlayer = playerTarget.position - transform.position;
            float distanceToPlayer = toPlayer.magnitude;
            if (distanceToPlayer > viewDistance)
            {
                return;
            }

            Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatToPlayer.sqrMagnitude > 0.0001f && distanceToPlayer > closeRangeIgnoreAngle)
            {
                float angle = Vector3.Angle(transform.forward, flatToPlayer.normalized);
                if (angle > viewAngle * 0.5f)
                {
                    return;
                }
            }

            Vector3 origin = transform.position + Vector3.up * mimicEyeHeight;
            Vector3 target = GetPlayerAimPoint();
            Vector3 direction = (target - origin).normalized;
            float distance = Vector3.Distance(origin, target);

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, lineOfSightMask, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0)
            {
                return;
            }

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                {
                    continue;
                }

                if (hitTransform == playerTarget || hitTransform.IsChildOf(playerTarget))
                {
                    HasVisualContact = true;
                    CurrentObservedPlayerPosition = playerTarget.position;
                    PlayerSeen?.Invoke(CurrentObservedPlayerPosition);
                }
                return;
            }
        }

        private Vector3 GetPlayerAimPoint()
        {
            CharacterController cc = playerTarget.GetComponent<CharacterController>();
            if (cc != null)
            {
                return playerTarget.position + cc.center;
            }

            Collider col = playerTarget.GetComponent<Collider>();
            if (col != null)
            {
                return col.bounds.center;
            }

            return playerTarget.position + Vector3.up * playerEyeHeight;
        }

        private void OnNoiseEmitted(MimicNoiseEvent noise)
        {
            if (noise.loudness < minNoiseLoudness)
            {
                return;
            }

            float effectiveRange = hearingRange * Mathf.Clamp(noise.loudness, 0.1f, 3f);
            float distance = Vector3.Distance(transform.position, noise.position);
            if (distance > effectiveRange)
            {
                return;
            }

            NoiseHeard?.Invoke(noise.position, noise.loudness);
        }
    }
}

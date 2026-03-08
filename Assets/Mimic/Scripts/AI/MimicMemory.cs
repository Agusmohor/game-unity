using System.Collections.Generic;
using UnityEngine;

namespace MimicSpace.AI
{
    public class MimicMemory : MonoBehaviour
    {
        private struct NoiseRecord
        {
            public Vector3 position;
            public float loudness;
            public float timestamp;
        }

        [Header("Evidence")]
        [SerializeField] private float evidence = 0f;
        [SerializeField] private float maxEvidence = 100f;
        [SerializeField] private float evidenceDecayPerSecond = 6f;
        [SerializeField] private float visionEvidenceGainPerSecond = 30f;
        [SerializeField] private float noiseEvidenceGainMultiplier = 18f;

        [Header("Memory Timing")]
        [SerializeField] private float noiseMemorySeconds = 12f;
        [SerializeField] private float visualMemorySeconds = 5f;

        [Header("Suspicion Zones")]
        [SerializeField] private float zoneSize = 6f;
        [SerializeField] private float zoneSuspicionDecayPerSecond = 2f;
        [SerializeField] private float noiseZoneGainMultiplier = 10f;
        [SerializeField] private float visualZoneGainPerSecond = 14f;

        private readonly List<NoiseRecord> recentNoises = new List<NoiseRecord>();
        private readonly Dictionary<Vector2Int, float> zoneSuspicion = new Dictionary<Vector2Int, float>();

        public Vector3 LastKnownPlayerPosition { get; private set; }
        public float LastSeenTime { get; private set; } = -999f;
        public float Evidence => evidence;

        public void RegisterPlayerSeen(Vector3 worldPos)
        {
            LastKnownPlayerPosition = worldPos;
            LastSeenTime = Time.time;
        }

        public void RegisterNoise(Vector3 worldPos, float loudness)
        {
            recentNoises.Add(new NoiseRecord
            {
                position = worldPos,
                loudness = loudness,
                timestamp = Time.time
            });

            Vector2Int zoneKey = WorldToZone(worldPos);
            AddZoneSuspicion(zoneKey, loudness * noiseZoneGainMultiplier);

            evidence = Mathf.Min(maxEvidence, evidence + loudness * noiseEvidenceGainMultiplier);
        }

        public void Tick(bool hasVisualContact, float deltaTime)
        {
            if (hasVisualContact)
            {
                evidence = Mathf.Min(maxEvidence, evidence + visionEvidenceGainPerSecond * deltaTime);
                AddZoneSuspicion(WorldToZone(LastKnownPlayerPosition), visualZoneGainPerSecond * deltaTime);
            }
            else
            {
                evidence = Mathf.Max(0f, evidence - evidenceDecayPerSecond * deltaTime);
            }

            DecayNoiseHistory();
            DecayZoneSuspicion(deltaTime);
        }

        public bool HasRecentVisual()
        {
            return Time.time - LastSeenTime <= visualMemorySeconds;
        }

        public bool TryGetBestRecentNoise(out Vector3 position, out float score)
        {
            position = Vector3.zero;
            score = 0f;
            if (recentNoises.Count == 0)
            {
                return false;
            }

            float now = Time.time;
            float best = float.MinValue;
            int bestIndex = -1;

            for (int i = 0; i < recentNoises.Count; i++)
            {
                NoiseRecord record = recentNoises[i];
                float age = now - record.timestamp;
                float freshness = Mathf.Clamp01(1f - age / noiseMemorySeconds);
                float value = record.loudness * freshness;
                if (value > best)
                {
                    best = value;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                return false;
            }

            position = recentNoises[bestIndex].position;
            score = best;
            return true;
        }

        public bool TryGetMostSuspiciousZone(out Vector3 zoneCenter, out float suspicionValue)
        {
            zoneCenter = Vector3.zero;
            suspicionValue = 0f;
            if (zoneSuspicion.Count == 0)
            {
                return false;
            }

            float best = float.MinValue;
            Vector2Int bestKey = default;
            foreach (KeyValuePair<Vector2Int, float> kv in zoneSuspicion)
            {
                if (kv.Value > best)
                {
                    best = kv.Value;
                    bestKey = kv.Key;
                }
            }

            if (best <= 0f)
            {
                return false;
            }

            zoneCenter = ZoneToWorld(bestKey);
            suspicionValue = best;
            return true;
        }

        private void DecayNoiseHistory()
        {
            float now = Time.time;
            for (int i = recentNoises.Count - 1; i >= 0; i--)
            {
                if (now - recentNoises[i].timestamp > noiseMemorySeconds)
                {
                    recentNoises.RemoveAt(i);
                }
            }
        }

        private void DecayZoneSuspicion(float deltaTime)
        {
            if (zoneSuspicion.Count == 0)
            {
                return;
            }

            List<Vector2Int> keys = new List<Vector2Int>(zoneSuspicion.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Vector2Int key = keys[i];
                float value = Mathf.Max(0f, zoneSuspicion[key] - zoneSuspicionDecayPerSecond * deltaTime);
                if (value <= 0.001f)
                {
                    zoneSuspicion.Remove(key);
                }
                else
                {
                    zoneSuspicion[key] = value;
                }
            }
        }

        private Vector2Int WorldToZone(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / zoneSize);
            int z = Mathf.FloorToInt(worldPos.z / zoneSize);
            return new Vector2Int(x, z);
        }

        private Vector3 ZoneToWorld(Vector2Int zone)
        {
            return new Vector3(
                (zone.x + 0.5f) * zoneSize,
                transform.position.y,
                (zone.y + 0.5f) * zoneSize
            );
        }

        private void AddZoneSuspicion(Vector2Int zoneKey, float amount)
        {
            if (zoneSuspicion.TryGetValue(zoneKey, out float current))
            {
                zoneSuspicion[zoneKey] = current + amount;
            }
            else
            {
                zoneSuspicion[zoneKey] = amount;
            }
        }
    }
}

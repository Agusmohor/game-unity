using System;
using UnityEngine;

namespace MimicSpace
{
    public struct MimicNoiseEvent
    {
        public Vector3 position;
        public float loudness;
        public GameObject source;
    }

    public static class MimicNoiseSystem
    {
        public static event Action<MimicNoiseEvent> NoiseEmitted;

        public static void EmitNoise(Vector3 position, float loudness, GameObject source = null)
        {
            MimicNoiseEvent noiseEvent = new MimicNoiseEvent
            {
                position = position,
                loudness = loudness,
                source = source
            };

            NoiseEmitted?.Invoke(noiseEvent);
        }
    }
}

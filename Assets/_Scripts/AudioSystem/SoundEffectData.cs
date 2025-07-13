using UnityEngine;
using System;

namespace PixelGameJam.Audio
{
    [Serializable]
    public class SoundClipData
    {
        public string SoundName; // 用于标识音效片段
        public AudioClip SoundClip;
        [Range(0f, 1f)]
        public float Volume = 1f;
        [Range(0f, 3f)]
        public float PitchMin = 1f;
        [Range(0f, 3f)]
        public float PitchMax = 1f;
    }
}
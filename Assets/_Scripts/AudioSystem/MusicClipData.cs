using UnityEngine;
using System;

namespace PixelGameJam.Audio
{
    [Serializable]
    public class MusicClipData
    {
        public string MusicName; // 用于标识音乐片段
        public AudioClip MusicClip;
        [Range(0f, 1f)]
        public float Volume = 1f;
        public bool Loop = true;
    }
}
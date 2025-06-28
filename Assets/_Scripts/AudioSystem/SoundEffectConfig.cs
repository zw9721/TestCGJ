using UnityEngine;
using System.Collections.Generic;

namespace PixelGameJam.Audio
{
    [CreateAssetMenu(fileName = "SoundEffectConfig", menuName = "Audio/Sound Effect Config", order = 2)]
    public class SoundEffectConfig : ScriptableObject
    {
        public List<SoundClipData> SoundClips = new List<SoundClipData>();

        // 可以添加一个方法来通过 SoundName 获取 SoundClip
        public SoundClipData GetSoundClip(string soundName)
        {
            return SoundClips.Find(clip => clip.SoundName == soundName);
        }
    }
}
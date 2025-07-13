using UnityEngine;
using System.Collections.Generic;

namespace PixelGameJam.Audio
{
    [CreateAssetMenu(fileName = "MusicConfig", menuName = "Audio/Music Config", order = 1)]
    public class MusicConfig : ScriptableObject
    {
        public List<MusicClipData> MusicClips = new List<MusicClipData>();

        // 可以添加一个方法来通过 MusicName 获取 MusicClipData
        public MusicClipData GetMusicClipData(string musicName)
        {
            return MusicClips.Find(clip => clip.MusicName == musicName);
        }
    }
}
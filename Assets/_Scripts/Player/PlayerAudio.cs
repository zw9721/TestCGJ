using UnityEngine;
using PixelGameJam.Audio;

namespace PixelGameJam.Player
{
    public class PlayerAudio : MonoBehaviour
    {
        /// <summary>
        /// 播放指定名称的音效，用于动画事件调用。
        /// </summary>
        /// <param name="soundName">要播放的音效名称。</param>
        public void PlaySound(string soundName)
        {
            AudioManager.Instance.PlaySound(soundName);
        }
    }
}
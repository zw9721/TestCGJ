using UnityEngine;
using QFramework;
using System.Collections.Generic; // 引入 Dictionary
using QFramework.AudioKit; // 引入 IAudioPlayer 所在的命名空间

namespace PixelGameJam.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Header("音频配置")]
        public MusicConfig MusicConfig;
        public SoundEffectConfig SoundEffectConfig;

        private Dictionary<string, IAudioPlayer> mLoopingSounds = new Dictionary<string, IAudioPlayer>(); // 用于存储正在播放的循环音效

        // 播放音乐
        public void PlayMusic(string musicName)
        {
            if (MusicConfig == null)
            {
                Debug.LogError("MusicConfig 未分配！");
                return;
            }

            MusicClipData clipData = MusicConfig.GetMusicClipData(musicName);
            if (clipData != null && clipData.MusicClip != null)
            {
                AudioKit.PlayMusic(clipData.MusicClip, clipData.Loop, null, null, clipData.Volume);
            }
            else
            {
                Debug.LogWarning($"未找到音乐片段: {musicName}");
            }
        }

        // 播放音效
        public void PlaySound(string soundName, bool loop = false) // 增加 loop 参数
        {
            if (SoundEffectConfig == null)
            {
                Debug.LogError("SoundEffectConfig 未分配！");
                return;
            }

            SoundClipData clipData = SoundEffectConfig.GetSoundClip(soundName);
            if (clipData != null && clipData.SoundClip != null)
            {
                float pitch = Random.Range(clipData.PitchMin, clipData.PitchMax);
                IAudioPlayer player = AudioKit.PlaySound(clipData.SoundClip, loop, null, clipData.Volume, pitch); // 传递 loop 参数

                if (loop)
                {
                    // 如果是循环音效，则存储起来以便后续停止
                    if (mLoopingSounds.ContainsKey(soundName))
                    {
                        mLoopingSounds[soundName].Stop(); // 停止旧的同名循环音效
                        mLoopingSounds.Remove(soundName);
                    }
                    mLoopingSounds.Add(soundName, player);
                }
            }
            else
            {
                Debug.LogWarning($"未找到音效片段: {soundName}");
            }
        }

        // 停止特定音效
        public void StopSound(string soundName)
        {
            if (mLoopingSounds.TryGetValue(soundName, out IAudioPlayer player))
            {
                player.Stop();
                mLoopingSounds.Remove(soundName);
            }
        }

        // 停止音乐
        public void StopMusic()
        {
            AudioKit.StopMusic();
        }

        // 停止所有音效
        public void StopAllSoundEffects()
        {
            AudioKit.StopAllSound(); // 使用 StopAllSound
            // 停止所有由 AudioManager 追踪的循环音效
            foreach (var player in mLoopingSounds.Values)
            {
                player.Stop();
            }
            mLoopingSounds.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllSoundEffects(); // 确保在销毁时停止所有音效
        }
    }
}
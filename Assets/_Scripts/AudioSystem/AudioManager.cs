using UnityEngine;
using QFramework;

namespace PixelGameJam.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Header("音频配置")]
        public MusicConfig MusicConfig;
        public SoundEffectConfig SoundEffectConfig;

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
        public void PlaySound(string soundName)
        {
            if (SoundEffectConfig == null)
            {
                Debug.LogError("SoundEffectConfig 未分配！");
                return;
            }

            SoundClipData clipData = SoundEffectConfig.GetSoundClip(soundName);
            if (clipData != null && clipData.SoundClip != null)
            {
                // 这里先使用 SoundClipData 中的 PitchMin 作为固定音高示例
                float pitch = Random.Range(clipData.PitchMin, clipData.PitchMax);
                AudioKit.PlaySound(clipData.SoundClip, false, null, clipData.Volume, pitch); // 使用 PlaySound 并添加 callBack 参数
            }
            else
            {
                Debug.LogWarning($"未找到音效片段: {soundName}");
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
        }
    }
}
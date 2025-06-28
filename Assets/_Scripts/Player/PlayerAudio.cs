using QFramework;
using UnityEngine;
using Game; // 引入 Game 命名空间
using PixelGameJam.Audio; // 引入 AudioManager 所在的命名空间

public class PlayerAudio : MonoBehaviour
{
    private PlayerState mPlayerState;
    private bool mIsMovingSoundPlaying = false;

    private void OnEnable()
    {
        mPlayerState = PlayerState.Instance;
        if (mPlayerState != null)
        {
            mPlayerState.OnStateChanged += HandleStateChange;
        }
    }

    private void OnDisable()
    {
        if (mPlayerState != null)
        {
            mPlayerState.OnStateChanged -= HandleStateChange;
        }
        // 确保在禁用时停止所有循环音效
        if (mIsMovingSoundPlaying)
        {
            PixelGameJam.Audio.AudioManager.Instance.StopSound("玩家脚步声_loop");
            mIsMovingSoundPlaying = false;
        }
    }

    private void HandleStateChange(EPlayerState newState)
    {
        switch (newState)
        {
            case EPlayerState.Moving:
            case EPlayerState.Carrying_Moving:
                if (!mIsMovingSoundPlaying)
                {
                    PixelGameJam.Audio.AudioManager.Instance.PlaySound("玩家脚步声_loop", true); // 播放移动音效 (循环)
                    mIsMovingSoundPlaying = true;
                }
                break;
            case EPlayerState.Idle:
            case EPlayerState.Carrying_Idle:
                if (mIsMovingSoundPlaying)
                {
                    PixelGameJam.Audio.AudioManager.Instance.StopSound("玩家脚步声_loop"); // 停止移动音效
                    mIsMovingSoundPlaying = false;
                }
                break;
            case EPlayerState.Struggling:
                PixelGameJam.Audio.AudioManager.Instance.PlaySound("玩家挣扎音效"); // 播放挣扎音效 (单次)
                break;
            case EPlayerState.Stunned:
                PixelGameJam.Audio.AudioManager.Instance.PlaySound("玩家眩晕音效"); // 播放眩晕音效 (单次)
                break;
            case EPlayerState.KnockedBack:
                PixelGameJam.Audio.AudioManager.Instance.PlaySound("玩家击退音效"); // 播放击退音效 (单次)
                break;
        }
    }
}
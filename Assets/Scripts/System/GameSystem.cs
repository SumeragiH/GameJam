using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameSystem : SingletonBaseWithMono<GameSystem>
{
    [SerializeField] public float ShiftPressInterval = 1.0f;
    private float tipIntervalTimer = 0f;
    private int tipIndex = 0;
    [SerializeField] private SubtitleView subtitleView;
    void Start()
    {
        tipIntervalTimer = 1f;
        SafeZoneSystem.Instance.InitActiveSafeZone();
    }

    void Update()
    {

        if (tipIndex >= 2 || tipIntervalTimer > 0f)
        {
            tipIntervalTimer -= Time.deltaTime;
        }
        else
        {
            if (tipIndex == 0)
            {
                ShowTip("用数字键+Shift切换光亮，照亮这片黑暗吧！", 3f);
            }
            else if (tipIndex == 1)
            {
                ShowTip("使用2+Shift，寻找下一处光亮吧", 3f);
            }
            tipIndex++;
            tipIntervalTimer = 3f;
        }
    }

    public void ShowTip(string tipContent, float duration)
    {
        subtitleView.ShowSubtitle(tipContent, duration);
    }
}

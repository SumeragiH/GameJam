using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundController : SingletonBaseWithMono<SoundController>
{
    public enum SoundTarget
    {
        A,
        B,
        Both
    }

    [Header("Targets (2 AudioSources)")]
    [SerializeField] private AudioSource _sourceA;
    [SerializeField] private AudioSource _sourceB;

    [Header("Default Volumes")]
    [Range(0f, 1f)][SerializeField] private float _defaultVolumeA = 1f;
    [Range(0f, 1f)][SerializeField] private float _defaultVolumeB = 1f;

    private Coroutine _routineA;
    private Coroutine _routineB;

    private void Awake()
    {
        ApplyDefaultVolumes();
    }

    /// <summary>
    /// 对外接口：指定哪个音源临时变大（带渐变）
    /// fadeIn -> hold -> fadeOut -> 恢复默认
    /// </summary>
    public void TempBoostWithFade(SoundTarget target, float boostedVolume, float holdDuration, float fadeInTime, float fadeOutTime)
    {
        boostedVolume = Mathf.Clamp01(boostedVolume);
        holdDuration = Mathf.Max(0f, holdDuration);
        fadeInTime = Mathf.Max(0f, fadeInTime);
        fadeOutTime = Mathf.Max(0f, fadeOutTime);

        switch (target)
        {
            case SoundTarget.A:
                RestartRoutineForA(boostedVolume, holdDuration, fadeInTime, fadeOutTime);
                break;
            case SoundTarget.B:
                RestartRoutineForB(boostedVolume, holdDuration, fadeInTime, fadeOutTime);
                break;
            case SoundTarget.Both:
                RestartRoutineForA(boostedVolume, holdDuration, fadeInTime, fadeOutTime);
                RestartRoutineForB(boostedVolume, holdDuration, fadeInTime, fadeOutTime);
                break;
        }
    }

    public void SetDefaultVolumes(float volumeA, float volumeB, bool applyNow = true)
    {
        _defaultVolumeA = Mathf.Clamp01(volumeA);
        _defaultVolumeB = Mathf.Clamp01(volumeB);

        if (applyNow) ApplyDefaultVolumes();
    }

    private void RestartRoutineForA(float boosted, float hold, float fadeIn, float fadeOut)
    {
        if (_sourceA == null) return;
        if (_routineA != null) StopCoroutine(_routineA);
        _routineA = StartCoroutine(BoostSingleRoutine(_sourceA, _defaultVolumeA, boosted, hold, fadeIn, fadeOut, () => _routineA = null));
    }

    private void RestartRoutineForB(float boosted, float hold, float fadeIn, float fadeOut)
    {
        if (_sourceB == null) return;
        if (_routineB != null) StopCoroutine(_routineB);
        _routineB = StartCoroutine(BoostSingleRoutine(_sourceB, _defaultVolumeB, boosted, hold, fadeIn, fadeOut, () => _routineB = null));
    }

    private IEnumerator BoostSingleRoutine(
        AudioSource source,
        float defaultVolume,
        float boostedVolume,
        float holdDuration,
        float fadeInTime,
        float fadeOutTime,
        System.Action onFinish)
    {
        float start = source.volume;

        // 1) Fade In -> boosted
        yield return FadeSingle(source, start, boostedVolume, fadeInTime);

        // 2) Hold
        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        // 3) Fade Out -> default
        yield return FadeSingle(source, source.volume, defaultVolume, fadeOutTime);

        // 4) 兜底
        source.volume = defaultVolume;
        onFinish?.Invoke();
    }

    private IEnumerator FadeSingle(AudioSource source, float from, float to, float duration)
    {
        if (source == null) yield break;

        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(from, to, p);
            yield return null;
        }

        source.volume = to;
    }

    private void ApplyDefaultVolumes()
    {
        if (_sourceA != null) _sourceA.volume = _defaultVolumeA;
        if (_sourceB != null) _sourceB.volume = _defaultVolumeB;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelAudioData", menuName = "Audio/LevelAudioData")]
public class LevelAudioData: ScriptableObject
{
    List<AudioClip> bgmClips = new List<AudioClip>();
    /// <summary>
    /// 扫描时候的音效
    /// </summary>
    List<AudioClip> scanClips = new List<AudioClip>();
}
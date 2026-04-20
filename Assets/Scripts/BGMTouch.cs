using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundController.Instance.TempBoostWithFade(SoundController.SoundTarget.A, 1f, 0.5f, 0.5f, 0.5f);
        }
    }
}


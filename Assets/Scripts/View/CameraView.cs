using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    public void AutoMove(float time,Vector2 targetPos)
    {
        transform.DOMove(targetPos, time);
    }
}

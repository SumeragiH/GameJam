using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanPointView : MonoBehaviour
{
    //如果玩家碰撞到这个物体
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectionSystem.Instance.CollectScanPoint(this);
            gameObject.SetActive(false);
        }
    }

    //重置扫描点的方法，在玩家死亡或重新开始时调用
    public void ResetScanPoint()
    {
        gameObject.SetActive(true);
    }
}

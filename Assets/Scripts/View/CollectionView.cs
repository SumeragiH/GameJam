using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionView : MonoBehaviour
{
    //如果玩家碰撞到这个物体
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectionSystem.Instance.CollectItem();
            gameObject.SetActive(false);
        }
    }
}

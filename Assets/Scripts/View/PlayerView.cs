using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public bool isGrounded = true; // 是否在地面上
    public bool isRunning = false; // 是否正在跑动
    public bool isMovementEnabled = true; // 是否启用移动
    void Start()
    {
        EventCenter.Instance.AddListener<float>("左右移动", OnHorizontalMove);
        //EventCenter.Instance.AddListener<float>("上下移动", OnVerticalMove);
        EventCenter.Instance.AddListener("跳跃", OnJump);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnHorizontalMove(float horizontal)
    {
        if (!isMovementEnabled)
        {
            return; // 如果移动被禁用，直接返回
        }
        else
        {
            // 处理水平移动逻辑
            //TODO
        }
    }

    private void OnVerticalMove(float vertical)
    {
        if (!isMovementEnabled)
        {
            return; // 如果移动被禁用，直接返回
        }
        else
        {
            // 处理垂直移动逻辑
            //TODO
        }
    }


    private void OnJump()
    {
        if (!isGrounded)
        {
            return; // 如果不在地面上，直接返回
        }
        else
        {
            // 处理跳跃逻辑
            //TODO
        }
    }

}

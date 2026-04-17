using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputSystem : MonoBehaviour
{
    public bool isInputEnabled = true; // 是否启用输入

    public float horizontalInput { get; private set; }
    public float verticalInput { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInputEnabled)
        {
            return; // 如果输入被禁用，直接返回
        }
        else
        {
            CheckInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            CheckJump(Input.GetKeyDown(KeyCode.Space));
        }

    }

    private void CheckInput(float horizontal, float vertical)
    {
        horizontalInput = horizontal;
        verticalInput = vertical;
        EventCenter.Instance.EventTrigger("左右移动", horizontalInput);
        EventCenter.Instance.EventTrigger("上下移动", verticalInput);
    }

    private void CheckJump(bool isJump)
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            EventCenter.Instance.EventTrigger("跳跃");
        }
    }

}

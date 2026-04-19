using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputSystem : SingletonBaseWithMono<UserInputSystem>
{
    public bool isInputEnabled = true; // 是否启用输入

    public float horizontalInput { get; private set; }
    public float verticalInput { get; private set; }

    private float shiftPressTimer = 0f;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (shiftPressTimer > 0f)
        {
            shiftPressTimer -= Time.deltaTime;
        }

        if (!isInputEnabled)
        {
            return; // 如果输入被禁用，直接返回
        }
        else
        {
            CheckInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            CheckJump(Input.GetKeyDown(KeyCode.Space));
            CheckShift();
        }
    }

    private void CheckShift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (shiftPressTimer <= 0f)
            {
                shiftPressTimer = GameSystem.Instance.ShiftPressInterval;
            }
            else
            {
                return;
            }
            if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Alpha4))
            {
                int coverIndex = -1;
                if (Input.GetKey(KeyCode.Alpha1))
                {
                    coverIndex = 0;
                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    coverIndex = 1;
                }
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    coverIndex = 2;
                }
                else if (Input.GetKey(KeyCode.Alpha4))
                {
                    coverIndex = 3;
                }
                EventCenter.Instance.EventTrigger("选择遮罩序号", coverIndex);
            }
            else
            {
                EventCenter.Instance.EventTrigger("shift按下");
            }
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
        if (isJump)
        {
            EventCenter.Instance.EventTrigger("跳跃");
        }
    }

}

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
    private int selectedCoverIndex = -1;

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
            CheckNumber();
            CheckShift();
        }
    }

    private void CheckNumber()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (selectedCoverIndex == 0)
                selectedCoverIndex = -1; // 再次按下同一数字键取消选择
            else
                selectedCoverIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (selectedCoverIndex == 1)
                selectedCoverIndex = -1; // 再次按下同一数字键取消选择
            else
                selectedCoverIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (selectedCoverIndex == 2)
                selectedCoverIndex = -1; // 再次按下同一数字键取消选择
            else
                selectedCoverIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (selectedCoverIndex == 3)
                selectedCoverIndex = -1; // 再次按下同一数字键取消选择
            else
                selectedCoverIndex = 3;
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
            if (selectedCoverIndex != -1)
            {
                EventCenter.Instance.EventTrigger("选择遮罩序号", selectedCoverIndex);
                selectedCoverIndex = -1;
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

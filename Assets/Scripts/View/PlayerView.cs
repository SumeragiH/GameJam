using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : SingletonBaseWithMono<PlayerView>
{
    [Header("玩家参数")]
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float jumpForce = 5f; // 跳跃力度
    [Header("玩家状态")]
    public bool isGrounded = true; // 是否在地面上
    public bool isRunning = false; // 是否正在跑动
    public bool isIdle = true;//是否处于待机动画
    [Header("玩家限制")]
    public bool isMovementEnabled = true; // 是否启用移动
    public bool isJumpEnabled = true;//是否启用跳跃

    private Rigidbody2D rigidbody2D; // 玩家刚体组件
    private Animator animator;//玩家动画组件


    void Start()
    {
        if (rigidbody2D == null)
            rigidbody2D = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        isDestroyEnable = true; // 设置为true，允许销毁实例
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
            this.transform.Translate(Vector3.right * horizontal * moveSpeed * Time.deltaTime); // 示例水平移动逻辑

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
        if (!isGrounded&&isJumpEnabled)//在地上并且可以跳跃
        {
            return; // 如果不在地面上，直接返回
        }
        else
        {
            // 处理跳跃逻辑
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<float>("左右移动", OnHorizontalMove);
        //EventCenter.Instance.RemoveListener<float>("上下移动", OnVerticalMove);
        EventCenter.Instance.RemoveListener("跳跃", OnJump);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // 碰到地面，设置为在地面上
        }
    }
    //public void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = false; // 离开地面，设置为不在地面上
    //    }
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : SingletonBaseWithMono<PlayerView>
{
    [Header("玩家参数")]
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float jumpForce = 5f; // 跳跃力度
    [SerializeField] private float doubleJumpforce = 3f; // 二段跳的跳跃力度

    [Header("玩家状态")]
    public bool isGrounded = true; // 是否在地面上
    public bool isRunning = false; // 是否正在跑动
    public bool isIdle = true;//是否处于待机动画
    public bool isJumping = false;//是否处于跳跃动画
    public bool isFalling = false;//是否处于下落动画
    public bool isDoubleJumping = false;//是否处于二段跳动画

    [Header("玩家限制")]
    public bool isMovementEnabled = true; // 是否启用移动
    public bool isJumpEnabled = true;//是否启用跳跃
    

    private new Rigidbody2D rigidbody2D; // 玩家刚体组件
    private Animator anim;//玩家动画组件

    private int jumpCount = 0; // 跳跃计数器
    void Start()
    {
        if (rigidbody2D == null)
            rigidbody2D = GetComponent<Rigidbody2D>();
        //if (anim == null)
        //    anim = GetComponent<Animator>();

        isDestroyEnable = true; // 设置为true，允许销毁实例

        EventCenter.Instance.AddListener<float>("左右移动", OnHorizontalMove);
        //EventCenter.Instance.AddListener<float>("上下移动", OnVerticalMove);
        EventCenter.Instance.AddListener("跳跃", OnJump);
    }

        // Update is called once per frame
    void Update()
    {
        if (isGrounded == true)
        {
            // 取消跳跃动画
            //anim.SetBool("isJumping", false);
        }
        else
        {
            // （在空中时）播放跳跃动画
            //anim.SetBool("isJumping", true);
        }
    }

    //
    public void SetPlayerView(CheckPointData checkPointData)
    {
        this.transform.position = checkPointData.playerPosition; // 设置玩家位置
    }

    private void OnHorizontalMove(float horizontal)
    {
        if (!isMovementEnabled)
        {
            return; // 如果移动被禁用，直接返回
        }
        else
        {
            if (horizontal > 0)
            {
                this.transform.rotation =Quaternion.AngleAxis(0,Vector3.up); // 向右移动，保持默认朝向
                this.transform.Translate(Vector3.right * horizontal * moveSpeed * Time.deltaTime, Space.Self); // 示例水平移动逻辑
                //Debug.Log("向右移动"+horizontal);
            }
            else if (horizontal < 0)
            {
                this.transform.rotation = Quaternion.AngleAxis(180, Vector3.up); // 向左移动，翻转精灵
                this.transform.Translate(Vector3.right * -horizontal * moveSpeed * Time.deltaTime, Space.Self); // 示例水平移动逻辑
                //Debug.Log("向左移动"+horizontal);
            }

            // 处理水平移动逻辑

            //anim.SetInteger("xSpeed", Mathf.Abs(Mathf.RoundToInt(horizontal))); // 设置动画参数，根据水平输入调整动画状态

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
        if (!isJumpEnabled)//不在地上或者不可以跳跃
        {
            return; // 如果不在地面上，直接返回
        }
        if (jumpCount >= 2)
        {
            return; // 如果已经跳跃两次，直接返回
        }

        jumpCount++; // 增加跳跃计数器

        float currentJumpForce = (jumpCount==2)?doubleJumpforce:jumpForce; // 跳跃力度，根据跳跃次数选择普通跳跃或二段跳的力度

        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0f);// 重置竖直速度，确保每次跳跃都能获得相同的跳跃力度
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        //更新动画状态
        isJumping = (jumpCount == 1);
        isDoubleJumping = (jumpCount == 2);
        isFalling = false;

        //anim.SetTrigger("isJump"); // 播放起跳动画

        Debug.Log(jumpCount == 2 ? "二段跳" : "跳跃");

    }
    /// <summary>
    /// 一个重置方法，用于在玩家死亡或重新开始时重置玩家的位置和状态等信息
    /// </summary>
    /// <param name="checkPointData"></param>
    public void Reset(CheckPointData checkPointData)
    {
        isGrounded = true;
        isRunning = false;
        isIdle = true;
        isJumping = false;
        isFalling = false;
        isDoubleJumping = false;
        isMovementEnabled = true;
        isJumpEnabled = true;
        jumpCount = 0; // 重置跳跃计数器

        this.transform.position = checkPointData.playerPosition; // 重置玩家位置
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            //anim.SetBool("isJumping", false);//触碰到地面上的时候取消跳跃动画
            jumpCount = 0;

            isJumping = false;
            isDoubleJumping = false;
            isFalling = false;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // 离开地面，设置为不在地面上
            //anim.SetBool("isJumping", true);//在空中的时候播放跳跃动画
            //Debug.Log("离开地面");
        }
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<float>("左右移动", OnHorizontalMove);
        //EventCenter.Instance.RemoveListener<float>("上下移动", OnVerticalMove);
        EventCenter.Instance.RemoveListener("跳跃", OnJump);
    }

}

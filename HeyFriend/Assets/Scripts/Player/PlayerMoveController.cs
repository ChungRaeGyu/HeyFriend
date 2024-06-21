using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMoveController : MonoBehaviourPunCallbacks
{
    PlayerInputController playerInputController;
    Rigidbody2D rigidBody2D;
    Animator animator;
    SpriteRenderer spriteRenderer;
    PhotonView PV;
    int jumpParamToHash = Animator.StringToHash("Jump");
    int speedParamToHash = Animator.StringToHash("Speed");
    private Vector2 moveDirection;
    private Vector2 parentMove;
    public float gravityscale;
    public float velocty_y;
    public float jumpPower;
    public List<Rigidbody2D> topPlayer;

    private bool isMove=false;
    [SerializeField] private float speed;

    // Start is called before the first frame update
    private void Awake() {
        playerInputController = GetComponent<PlayerInputController>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        PV = GetComponent<PhotonView>();
    }
    void Start()
    {
        if (!pv.IsMine) return;
        playerInputController.OnMoveEvent+= OnMove;
        playerInputController.OnJumpEvent+=JumpMoveMent;
    }
    void FixedUpdate()
    {
        if (!pv.IsMine) return;
        MoveMent();
    }
    private void JumpMoveMent()
    {
        if(Math.Round(rigidBody2D.velocity.y,2)==0){
            rigidBody2D.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetTrigger(jumpParamToHash);
        }
    }
    private void OnMove(Vector2 direction){
        moveDirection = direction* speed;
    }
    private void MoveMent()
    {
        moveDirection.y = rigidBody2D.velocity.y- gravityscale;
        rigidBody2D.velocity = moveDirection+ parentMove;
        if (moveDirection.x != 0) PV.RPC("FilpXRPC", RpcTarget.AllBuffered, moveDirection.x);
        animator.SetFloat(speedParamToHash, Mathf.Abs(rigidBody2D.velocity.x));

        foreach(Rigidbody2D rb in topPlayer)
        {
            Vector2 dir = moveDirection * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + dir);
        }
    }

    [PunRPC]
    void FilpXRPC(float x)
    {
        spriteRenderer.flipX = x <= 0; /*? true : false;*/
    }


    public void JumpAction(float jumpPower)
    {
        Debug.Log("점프액션");
        rigidBody2D.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        animator.SetTrigger(jumpParamToHash);
    }

    public void AddParentVelocity(float velocity_X){
        parentMove.x=velocity_X;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isFoot = collision.contacts[0].normal == Vector2.down;

        if (collision.gameObject.gameObject.CompareTag("Player") && isFoot)
            topPlayer.Add(collision.gameObject.GetComponent<Rigidbody2D>());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (topPlayer.Contains(rb)) topPlayer.Remove(rb);
    }

}

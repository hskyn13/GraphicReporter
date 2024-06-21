using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator ani;
    SpriteRenderer spriteRenderer;
public float maxSpeed;
public float jumpPower;
private bool isGrounded = false;

private bool isJumping = false;

void Awake()
{
    rigid = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    ani = GetComponent<Animator>();
}

private void Update()
{
    if (Input.GetButtonDown("Jump") && !isJumping )
    {
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        isJumping = true;
        isGrounded = false;
        ani.SetBool("isJumping", true);
        
    }

    if (Input.GetButtonUp("Horizontal"))
    {
        // 정지
        rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
    }

    // 방향전환
    if (Input.GetButtonDown("Horizontal"))
    {
        spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
    }

    if (Mathf.Abs(rigid.velocity.x) < 0.3f)
    {
        ani.SetBool("isWalk", false);
    }
    else
    {
        ani.SetBool("isWalk", true);
    }

    // 상승 및 하강 상태 감지 및 설정
    if (rigid.velocity.y > 0)
    {
        ani.SetBool("isFalling", false);
    }
    else if (rigid.velocity.y < 0)
    {
        ani.SetBool("isFalling", true);
    }

    CheckGround();
    ani.SetBool("isGrounded",isGrounded);
}

void FixedUpdate()
{
    float h = Input.GetAxisRaw("Horizontal");

    rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

    if (rigid.velocity.x > maxSpeed) // 오른쪽
    {
        rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
    }
    else if (rigid.velocity.x < maxSpeed * (-1)) // 왼쪽
    {
        rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
    }

    // 착지 감지
    if (isJumping && rigid.velocity.y <= 0)
    {
        Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider != null && rayHit.distance < 0.5f)
        {
            ani.SetBool("isJumping", false);
            ani.SetBool("isFalling", false);
            ani.SetTrigger("isLanding");
            isJumping = false;
        }
    }
    else if (rigid.velocity.y < 0 && !isJumping)
    {
        ani.SetBool("isFalling", true);
    }
}

void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.tag == "Enemy")
    {
        OnDamaged(collision.transform.position);
    }
}

void OnDamaged(Vector2 targetPos)
{
    gameObject.layer = 11;

    spriteRenderer.color = new Color(1, 1, 1, 0.4f);

    int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
    rigid.AddForce(new Vector2(dirc, 1) * 15, ForceMode2D.Impulse);

    ani.SetTrigger("isDamaged");

    Invoke("OffDamaged", 2);
}

void OffDamaged()
{
    gameObject.layer = 10;
    spriteRenderer.color = new Color(1, 1, 1, 1);
}

void CheckGround()
{
    // 캐릭터의 발 위치에서 레이캐스트 발사
    Vector2 position = new Vector2(rigid.position.x, rigid.position.y - 0.2f);
    Debug.DrawRay(position, Vector2.down * 0.35f, new Color(1, 0, 0));
    RaycastHit2D rayHit = Physics2D.Raycast(position, Vector2.down, 0.35f, LayerMask.GetMask("Platform"));

    if (rayHit.collider != null && rayHit.distance < 0.3f)
    {
        isGrounded = false;
    }
    else
    {
        isGrounded = true;
    }
}

}

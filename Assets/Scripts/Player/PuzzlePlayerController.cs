using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class PuzzlePlayerController : MonoBehaviour
{
    public Vector2 playerSpeed = Vector2.one;
    public bool hasTeddy = false;

    public Animator playerAnimator;
    private Rigidbody RB;
    private GroundCheck groundChecker;

    public Vector2 normalizedInputs;

    public bool isCrouched = false;
    public bool isBlocking = false;

    public int maxHealth = 200;
    private int curHealth = 0;

    public bool invulnerable = false;
    public float invulnerabilityTime = 0.25f;
    public float respawnInvulnerabilityTime = 2.0f;

    void Start()
    {
        curHealth = maxHealth;
        RB = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(2).gameObject.GetComponent<GroundCheck>();
    }

    void Update()
    {
        normalizedInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        isBlocking = false;
        isCrouched = false;

        if (groundChecker.isGrounded)
        {
            if (Input.GetButton("Crouch"))
                isCrouched = true;
            RB.velocity = new Vector3(normalizedInputs.x * playerSpeed.x * Time.fixedDeltaTime, RB.velocity.y, normalizedInputs.y * playerSpeed.y * Time.fixedDeltaTime);
            if (normalizedInputs.x > 0)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (normalizedInputs.x < 0)
                transform.localScale = new Vector3(1f, 1f, 1f);
        }

        playerAnimator.SetBool("InAir", !groundChecker.isGrounded);
        playerAnimator.SetBool("Crouched", isCrouched);
        groundChecker.isGrounded = false;
    }

    void Damage(int[] data)
    {
        if (!invulnerable)
        {
            int damage = data[0];
            int attackType = data[1];

            if (isBlocking)
            {
                if (isCrouched && attackType == 0 || !isCrouched && attackType == 1)
                    return;
            }
            curHealth -= damage;
            if (curHealth <= 0)
            {
                curHealth = 0;
                Die();
            }
            else
            {
                playerAnimator.SetTrigger("Hit");
                playerAnimator.Update(0.1f);
                playerAnimator.ResetTrigger("Hit");
                StartCoroutine(InvulnerableMode(invulnerabilityTime));
            }
        }
    }

    void Die()
    {
        invulnerable = true;
        Debug.LogError("Dead :(");
    }

    IEnumerator InvulnerableMode(float time)
    {
        invulnerable = true;
        yield return new WaitForSeconds(time);
        invulnerable = false;
    }
}

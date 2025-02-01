using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class FightPlayerController : MonoBehaviour
{
    public Vector2 playerSpeed = Vector2.one;
    public bool hasTeddy = false;

    public Animator playerAnimator;
    private Rigidbody RB;
    private GroundCheck groundChecker;

    public Vector2 normalizedInputs;
    public bool jumpInput = false;
    public bool isJumping = false;
    private float jumpHeight = 7f;

    private bool isCrouched = false;

    public bool kickInput = false;
    private float airKickVelocity = 5f;
    public float timeInAir = 0f;
    private float maxAirKickTime = 0.5f;
    private float minAirKickTime = 0.15f;

    public bool jabInput = false;
    public int jabNum = 0;
    public float timeSinceJab = 0;
    private float maxJabInterval = 0.6f;

    public bool isBlocking = false;

    public int minKnockout = 50;
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

        if (Input.GetButtonDown("Jump"))
            jumpInput = true;

        if (Input.GetButtonDown("Kick") && !isBlocking)
            kickInput = true;

        if (Input.GetButtonDown("Jab"))
            jabInput = true;
    }

    void FixedUpdate()
    {
        //Debug.Log(playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        isBlocking = false;
        playerAnimator.SetBool("Walking", false);
        //isCrouched = false;
        if (jabNum != 0 && timeSinceJab <= maxJabInterval)
        {
            timeSinceJab += Time.fixedDeltaTime;
        }
        if (timeSinceJab > maxJabInterval)
        {
            jabNum = 0;
            playerAnimator.SetInteger("CurrentJab", jabNum);
            timeSinceJab = 0;
        }
        if (isJumping && jumpInput)
        {
            groundChecker.isGrounded = false;
            jumpInput = false;
        }
        else if (groundChecker.isGrounded)
        {
            if (!isJumping)
                RB.velocity = new Vector3(0, RB.velocity.y, 0);
            timeInAir = 0;

            if (!isBlocking && normalizedInputs.magnitude > 0.075f)
            {
                int currentState = playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                if (currentState == -66248418 || currentState == 375856309)
                    playerAnimator.SetBool("Walking", true);
            }
            if (Input.GetButton("Block"))
            {
                isBlocking = true;
                playerAnimator.SetBool("Walking", false);
            }
            //if (Input.GetButton("Crouch"))
            //    isCrouched = true;
            if (jabInput && !isBlocking)
            {
                int currentState = playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                // Check if trigger did anything
                if (currentState == -66248418 || currentState == 375856309)
                {
                    playerAnimator.SetTrigger("JabInput");
                    playerAnimator.SetBool("Walking", false);
                    timeSinceJab = 0;
                    ++jabNum;
                    if (jabNum > 2 || isCrouched && jabNum > 1)
                        jabNum = 0;
                    playerAnimator.SetInteger("CurrentJab", jabNum);
                }
            }
            if (!isBlocking && kickInput)
                playerAnimator.SetTrigger("KickInput");
        }
        else
        {
            if (kickInput && timeInAir < maxAirKickTime && timeInAir >= minAirKickTime)
            {
                playerAnimator.SetTrigger("KickInput");
                playerAnimator.SetBool("Walking", false);
                timeInAir = maxAirKickTime;
                RB.velocity = new Vector3(RB.velocity.x + transform.localScale.x * airKickVelocity, 0f, 0f);
            }
            timeInAir += Time.fixedDeltaTime;
        }

        playerAnimator.SetBool("InAir", !groundChecker.isGrounded);
        //playerAnimator.SetBool("Crouched", isCrouched);
        playerAnimator.SetBool("Blocking", isBlocking);
        if (!groundChecker.isGrounded || isBlocking)
            playerAnimator.SetBool("Walking", false);

        if (groundChecker.isGrounded)
        {
            if (playerAnimator.GetBool("Walking"))
            {
                RB.velocity = new Vector3(normalizedInputs.x * playerSpeed.x, RB.velocity.y, normalizedInputs.y * playerSpeed.y);
                if (normalizedInputs.x > 0)
                    transform.localScale = new Vector3(1f, 1f, 1f);
                else if (normalizedInputs.x < 0)
                    transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            // Got jump input
            if (!isJumping && jumpInput)
            {
                // Jump:
                isJumping = true;
                groundChecker.isGrounded = false;
                RB.velocity *= 0.65f;
                RB.velocity = new Vector3(RB.velocity.x, RB.velocity.y + jumpHeight, RB.velocity.z);
                playerAnimator.SetTrigger("Jump");
                playerAnimator.SetBool("Walking", false);
            }
            else if (isJumping)
            {
                isJumping = false;
            }
        }

        groundChecker.isGrounded = false;
        jabInput = false;
        kickInput = false;
    }

    void Damage(int[] data)
    {
        if (!invulnerable)
        {
            int damage = data[0];
            int attackType = data[1];
            int attackDir = data[2];

            if (isBlocking) //  && attackDir == transform.localScale.x
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

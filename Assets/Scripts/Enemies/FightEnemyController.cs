using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class FightEnemyController : MonoBehaviour
{
    public Vector2 enemySpeed = Vector2.one;

    public Animator enemyAnimator;
    private Rigidbody RB;
    private GroundCheck groundChecker;

    public Vector2 normalizedInputs;
    public bool jumpInput = false;
    public bool isJumping = false;
    private float jumpHeight = 4f;

    public bool isCrouched = false;

    public bool kickInput = false;
    private float airKickVelocity = 5f;
    public float timeInAir = 0f;
    private float maxAirKickTime = 0.5f;
    private float minAirKickTime = 0.15f;

    public bool jabInput = false;
    public int jabNum = 0;
    public float timeSinceJab = 0;
    private float maxJabInterval = 0.45f;

    public bool isBlocking = false;

    public int minKnockout = 50;
    public int maxHealth = 200;
    public int curHealth = 0;

    public bool invulnerable = false;
    public float invulnerabilityTime = 0.25f;

    public bool engaged = false;

    void Start()
    {
        curHealth = maxHealth;
        RB = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(2).gameObject.GetComponent<GroundCheck>();
    }

    void Update()
    {
        //normalizedInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        //if (Input.GetButtonDown("Jump"))
        //    jumpInput = true;

        //if (Input.GetButtonDown("Kick") && !isBlocking)
        //    kickInput = true;

        //if (Input.GetButtonDown("Jab"))
        //    jabInput = true;
    }

    void FixedUpdate()
    {
        isBlocking = false;
        isCrouched = false;
        if (jabNum != 0 && timeSinceJab <= maxJabInterval)
        {
            timeSinceJab += Time.fixedDeltaTime;
        }
        if (timeSinceJab > maxJabInterval)
        {
            jabNum = 0;
            enemyAnimator.SetInteger("CurrentJab", jabNum);
            timeSinceJab = 0;
        }
        if (isJumping && jumpInput)
        {
            groundChecker.isGrounded = false;
            jumpInput = false;
        }
        else if (groundChecker.isGrounded)
        {
            timeInAir = 0;
            if (Input.GetButton("Block"))
                isBlocking = true;
            if (Input.GetButton("Crouch"))
                isCrouched = true;
            if (jabInput && !isBlocking)
            {
                int currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                enemyAnimator.SetTrigger("JabInput");
                enemyAnimator.Update(0.1f);
                enemyAnimator.ResetTrigger("JabInput");
                // Check if trigger did anything
                if (currentState != enemyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash)
                {
                    timeSinceJab = 0;
                    ++jabNum;
                    if (jabNum > 2 || isCrouched && jabNum > 1)
                        jabNum = 0;
                    enemyAnimator.SetInteger("CurrentJab", jabNum);
                }
            }
            if (!isBlocking && kickInput)
                enemyAnimator.SetTrigger("KickInput");
            RB.velocity = new Vector3(normalizedInputs.x * enemySpeed.x * Time.fixedDeltaTime, RB.velocity.y, normalizedInputs.y * enemySpeed.y * Time.fixedDeltaTime);
            if (normalizedInputs.x > 0)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (normalizedInputs.x < 0)
                transform.localScale = new Vector3(1f, 1f, 1f);
            // Got jump input
            if (!isJumping && jumpInput)
            {
                // Jump:
                isJumping = true;
                groundChecker.isGrounded = false;
                RB.velocity *= 0.65f;
                RB.velocity = new Vector3(RB.velocity.x, RB.velocity.y + jumpHeight, RB.velocity.z);
                enemyAnimator.SetTrigger("Jump");
            }
            else if (isJumping)
            {
                isJumping = false;
            }
        }
        else
        {
            if (kickInput && timeInAir < maxAirKickTime && timeInAir >= minAirKickTime)
            {
                enemyAnimator.SetTrigger("KickInput");
                timeInAir = maxAirKickTime;
                RB.velocity = new Vector3(RB.velocity.x - transform.localScale.x * airKickVelocity, 0f, 0f);
            }
            timeInAir += Time.fixedDeltaTime;
        }

        enemyAnimator.SetBool("InAir", !groundChecker.isGrounded);
        enemyAnimator.SetBool("Crouched", isCrouched);
        enemyAnimator.SetBool("Blocking", isBlocking);
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

            Debug.Log("Damage: " + damage);
            curHealth -= damage;
            if (curHealth <= 0)
            {
                curHealth = 0;
                Die();
            }
            else
            {
                enemyAnimator.SetTrigger("Hit");
                enemyAnimator.Update(0.1f);
                enemyAnimator.ResetTrigger("Hit");
                Debug.Log("Invulnerable!");
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

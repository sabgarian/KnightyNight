using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class FightEnemyController : MonoBehaviour
{
    public float entryTime = 2f;
    public bool mainEnemy = false;
    public Vector2 EngagementTimeRange = Vector2.one;

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

    private EngagementManager engagementManager;
    public bool engaged = false;

    public Vector2 bystanderRangeMin;
    public Vector2 bystanderRangeMax;

    public Vector2 intimidationRangeMin;
    public Vector2 intimidationRangeMax;
    public Vector2 attackInterval;

    public Vector2 attackRange = new Vector2(1, 3);
    public Vector2 attackHesitation = new Vector2(0.2f, 0.5f);

    public float attackTime = 0f;
    public float maxAttackTime = 3f;

    public float timeSinceAttack = 0f;
    public float currentAttackInterval = 10f;

    public Vector2 randomInputTime = new Vector2(0.5f, 2f);
    public float currentRandomInputTime = 0f;
    public float maxRandomInputTime = 0f;

    public Vector2 pauseRange = new Vector2(0.25f, 0.75f);
    public float pauseTime = 0f;
    public float pauseLength = 0f;

    public Vector2 movementDecisionDelayRange = new Vector2(0.25f, 0.75f);
    public float movementDecisionDelayTime = 0f;
    public float movementDecisionDelay = 0f;
    public bool approaching = false;
    public bool retreating = false;

    public Vector2 currentRandomInputs = Vector2.zero;

    public bool isAttacking = false;

    public bool cutSceneMode = false;

    private Animator deathParticles;
    private int currentSmack = 0;
    public bool sequentialSmacks = true;
    public AudioClip[] smackSounds;
    public AudioClip[] finalSmackSounds;
    public AudioSource smackPlayer;

    void Start()
    {
        engagementManager = GameObject.FindWithTag("EngagementManager").GetComponent<EngagementManager>();
        if (mainEnemy)
            engagementManager.AddMainEnemy(gameObject, EngagementTimeRange);
        else
            engagementManager.AddEnemy(gameObject, EngagementTimeRange);

        curHealth = maxHealth;
        RB = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(2).gameObject.GetComponent<GroundCheck>();
        deathParticles = transform.GetChild(3).gameObject.GetComponent<Animator>();
        StartCoroutine(EnterCutScene());
    }

    IEnumerator EnterCutScene()
    {
        StartCutScene();
        normalizedInputs = new Vector2(1, 0);
        invulnerable = true;
        yield return new WaitForSeconds(entryTime);
        EndCutScene();
        invulnerable = false;
    }

    void Engage()
    {
        engaged = true;
    }

    void Disengage()
    {
        engaged = false;
    }

    void Update()
    {
        if (cutSceneMode)
            return;
        if (engaged)
            AttackUpdate();
        else
            WaitUpdate();

        //normalizedInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        //if (Input.GetButtonDown("Jump"))
        //    jumpInput = true;

        //if (Input.GetButtonDown("Kick") && !isBlocking)
        //    kickInput = true;

        //if (Input.GetButtonDown("Jab"))
        //    jabInput = true;
    }

    void AttackUpdate()
    {
        normalizedInputs = Vector3.zero;
        if (timeSinceAttack >= currentAttackInterval)
        {
            timeSinceAttack = 0f;
            currentAttackInterval = Random.Range(attackInterval.x, attackInterval.y);
            isAttacking = true;
            attackTime = 0f;
        }
        if (!isAttacking)
        {
            WaitUpdate();
        }
        else
        {
            if (attackTime >= maxAttackTime)
            {
                isAttacking = false;
                return;
            }
            attackTime += Time.deltaTime;
            Vector3 playerDir = (engagementManager.playerTrans.position - transform.position);
            Vector2 playerNormDir = new Vector2(playerDir.x, playerDir.z).normalized;
            Vector2 playerAbsDist = new Vector2(Mathf.Abs(playerDir.x), Mathf.Abs(playerDir.z));
            if (playerAbsDist.x <= attackRange.x && playerAbsDist.y <= attackRange.y)
            {
                jabInput = true;
                isAttacking = false;
                return;
            }
            normalizedInputs = playerNormDir;
        }
    }

    void WaitUpdate()
    {
        timeSinceAttack += Time.deltaTime;

        Vector3 playerDir = (engagementManager.playerTrans.position - transform.position);
        Vector2 playerNormDir = new Vector2(playerDir.x, playerDir.z).normalized;
        Vector2 playerAbsDist = new Vector2(Mathf.Abs(playerDir.x), Mathf.Abs(playerDir.z));
        if (approaching)
        {
            movementDecisionDelayTime += Time.deltaTime;
            normalizedInputs = playerNormDir;
            if (movementDecisionDelayTime >= movementDecisionDelay)
            {
                approaching = false;
                movementDecisionDelayTime = 0;
            }
        }
        else if (retreating)
        {
            movementDecisionDelayTime += Time.deltaTime;
            normalizedInputs = -playerNormDir;
            if (movementDecisionDelayTime >= movementDecisionDelay)
            {
                retreating = false;
                movementDecisionDelayTime = 0;
            }
        }
        else
        {
            if (engaged && playerAbsDist.x <= intimidationRangeMax.x && playerAbsDist.y <= intimidationRangeMax.y || !engaged && playerAbsDist.x <= bystanderRangeMax.x && playerAbsDist.y <= bystanderRangeMax.y)
            {
                if (engaged && playerAbsDist.x <= intimidationRangeMin.x && playerAbsDist.y <= intimidationRangeMin.y || !engaged && playerAbsDist.x <= bystanderRangeMin.x && playerAbsDist.y <= bystanderRangeMin.y)
                {
                    retreating = true;
                    movementDecisionDelay = Random.Range(movementDecisionDelayRange.x, movementDecisionDelayRange.y);
                }
                else
                {
                    if (currentRandomInputTime >= maxRandomInputTime)
                    {
                        pauseTime = 0f;
                        pauseLength = Random.Range(pauseRange.x, pauseRange.y);
                        currentRandomInputTime = 0f;
                        maxRandomInputTime = Random.Range(randomInputTime.x, randomInputTime.y);
                        currentRandomInputs = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    }
                    if (pauseTime <= pauseLength)
                    {
                        pauseTime += Time.deltaTime;
                    }
                    else
                    {
                        currentRandomInputTime += Time.deltaTime;
                        normalizedInputs = currentRandomInputs.normalized;
                    }
                }
            }
            else
            {
                approaching = true;
                movementDecisionDelay = Random.Range(movementDecisionDelayRange.x, movementDecisionDelayRange.y);
            }
        }
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
            //if (Input.GetButton("Block"))
            //    isBlocking = true;
            //if (Input.GetButton("Crouch"))
            //    isCrouched = true;
            if (jabInput && !isBlocking)
            {
                int currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                enemyAnimator.SetTrigger("Attack");
                enemyAnimator.Update(0.1f);
                enemyAnimator.ResetTrigger("Attack");
                // Check if trigger did anything
                if (currentState != enemyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash)
                {
                    timeSinceJab = 0;
                    ++jabNum;
                    if (jabNum > 2 || isCrouched && jabNum > 1)
                        jabNum = 0;
                    //enemyAnimator.SetInteger("CurrentJab", jabNum);
                }
            }
            //if (!isBlocking && kickInput)
            //    enemyAnimator.SetTrigger("KickInput");
            RB.velocity = new Vector3(normalizedInputs.x * enemySpeed.x, RB.velocity.y, normalizedInputs.y * enemySpeed.y);
            Vector3 playerDir = (engagementManager.playerTrans.position - transform.position);
            Vector2 playerNormDir = new Vector2(playerDir.x, playerDir.z).normalized;
            if (playerNormDir.x > 0)
                transform.localScale = new Vector3(1f, 1f, 1f);
            else if (playerNormDir.x < 0)
                transform.localScale = new Vector3(-1f, 1f, 1f);
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
        //enemyAnimator.SetBool("Crouched", isCrouched);
        //enemyAnimator.SetBool("Blocking", isBlocking);
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
                smackPlayer.clip = finalSmackSounds[Random.Range(0, finalSmackSounds.Length)];
                smackPlayer.Play();
                curHealth = 0;
                Die();
            }
            else
            {
                if (sequentialSmacks)
                {
                    smackPlayer.clip = smackSounds[currentSmack];
                    ++currentSmack;
                    if (currentSmack >= smackSounds.Length)
                        currentSmack = 0;
                }
                else
                {
                    smackPlayer.clip = smackSounds[Random.Range(0, smackSounds.Length)];
                }
                smackPlayer.Play();
                enemyAnimator.SetTrigger("Hit");
                enemyAnimator.Update(0.1f);
                enemyAnimator.ResetTrigger("Hit");
                StartCoroutine(InvulnerableMode(invulnerabilityTime));
                if (!engaged)
                    engagementManager.SkipWaitlist(gameObject);
                isAttacking = false;
            }
        }
    }

    void Die()
    {
        if (invulnerable)
            return;
        invulnerable = true;
        engagementManager.RemoveEnemy(gameObject);
        deathParticles.transform.parent = transform.parent;
        deathParticles.SetTrigger("Die");
        Destroy(gameObject);
        //Debug.LogError("Dead :(");
    }

    public void StartCutScene()
    {
        cutSceneMode = true;
        foreach (Transform child in transform)
        {
            BoxCollider col = child.gameObject.GetComponent<BoxCollider>();
            if (col != null && !col.isTrigger && child.gameObject.layer == 10)
                child.gameObject.layer = 13;
        }
    }

    public void EndCutScene()
    {
        cutSceneMode = false;
        foreach (Transform child in transform)
        {
            BoxCollider col = child.gameObject.GetComponent<BoxCollider>();
            if (col != null && !col.isTrigger && child.gameObject.layer == 13)
                child.gameObject.layer = 10;
        }
    }

    IEnumerator InvulnerableMode(float time)
    {
        invulnerable = true;
        yield return new WaitForSeconds(time);
        invulnerable = false;
    }
}

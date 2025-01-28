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

    public bool interactInput = false;
    private bool usedInteractInput = false;

    public Transform pushingObject = null;
    public bool pushDir;
    public int pushingLayer = 12;

    void Start()
    {
        curHealth = maxHealth;
        RB = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(2).gameObject.GetComponent<GroundCheck>();
    }

    void Update()
    {
        interactInput = false;

        if (Input.GetButton("Jab"))
            interactInput = true;
        else
            usedInteractInput = false;

        normalizedInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if (pushingObject != null)
        {
            if (!interactInput)
            {
                EndPush();
                return;
            }

            if (pushDir)
            {
                if (normalizedInputs.x < 0)
                {
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, -1, 0);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, -1, 0);
                }
                else
                {
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, 0, 1);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, 0, 1);
                }
            }
            else
            {
                if (normalizedInputs.x < 0)
                {
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, -1, 0);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, 0, 1);
                }
                else
                {
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, 0, 1);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, -1, 0);
                }
            }

            float minDir = 0;
            if (Mathf.Abs(normalizedInputs.x) < Mathf.Abs(normalizedInputs.y))
                minDir = Mathf.Abs(normalizedInputs.x);
            else
                minDir = Mathf.Abs(normalizedInputs.y);
            normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, -minDir, minDir);
            normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, -minDir, minDir);
        }
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
            {
                if (pushingObject != null)
                    pushingObject.transform.parent = null;
                transform.localScale = new Vector3(-1f, 1f, 1f);
                if (pushingObject != null)
                    pushingObject.transform.parent = transform;
            }
            else if (normalizedInputs.x < 0)
            {
                if (pushingObject != null)
                    pushingObject.transform.parent = null;
                transform.localScale = new Vector3(1f, 1f, 1f);
                if (pushingObject != null)
                    pushingObject.transform.parent = transform;
            }
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

    public void EndPush()
    {
        pushingObject.transform.parent = null;
        pushingObject = null;
    }

    public void TryPushing(Transform newObj, bool PushDir)
    {
        usedInteractInput = true;
        pushingObject = newObj;
        newObj.transform.parent = transform;
        newObj.gameObject.layer = pushingLayer;
        pushDir = PushDir;
    }
}

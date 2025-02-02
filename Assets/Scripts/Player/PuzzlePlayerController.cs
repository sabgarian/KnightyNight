using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class PuzzlePlayerController : MonoBehaviour
{
    private PuzzleLevelManager levelController;

    public Quaternion perspectiveRotation;

    public Vector2 playerSpeed = Vector2.one;
    public Vector2 pushSpeed = Vector2.one;
    public bool hasTeddy = false;

    public Animator playerAnimator;
    private Rigidbody RB;
    private GroundCheck groundChecker;

    public Vector2 normalizedInputs;

    public int maxHealth = 200;
    private int curHealth = 0;

    public bool invulnerable = false;
    public float invulnerabilityTime = 0.25f;
    public float respawnInvulnerabilityTime = 2.0f;

    public bool interactInput = false;
    [HideInInspector]
    public bool usedInteractInput = false;

    public Transform pushingObject = null;
    public bool pushDir;
    public bool pushSide;
    public int pushingLayer = 12;

    public int keyCount = 0;

    void Awake()
    {
        curHealth = maxHealth;
        RB = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(2).gameObject.GetComponent<GroundCheck>();
        //Freeze();
        levelController = GameObject.FindWithTag("MainCamera").GetComponent<PuzzleLevelManager>();
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

            bool newAnimDir = false;
            if (!pushDir)
            {
                if (normalizedInputs.x < 0)
                {
                    newAnimDir = !pushSide;
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, -1, 0);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, -1, 0);
                }
                else
                {
                    newAnimDir = pushSide;
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, 0, 1);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, 0, 1);
                }
            }
            else
            {
                if (normalizedInputs.x < 0)
                {
                    newAnimDir = !pushSide;
                    normalizedInputs.x = Mathf.Clamp(normalizedInputs.x, -1, 0);
                    normalizedInputs.y = Mathf.Clamp(normalizedInputs.y, 0, 1);
                }
                else
                {
                    newAnimDir = pushSide;
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
            if (normalizedInputs.magnitude != 0)
            {
                playerAnimator.SetBool("PushType", newAnimDir);
            }
        }
    }

    void FixedUpdate()
    {
        playerAnimator.SetBool("Walking", false);
        RB.velocity = new Vector3(0, RB.velocity.y, 0);

        if (groundChecker.isGrounded)
        {
            if (normalizedInputs.magnitude > 0.075f)
            {
                int currentState = playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                //Debug.Log(currentState);
                if (currentState == -66248418 || currentState == 375856309 || currentState == -1437861093 || currentState == 1618751900)
                    playerAnimator.SetBool("Walking", true);
            }
            if (playerAnimator.GetBool("Walking"))
            {
                if (pushingObject == null)
                {
                    RB.velocity = perspectiveRotation * new Vector3(normalizedInputs.x * playerSpeed.x, RB.velocity.y, normalizedInputs.y * playerSpeed.y);
                    if (normalizedInputs.x > 0 && transform.localScale.x < 0)
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    else if (normalizedInputs.x < 0 && transform.localScale.x > 0)
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    RB.velocity = perspectiveRotation * new Vector3(normalizedInputs.x * pushSpeed.x, RB.velocity.y, normalizedInputs.y * pushSpeed.y);
                    if (normalizedInputs.x > 0 && transform.localScale.x < 0)
                    {
                        pushingObject.transform.parent = null;
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                        pushingObject.transform.parent = transform;
                    }
                    else if (normalizedInputs.x < 0 && transform.localScale.x > 0)
                    {
                        pushingObject.transform.parent = null;
                        transform.localScale = new Vector3(1f, 1f, 1f);
                        pushingObject.transform.parent = transform;
                    }
                }
            }
        }

        playerAnimator.SetBool("InAir", !groundChecker.isGrounded);
        groundChecker.isGrounded = false;
    }

    public void Damage(int[] data)
    {
        if (!invulnerable)
            Die();
    }

    public void Kill(GameObject killer)
    {
        Damage(null);
    }

    public void Freeze()
    {
        RB.isKinematic = true;
    }

    public void Unfreeze()
    {
        RB.isKinematic = false;
    }

    void Die()
    {
        Debug.Log("Dead!");
        invulnerable = true;
        Freeze();
        levelController.StartCoroutine(levelController.LoadPuzzle());
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
        playerAnimator.SetBool("Pushing", false);
    }

    public void TryPushing(Transform newObj, bool PushDir, bool PushSide)
    {
        usedInteractInput = true;
        pushingObject = newObj;
        newObj.transform.parent = transform;
        newObj.gameObject.layer = pushingLayer;
        pushDir = PushDir;
        pushSide = PushSide;
        playerAnimator.SetBool("Pushing", true);
    }
}

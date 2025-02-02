using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public int stationaryLayer = 11;

    [HideInInspector]
    public Transform levelParent;

    public LayerMask playerLayer;
    private GroundCheck groundCheck;
    private PuzzlePlayerController playerScript;
    private Rigidbody RB;
    private bool falling = false;
    private bool pushable = true;

    void Start()
    {
        levelParent = transform.parent;
        playerScript = GameObject.FindWithTag("Player").GetComponent<PuzzlePlayerController>();
        groundCheck = transform.GetChild(0).gameObject.GetComponent<GroundCheck>();
        RB = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (transform.parent.gameObject.layer != 9)
            gameObject.layer = stationaryLayer;

        if (!groundCheck.isGrounded && !falling)
        {
            if (playerScript.pushingObject == transform)
                playerScript.EndPush();
            RB = gameObject.AddComponent<Rigidbody>();
            RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            RB.useGravity = true;
            falling = true;
            pushable = false;
        }
        else if (groundCheck.isGrounded && falling)
        {
            RB.useGravity = false;
            Destroy(RB);
            falling = false;
            pushable = true;
        }
        groundCheck.isGrounded = false;
    }

    public void OnTriggerEnter(Collider col)
    {
        PushCheck(col);
    }

    public void OnTriggerStay(Collider col)
    {
        PushCheck(col);
    }

    void PushCheck(Collider col)
    {
        if (!pushable)
            return;
        if (playerLayer == (playerLayer | (1 << col.gameObject.layer)))
        {
            if (playerScript != null && playerScript.interactInput && !playerScript.usedInteractInput && playerScript.pushingObject == null)
            {
                Vector3 pushDirection = playerScript.transform.position - transform.position;
                pushDirection = transform.rotation * pushDirection;

                bool pushDir = Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.z);
                bool pushSide = false;
                if (pushDir)
                    pushSide = pushDirection.x > 0;
                else
                    pushSide = pushDirection.z > 0;
                playerScript.TryPushing(transform, pushDir, pushSide);
            }
        }
    }
}

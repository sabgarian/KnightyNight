using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbyHand : MonoBehaviour
{
    public LayerMask playerAndObstacles;
    public Vector2 direction;
    public Animator grabAnim;
    private Transform playerTrans;
    public bool grabbing = false;
    public Transform handTrans;

    public float cooldownLength = 10f;
    private float cooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        cooldownTime = cooldownLength;
        playerTrans = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (grabbing)
        {
            int currentState = grabAnim.GetCurrentAnimatorStateInfo(0).fullPathHash;
            playerTrans.position = handTrans.position;
            Debug.Log(currentState);
            if (currentState == -66248418)
            {
                playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Unfreeze();
                grabbing = false;
            }
            return;
        }
        if (cooldownTime < cooldownLength)
        {
            cooldownTime += Time.deltaTime;
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, playerAndObstacles))
        {
            if (hit.collider.gameObject.layer == 9)
            {
                playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Freeze();
                grabbing = true;
                cooldownTime = 0f;
            }
        }
    }
}

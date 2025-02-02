using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterEnemy : MonoBehaviour
{
    public LayerMask playerAndObstacles;
    public float curRot = 0f;
    public float frustumSize = 85f;
    public float comprehensionTime = 3f;
    public float seeingDistance = 100f;
    private float huntSpeed = 10f;
    private float returnSpeed = 10f;

    private bool playerSpotted = false;
    private float playerSpottedTime = 0f;
    private bool hunting = false;
    private bool returning = false;
    private Transform playerTrans;

    private MusicManager musicBox;
    private Animator monsterAnim;

    void Start()
    {
        musicBox = GameObject.FindWithTag("MainCamera").GetComponent<MusicManager>();
        playerTrans = GameObject.FindWithTag("Player").transform;
        monsterAnim = transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        float angleToTarget = Mathf.Atan2(transform.position.z - playerTrans.position.z, transform.position.x - playerTrans.position.x) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(curRot, angleToTarget);

        if (Mathf.Abs(angleDifference) <= frustumSize * 0.5f)
        {
            RaycastHit hit;
            //Debug.Log("casting ray");
            //Debug.DrawRay(transform.position, (playerTrans.position - transform.position).normalized * 1000f, Color.red);
            if (Physics.Raycast(transform.position, (playerTrans.position - transform.position).normalized, out hit, seeingDistance, playerAndObstacles))
            {
                //Debug.DrawRay(transform.position, (playerTrans.position - transform.position).normalized * 1000f, Color.yellow);
                //Debug.Log("rendering ray");
                if (hit.collider.gameObject.layer == 9)
                {
                    if (!playerSpotted)
                    {
                        playerSpotted = true;
                        gameObject.SendMessage("InterruptMovement");
                    }
                }
                else
                    playerSpotted = false;
            }
        }
        else
            playerSpotted = false;

        if (!playerSpotted && playerSpottedTime < 0)
        {
            playerSpottedTime = 0;
            gameObject.SendMessage("ResumeMovement");
        }
        if (!playerSpotted && playerSpottedTime > 0)
        {
            playerSpottedTime -= Time.deltaTime;
            if (playerSpottedTime < 0)
                musicBox.TransitionTo(1, -1, 0.5f);
        }
        else if (playerSpotted && playerSpottedTime < comprehensionTime)
        {
            if (playerSpottedTime <= 0)
                musicBox.TransitionTo(1, 2, 0.5f);
            playerSpottedTime += Time.deltaTime;
        }
        else if (playerSpottedTime >= comprehensionTime)
        {
            playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Kill(gameObject);
            monsterAnim.SetTrigger("Scare");
        }
    }

    void SetDirection(float rotation)
    {
        curRot = rotation;
    }
}

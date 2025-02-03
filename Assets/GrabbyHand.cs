using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbyHand : MonoBehaviour
{
    public LayerMask playerAndObstacles;
    public LayerMask movableObstacles;
    public Animator grabAnim;
    private Transform playerTrans;
    public bool grabbing = false;

    public Transform handTrans;
    public Transform armPivot;
    public Quaternion armRotOffset;
    public float armPivotScale = 10;
    public float handScale = 10;

    public float grabSpeed = 0.25f;
    public float reachingToGrabRatio = 0.25f;

    public float grabProgress = 0f;
    public float grabDistance = 0f;

    public float cooldownLength = 10f;
    private float cooldownTime;

    private bool holeBlocked = false;

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
            float smoothProgress = 0;
            grabProgress += Time.deltaTime * grabSpeed;
            if (grabProgress < reachingToGrabRatio)
                smoothProgress = Mathf.SmoothStep(0f, 1f, grabProgress / reachingToGrabRatio);
            else
                smoothProgress = Mathf.SmoothStep(1f, 0f, (grabProgress - reachingToGrabRatio) / (1 - reachingToGrabRatio));

            float newScale = smoothProgress * grabDistance * armPivotScale;
            armPivot.localScale = new Vector3(newScale, newScale, newScale);
            float newHandScale = handScale;
            if (newScale > 0)
                newHandScale = handScale / newScale;
            handTrans.localScale = new Vector3(newHandScale, newHandScale, newHandScale);

            if (grabProgress >= reachingToGrabRatio)
                playerTrans.position = new Vector3(handTrans.position.x, playerTrans.position.y, handTrans.position.z);
            if (grabProgress >= 1f || playerTrans.gameObject.GetComponent<PuzzlePlayerController>().invulnerable)
            {
                playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Unfreeze();
                grabbing = false;
                cooldownTime = 0f;
                grabAnim.SetBool("Grabbing", false);
                grabAnim.SetBool("Active", false);
            }
            return;
        }
        if (cooldownTime < cooldownLength)
        {
            grabAnim.SetBool("Active", false);
            cooldownTime += Time.deltaTime;
        }
        if (!grabAnim.GetBool("Active"))
            return;
        if (grabAnim.GetCurrentAnimatorStateInfo(0).fullPathHash == -1324054277)
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position, armRotOffset * armPivot.rotation * Vector3.forward * 1000f, Color.yellow);
            if (Physics.Raycast(armPivot.transform.position, armRotOffset * armPivot.rotation * Vector3.forward, out hit, Mathf.Infinity, playerAndObstacles))
            {
                if (hit.collider.gameObject.layer == 9)
                {
                    playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Freeze();
                    grabbing = true;
                    grabAnim.SetBool("Grabbing", true);
                    cooldownTime = 0f;
                    grabDistance = (playerTrans.transform.position - armPivot.transform.position).magnitude;
                    grabProgress = 0;
                }
            }
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        CheckHole(col);
    }

    public void OnTriggerStay(Collider col)
    {
        CheckHole(col);
    }

    public void FixedUpdate()
    {
        if (grabAnim.GetBool("Active") && holeBlocked)
        {
            grabAnim.SetBool("Active", false);
        }
        else if (!grabAnim.GetBool("Active") && !holeBlocked && cooldownTime >= cooldownLength)
        {
            grabAnim.SetBool("Active", true);
        }
        holeBlocked = false;
    }

    public void CheckHole(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (movableObstacles == (movableObstacles | (1 << col.gameObject.layer)))
        {
            //Debug.Log("yayayayya");
            holeBlocked = true;
        }
    }
}

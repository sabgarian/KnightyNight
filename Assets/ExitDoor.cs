using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public LayerMask playerLayer;
    public bool open = false;

    public int lockCount = 0;
    private BoxCollider col;
    private PuzzlePlayerController playerScript;

    public bool blockedDoor = false;
    public bool playerInDoor = false;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = GameObject.FindWithTag("Player").GetComponent<PuzzlePlayerController>();
        col = transform.GetChild(0).gameObject.GetComponent<BoxCollider>();
    }

    public void OnTriggerEnter(Collider col)
    {
        UnlockCheck(col);
    }

    public void OnTriggerStay(Collider col)
    {
        UnlockCheck(col);
    }

    void FixedUpdate()
    {
        if (!blockedDoor && playerInDoor)
        {
            if (playerScript != null && playerScript.interactInput && !playerScript.usedInteractInput)
            {
                if (!open)
                {
                    playerScript.usedInteractInput = true;
                    if (playerScript.keyCount >= lockCount)
                    {
                        playerScript.keyCount -= lockCount;
                        lockCount = 0;
                        open = true;
                    }
                }
                else
                {
                    playerScript.usedInteractInput = true;
                    GameObject.FindWithTag("MainCamera").GetComponent<PuzzleLevelManager>().FinishPuzzle();
                }
            }
        }
        blockedDoor = false;
        playerInDoor = false;
    }

    void UnlockCheck(Collider col)
    {
        if (playerLayer == (playerLayer | (1 << col.gameObject.layer)))
        {
            if (col.gameObject.layer == 9)
                playerInDoor = true;
            else
                blockedDoor = true;
        }
    }
}

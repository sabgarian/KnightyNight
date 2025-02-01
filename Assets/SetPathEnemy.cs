using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathEnemy : MonoBehaviour
{
    public List<Vector3> pathNodes = new List<Vector3>();
    public float movementSpeed = 2f;
    public bool playing = true;

    private Animator monsterAnim;
    private int oldNode = 0;
    private int newNode = 0;
    private float nodeProgress = 1f;
    private float nodeSpeed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        monsterAnim = transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing || movementSpeed == 0)
            return;
        monsterAnim.SetBool("Walking", true);
        if (nodeProgress >= 1)
        {
            nodeProgress -= 1f;
            oldNode = newNode;
            ++newNode;
            if (newNode >= pathNodes.Count)
                newNode = 0;
            Vector3 newDir = pathNodes[oldNode] - pathNodes[newNode];
            nodeSpeed = newDir.magnitude / movementSpeed;
            gameObject.SendMessage("SetDirection", Mathf.Atan2(newDir.z, newDir.x) * Mathf.Rad2Deg, SendMessageOptions.DontRequireReceiver);
        }
        transform.position = Vector3.Lerp(pathNodes[oldNode], pathNodes[newNode], nodeProgress);
        if (pathNodes[oldNode].x > pathNodes[newNode].x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
        nodeProgress += Time.deltaTime / nodeSpeed;
    }

    void InterruptMovement()
    {
        monsterAnim.SetBool("Walking", false);
        playing = false;
    }

    void ResumeMovement()
    {
        monsterAnim.SetBool("Walking", true);
        playing = true;
    }
}

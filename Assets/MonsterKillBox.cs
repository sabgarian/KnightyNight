using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterKillBox : MonoBehaviour
{
    private bool hasKilled = false;
    private Animator monsterAnim;

    // Start is called before the first frame update
    void Start()
    {
        monsterAnim = transform.parent.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        HitSomething(col);
    }

    void OnTriggerStay(Collider col)
    {
        HitSomething(col);
    }

    void HitSomething(Collider col)
    {
        if (hasKilled)
            return;

        if (col.transform.parent.gameObject.layer == 9)
        {
            hasKilled = true;
            col.gameObject.transform.parent.gameObject.SendMessage("Kill", gameObject);
            transform.parent.parent.gameObject.SendMessage("InterruptMovement");
            monsterAnim.SetTrigger("Scare");
        }
    }
}

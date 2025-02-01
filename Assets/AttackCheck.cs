using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    public int damage = 0;
    public int attackType = 0;

    // Start is called before the first frame update
    void Start()
    {
        
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
        if (transform.parent.gameObject.layer != col.gameObject.layer)
        {
            //Debug.Log("hit " + transform.parent.gameObject.name);
            col.gameObject.transform.parent.gameObject.SendMessage("Damage", new int[] { damage, attackType, -(int)transform.parent.parent.localScale.x });
        }
    }
}

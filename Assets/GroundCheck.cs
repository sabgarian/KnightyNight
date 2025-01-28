using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded = false;
    public LayerMask validGround;

    void OnTriggerStay(Collider col)
    {
        if (validGround == (validGround | (1 << col.gameObject.layer)))
            isGrounded = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeddyPickup : MonoBehaviour
{
    private float bobProgress = 0;
    public float bobTime = 1f;
    public float bobMax = 1f;
    public float bobMin = 1f;
    public bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float halfBobTime = bobTime * 0.5f;
        if (bobProgress < halfBobTime)
        {
            float smoothed = Mathf.SmoothStep(0, 1f, bobProgress / halfBobTime);
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(bobMin, bobMax, smoothed), transform.localPosition.z);
        }
        else
        {
            float smoothed = Mathf.SmoothStep(0, 1f, (bobProgress - halfBobTime) / halfBobTime);
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(bobMax, bobMin, smoothed), transform.localPosition.z);
        }

        bobProgress += Time.deltaTime;
        if (bobProgress >= bobTime)
            bobProgress -= bobTime;
    }

    void OnTriggerEnter()
    {
        if (triggered)
            return;
        GameObject.FindWithTag("MainCamera").GetComponent<PuzzleLevelManager>().SwitchToFight();
        triggered = true;
    }
}

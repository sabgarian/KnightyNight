using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngagementManager : MonoBehaviour
{
    public Transform playerTrans;

    public int livingEnemies = 0;
    public GameObject[] Waves; // gameobject storing waves to set to active
    public float enemyEntryTime = 3f; // switch animation to here and make invulnerable when approaching

    public int MaximumEngagers = 2;

    public List<GameObject> enrolled = new List<GameObject>();
    public List<Vector2> EngagementTimeRange = new List<Vector2>();

    public List<int> waitlist = new List<int>();

    public List<int> engaged = new List<int>();
    public List<float> engagementTime = new List<float>();
    public List<float> maxEngagementTime = new List<float>();

    void Start()
    {
        playerTrans = GameObject.Find("FightPlayer").transform;
    }

    void Update()
    {
        for (int i = 0; i < engagementTime.Count; ++i)
        {
            engagementTime[i] += Time.deltaTime;
            if (engagementTime[i] >= maxEngagementTime[i])
            {
                Disengage(engaged[i]);
            }
        }

        if (engaged.Count < MaximumEngagers && waitlist.Count > 0)
            Engage(0);
    }

    public void AddEnemy(GameObject target, Vector2 engagementTimeRange)
    {
        if (enrolled.Contains(target))
        {
            Debug.LogError("Tried to add an enemy twice! Don't do that!");
            return;
        }
        enrolled.Add(target);
        EngagementTimeRange.Add(engagementTimeRange);
        waitlist.Add(enrolled.Count - 1);
    }

    public void RemoveEnemy(GameObject target)
    {
        for (int i = 0; i < enrolled.Count; ++i)
        {
            if (enrolled[i] == target)
            {
                //enrolled.RemoveAt(i);
                //EngagementTimeRange.RemoveAt(i);
                bool removedFromQueues = false;
                for (int z = 0; z < waitlist.Count; ++z)
                {
                    if (waitlist[z] == i)
                    {
                        waitlist.RemoveAt(z);
                        removedFromQueues = true;
                        break;
                    }
                }
                if (removedFromQueues)
                    break;
                for (int z = 0; z < engaged.Count; ++z)
                {
                    if (engaged[z] == i)
                    {
                        engaged.RemoveAt(z);
                        engagementTime.RemoveAt(z);
                        maxEngagementTime.RemoveAt(z);
                        removedFromQueues = true;
                        break;
                    }
                }
                if (!removedFromQueues)
                    Debug.LogError("Enemy (" + target.name + ") not found in any Engagement Queue!");
                return;
            }
        }
        Debug.LogError("Enemy (" + target.name + ") not found to remove from Enrollment!");
    }

    public void Engage(int waitlistInd)
    {
        int indToAdd = waitlist[waitlistInd];
        waitlist.RemoveAt(waitlistInd);

        engaged.Add(indToAdd);
        engagementTime.Add(0f);
        maxEngagementTime.Add(Random.Range(EngagementTimeRange[indToAdd].x, EngagementTimeRange[indToAdd].y));
        enrolled[indToAdd].SendMessage("Engage");
    }

    public void SkipWaitlist(GameObject target)
    {
        if (engaged.Count >= MaximumEngagers)
        {
            int closestToDoneInd = -1;
            float closestToDoneTime = Mathf.Infinity;

            for (int i = 0; i < engaged.Count; ++i)
            {
                float timeLeft = maxEngagementTime[i] - engagementTime[i];
                if (timeLeft < closestToDoneTime)
                {
                    closestToDoneInd = engaged[i];
                    closestToDoneTime = timeLeft;
                }
            }

            if (closestToDoneInd != -1)
                Disengage(closestToDoneInd);
        }

        for (int i = 0; i < waitlist.Count; ++i)
        {
            if (enrolled[waitlist[i]] == target)
            {
                Engage(i);
                return;
            }
        }
        Debug.LogError("Enemy (" + target.name + ") not found on waitlist for skip!");
    }

    public void AddMainEnemy(GameObject target, Vector2 engagementTimeRange)
    {
        AddEnemy(target, engagementTimeRange);
        Engage(waitlist[waitlist.Count - 1]);
    }

    public void Disengage(int targetInd)
    {
        for (int i = 0; i < engaged.Count; ++i)
        {
            if (engaged[i] == targetInd)
            {
                enrolled[engaged[i]].SendMessage("Disengage");
                waitlist.Add(engaged[i]);
                engaged.RemoveAt(i);
                engagementTime.RemoveAt(i);
                maxEngagementTime.RemoveAt(i);
                return;
            }
        }
        Debug.LogError("Enemy at index (" + targetInd + ") not found in any Engagement Queue!");
    }
}

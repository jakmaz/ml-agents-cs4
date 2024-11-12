using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AgentHearing : MonoBehaviour
{
    public UnityEvent Noise;

    void Start()
    {
        if (Noise == null)
            Noise = new UnityEvent();

        Noise.AddListener(CollisionNoise);
    }
    void CollisionNoise()
    {
        Debug.Log("tackle");
    }
}

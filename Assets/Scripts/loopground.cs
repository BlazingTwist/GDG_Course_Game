using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loopground : MonoBehaviour
{
    public float backgroundspeed;
    public Renderer bgren;

    // Update is called once per frame
    void Update()
    {
        bgren.material.mainTextureOffset += new Vector2(backgroundspeed * Time.deltaTime,0f);
    }
}

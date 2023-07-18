using System;
using TMPro;
using UnityEngine;
using World;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public GameObject axe;
    public GameObject envelope;
    public GameObject map;
    public GameObject rope;

    private bool axeCollected;
    private bool envelopeCollected = false;
    private bool mapCollected = false;
    private bool ropeCollected = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        UpdateItems();
    }

    public static ItemManager GetInstance()
    {
        return instance;
    }

    public void UpdateItems()
    {
        axe.SetActive(axeCollected);
        envelope.SetActive(envelopeCollected);
        map.SetActive(mapCollected);
        rope.SetActive(ropeCollected);
    }

    public void CollectAxe()
    {
        axeCollected = true;
        UpdateItems();
    }

    public void CollectEnvelope()
    {
        envelopeCollected = true;
        UpdateItems();
    }

    public void CollectMap()
    {
        mapCollected = true;
        UpdateItems();
    }

    public void CollectRope()
    {
        ropeCollected = true;
        UpdateItems();
    }


}


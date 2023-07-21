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

    private bool axeCollected = false;
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

    public void CollectAxe(bool val)
    {
        axeCollected = val;
        UpdateItems();
    }

    public void CollectEnvelope(bool val)
    {
        envelopeCollected = val;
        UpdateItems();
    }

    public void CollectMap(bool val)
    {
        mapCollected = val;
        UpdateItems();
    }

    public void CollectRope(bool val)
    {
        ropeCollected = val;
        UpdateItems();
    }

    public bool GetAxe()
    {
        return axeCollected;
    }

    public bool GetEnvelope()
    {
        return envelopeCollected;
    }

    public bool GetMap()
    {
        return mapCollected;
    }

    public bool GetRope()
    {
        return ropeCollected;
    }

	public void Destroy()
    {
        Destroy(this.gameObject);
    }
}


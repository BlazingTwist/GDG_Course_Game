using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneManager : MonoBehaviour
{
    protected ItemManager itemManager;
    public GameObject explanation;
    public GameObject beachAxeRope;
    public GameObject beachAxeRopeMap;
    public GameObject beachNoAxeRope;
    public GameObject beachNoAxeRopeMap;
    public GameObject mainlandMapEnvelope;
    public GameObject mainlandMapNoEnvelope;
    public GameObject mainlandNoMapEnvelope;
    public GameObject mainlandNoMapNoEnvelope;
    public GameObject endMap;
    public GameObject endNoMap;


    private void Awake()
    {
        Debug.Log("Awake");
        itemManager = ItemManager.GetInstance();
    }
    public void SwitchBeach()
    {
        Debug.Log("SwitchBeach");
        if (itemManager.GetAxe() == true && itemManager.GetRope() == true && itemManager.GetMap() == true)
        {
            beachAxeRopeMap.SetActive(true);
        }
        else if (itemManager.GetAxe() == true && itemManager.GetRope() == true && itemManager.GetMap() == false)
        {
            beachAxeRope.SetActive(true);
        }
        else if ((itemManager.GetAxe() == false && itemManager.GetRope() == true && itemManager.GetMap() == true) ||
            (itemManager.GetAxe() == true && itemManager.GetRope() == false && itemManager.GetMap() == true) ||
            (itemManager.GetAxe() == false && itemManager.GetRope() == false && itemManager.GetMap() == true))
        {
            beachNoAxeRopeMap.SetActive(true);
        }
        else
        {
            beachNoAxeRope.SetActive(true);
        }
    }

	public void FindAxeRope()
    {
        itemManager.CollectAxe(true);
        itemManager.CollectRope(true);
    }

	public void SwitchMainland()
    {
        Debug.Log("SwitchMainland");
		if(itemManager.GetMap() == true && itemManager.GetEnvelope() == true)
        {
            mainlandMapEnvelope.SetActive(true);
        }
		else if (itemManager.GetMap() == true && itemManager.GetEnvelope() == false)
        {
            mainlandMapNoEnvelope.SetActive(true);
        }
        else if (itemManager.GetMap() == false && itemManager.GetEnvelope() == true)
        {
            mainlandNoMapEnvelope.SetActive(true);
        }
        else
        {
            mainlandNoMapNoEnvelope.SetActive(true);
        }
    }

	public void SwitchEnd()
    {
        if (itemManager.GetMap() == true)
        {
            endMap.SetActive(true);
        }
        else
        {
            endNoMap.SetActive(true);
        }
    }

	public void LoadMainMenu()
    {
        SceneManager.LoadScene("Hauptmenue");
        itemManager.Destroy();
    }

    public void SwitchAxe()
    {
        if (itemManager.GetAxe() == true)
        {
            itemManager.CollectAxe(false);
        }
        else
        {
            itemManager.CollectAxe(true);
        }
    }

    public void SwitchEnvelope()
    {
        if (itemManager.GetEnvelope() == true)
        {
            itemManager.CollectEnvelope(false);
        }
        else
        {
            itemManager.CollectEnvelope(true);
        }
    }

    public void SwitchMap()
    {
        if (itemManager.GetMap() == true)
        {
            itemManager.CollectMap(false);
        }
        else
        {
            itemManager.CollectMap(true);
        }
    }

    public void SwitchRope()
    {
        if (itemManager.GetRope() == true)
        {
            itemManager.CollectRope(false);
        }
        else
        {
            itemManager.CollectRope(true);
        }
    }
}

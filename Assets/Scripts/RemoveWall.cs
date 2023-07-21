using System;
using UnityEngine;

public class RemoveWall : MonoBehaviour
{

    [SerializeField] private GameObject[] originKeyObjects;
    [SerializeField] private GameObject removedObject;
    private GameObject[] activeKeyObjects;
    void Start()
    {
        activeKeyObjects = originKeyObjects;
    }

    void Update()
    {
        if (!activeKeyObjects[activeKeyObjects.Length-1].activeSelf)
        {
            if (activeKeyObjects.Length == 1)
            {
                removedObject.SetActive(false);
            }
            else
            {
                int counter = 0;
				for(int i=0;i<activeKeyObjects.Length - 1;i++)
                {
                    if (activeKeyObjects[i].activeSelf)
                    {
                        counter++;
                    }
                }
                if (counter == activeKeyObjects.Length - 1)
                {
                    Array.Resize(ref activeKeyObjects, activeKeyObjects.Length - 1);
                }
                else
                {
                    activeKeyObjects = originKeyObjects;
                    for (int i = 0; i < activeKeyObjects.Length; i++)
                    {
                        activeKeyObjects[i].SetActive(true);
                    }
                }
            }
        }
    }
}


using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ItemManager : MonoBehaviour
    {
        public GameObject[] keyObjects;
        public GameObject wallObject;

        private List<GameObject> collectedItems = new List<GameObject>();
        private int expectedIndex = 0;

        private void Start()
        {
            ActivateExpectedKeyObject();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
                 RemoveWall();
            Debug.Log(other.CompareTag("Key"));
            if (other.CompareTag("Key"))
            
            {
                GameObject keyObject = other.gameObject;

                if (keyObject == keyObjects[expectedIndex])
                {
                    collectedItems.Add(keyObject);
                    expectedIndex++;

                    RemoveWall();

                    if (expectedIndex < keyObjects.Length)
                    {
                        ActivateExpectedKeyObject();
                    }
                }
                else
                {
                    ResetKeyObjects();
                }
            }
        }

        private void ActivateExpectedKeyObject()
        {
            keyObjects[expectedIndex].SetActive(true);
        }

        private void RemoveWall()
        {
            wallObject.SetActive(false);
        }

        private void ResetKeyObjects()
        {
            foreach (GameObject keyObject in keyObjects)
            {
                keyObject.SetActive(true);
            }

            collectedItems.Clear();
            expectedIndex = 0;
        }
    }
}

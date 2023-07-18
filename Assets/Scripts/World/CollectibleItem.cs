using UnityEngine;

namespace World
{

    public class CollectibleItem : Interactable
    {
        protected ItemManager itemManager;
        public string itemName;
        public override void OnInteractComplete()
        {
            base.OnInteractComplete();

            if (!gameObject.activeSelf)
            {
                return;
            }

            levelProgressManager.OnCollectibleGotten();
            gameObject.SetActive(false);

            itemManager = ItemManager.GetInstance();
            ActivateItemInUI(itemName);


        }

        public void ActivateItemInUI(string name)
        {
            if (name == "Axe")
            {
                itemManager.CollectAxe();
            }
            if (name == "Envelope")
            {
                itemManager.CollectEnvelope();
            }
            if (name == "Map")
            {
                itemManager.CollectMap();
            }
            if (name == "Rope")
            {
                itemManager.CollectRope();
            }

        }
    }

}

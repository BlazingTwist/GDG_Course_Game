namespace World
{

    public class CollectiblePuzzle : Interactable
    {
        public override void OnInteractComplete()
        {
            base.OnInteractComplete();

            if (!gameObject.activeSelf)
            {
                return;
            }
			
            gameObject.SetActive(false);

        }
    }

}
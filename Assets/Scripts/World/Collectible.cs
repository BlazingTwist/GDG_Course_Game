namespace World {

	public class Collectible : Interactable {
		public override void OnInteractComplete() {
			base.OnInteractComplete();
			
			if (!gameObject.activeSelf) {
				return;
			}

			levelProgressManager.OnCollectibleGotten();
			gameObject.SetActive(false);
		}
	}

}

namespace World {

	public class RaetselLvl1 : Interactable {
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

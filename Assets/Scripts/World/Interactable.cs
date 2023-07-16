using UnityEngine;

namespace World {

	public class Interactable : MonoBehaviour {
		[SerializeField] private Vector3 promptOffset = Vector3.up;
		[SerializeField] private string titleText;
		[SerializeField, TextArea(1, 10)] private string displayText;

		protected LevelProgressManager levelProgressManager;

		private void OnEnable() {
			levelProgressManager = GameManager.GetInstance().GetLevelProgressManager();
		}

		public Vector3 PromptOffset => promptOffset;

		public string TitleText => titleText;

		public string DisplayText => displayText;

		public virtual void OnInteractStart() {
			levelProgressManager.PauseTimer(true);
		}

		public virtual void OnInteractComplete() {
			levelProgressManager.PauseTimer(false);
		}

	}

}

using UnityEngine;

namespace World {

	public class Interactable : MonoBehaviour
	{
		[SerializeField] private Vector3 promptOffset = Vector3.up;
		[SerializeField] private string titleText;
		[SerializeField, TextArea(1, 10)] private string displayText;

		public Vector3 PromptOffset => promptOffset;

		public string TitleText => titleText;
		
		public string DisplayText => displayText;

	}

}

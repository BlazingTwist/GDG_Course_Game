using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {

	public class RebindHandler : MonoBehaviour {
		[SerializeField] private TMP_Text bindingNameText;
		[SerializeField] private TMP_Text bindingKeyText;

		/// <summary>
		/// The GameObject to show by default
		/// </summary>
		[SerializeField] private GameObject showBindingObject;
	
		/// <summary>
		/// The GameObject to show while rebinding is occuring
		/// </summary>
		[SerializeField] private GameObject awaitBindingObject;

		private EFSInputManager inputManager;
		private InputHandler inputHandler;
		private int bindingIndex;

		private void Start() {
			GameManager gameManager = GameManager.GetInstance();
			inputManager = gameManager.GetInputManager();
		}

		[PublicAPI]
		public void SetRebindTarget(InputHandler handler, int index, string bindName) {
			inputHandler = handler;
			bindingIndex = index;

			bindingNameText.text = bindName;
			bindingKeyText.text = inputHandler.inputAction.GetBindingDisplayString(bindingIndex);
		}

		[PublicAPI]
		public void StartRebinding() {
			showBindingObject.SetActive(false);
			awaitBindingObject.SetActive(true);
			
			inputManager.StartRebinding(inputHandler, bindingIndex, RebindComplete);
		}

		private void RebindComplete() {
			awaitBindingObject.SetActive(false);
			showBindingObject.SetActive(true);
			bindingKeyText.text = inputHandler.inputAction.GetBindingDisplayString(bindingIndex);
		}
	}

}

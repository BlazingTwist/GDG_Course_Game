using Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ui {

	public class InteractPrompt : MonoBehaviour {
		[SerializeField] private TMP_Text interactButtonLabel;
		private EFSInputManager inputManager;
		private InputHandler interactHandler;

		private void OnEnable() {
			inputManager = GameManager.GetInstance().GetInputManager();
			interactHandler = inputManager.GetButton(EGameplay_Button.Interact);
			inputManager.RebindEvent += OnRebindKey;
		}

		private void OnDisable() {
			inputManager.RebindEvent -= OnRebindKey;
		}

		private void OnRebindKey(InputHandler input, int bindingIndex) {
			if (input == interactHandler) {
				interactButtonLabel.text = input.inputAction.GetBindingDisplayString(bindingIndex);
			}
		}
	}

}

using Input;
using UnityEngine;

namespace Ui {

	/// <summary>
	/// Attach this component to the PauseMenu UI
	/// </summary>
	public class PauseMenuController : MonoBehaviour {

		private UiManager uiManager;
		private EFSInputManager inputManager;

		private void Awake() {
			GameManager gameManager = GameManager.GetInstance();
			uiManager = gameManager.GetUiController();
			inputManager = gameManager.GetInputManager();
		}

		private void Update() {
			if (inputManager.GetButton(EMenu_Button.Back).IsTriggered(0)) {
				uiManager.ClosePauseMenu();
			}
		}
	}

}

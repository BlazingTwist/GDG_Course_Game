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
			GameController gameController = GameController.GetInstance();
			uiManager = gameController.GetUiController();
			inputManager = gameController.GetInputManager();
		}

		private void Update() {
			if (inputManager.GetButton(EMenu_Button.Back).IsTriggered(0)) {
				uiManager.ClosePauseMenu();
			}
		}
	}

}

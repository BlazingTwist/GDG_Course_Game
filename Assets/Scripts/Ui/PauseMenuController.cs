using Input;
using UnityEngine;

namespace Ui {

	/// <summary>
	/// Attach this component to the PauseMenu UI
	/// </summary>
	public class PauseMenuController : MonoBehaviour {

		private UiController uiController;
		private EFSInputManager inputManager;

		private void Awake() {
			GameController gameController = GameController.GetInstance();
			uiController = gameController.GetUiController();
			inputManager = gameController.GetInputManager();
		}

		private void Update() {
			if (inputManager.GetButton(EMenu_Button.Back).IsTriggered(0)) {
				uiController.ClosePauseMenu();
			}
		}
	}

}

using Input;
using UnityEngine;

namespace Ui {

	/// <summary>
	/// Singleton System for Controlling UI
	/// </summary>
	public class UiManager : MonoBehaviour {
		[SerializeField] private GameObject pauseMenu;

		private EFSInputManager inputManager;

		private void Start() {
			GameManager gameManager = GameManager.GetInstance();
			inputManager = gameManager.GetInputManager();
		}

		public void OpenPauseMenu() {
			if (pauseMenu.activeInHierarchy) {
				return;
			}

			Time.timeScale = 0;
			pauseMenu.SetActive(true);
			inputManager.SetActiveActions(inputManager.MenuActions);
		}

		public void ClosePauseMenu() {
			if (!pauseMenu.activeInHierarchy) {
				return;
			}

			Time.timeScale = 1;
			pauseMenu.SetActive(false);
			inputManager.SetActiveActions(inputManager.GameplayActions);
		}
	}

}

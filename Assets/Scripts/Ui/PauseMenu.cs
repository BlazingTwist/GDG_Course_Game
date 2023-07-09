using System;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ui {

	public class PauseMenu : MonoBehaviour {
		[SerializeField] private GameObject pauseMenuCanvas;
		[SerializeField] private GameObject pauseMenuUI;
		[SerializeField] private GameObject remapUI;

		private GameManager gameManager;
		private EFSInputManager inputManager;
		private State currentState;

		private void OnEnable() {
			gameManager = GameManager.GetInstance();
			inputManager = gameManager.GetInputManager();
			
			gameManager.PauseEvent += OnPauseChanged;
			inputManager.GetButton(EGameplay_Button.Pause).AddTriggerCallback(OnPauseKeyEvent);
			inputManager.GetButton(EMenu_Button.Back).AddTriggerCallback(OnNavBackKeyEvent);
		}

		private void OnDisable() {
			gameManager.PauseEvent -= OnPauseChanged;
			inputManager.GetButton(EGameplay_Button.Pause).RemoveTriggerCallback(OnPauseKeyEvent);
			inputManager.GetButton(EMenu_Button.Back).RemoveTriggerCallback(OnNavBackKeyEvent);
		}

		private void Start() {
			// set at startup, so the elements can be enabled in the prefab for easy editing
			pauseMenuCanvas.SetActive(false);
			pauseMenuUI.SetActive(false);
			remapUI.SetActive(false);
		}

		private void OnPauseChanged(bool isPaused) {
			pauseMenuUI.SetActive(isPaused);
			if (isPaused) {
				inputManager.SetActiveActions(inputManager.MenuActions);
				HandleStateChange(State.PauseMenu);
			} else {
				inputManager.SetActiveActions(inputManager.GameplayActions);
				HandleStateChange(State.Closed);
			}
		}

		private void OnPauseKeyEvent(InputAction.CallbackContext context) {
			gameManager.Pause();
		}

		private void OnNavBackKeyEvent(InputAction.CallbackContext context) {
			switch (currentState) {
				case State.PauseMenu:
					Resume();
					break;
				case State.RebindMenu:
					CloseRemapUI();
					break;
				case State.Closed:
					// ignore
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleStateChange(State state) {
			switch (currentState) {
				case State.Closed:
					pauseMenuCanvas.SetActive(true);
					break;
				case State.PauseMenu:
					pauseMenuUI.SetActive(false);
					break;
				case State.RebindMenu:
					remapUI.SetActive(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			switch (state) {
				case State.Closed:
					pauseMenuCanvas.SetActive(false);
					break;
				case State.PauseMenu:
					pauseMenuUI.SetActive(true);
					break;
				case State.RebindMenu:
					remapUI.SetActive(true);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
			
			currentState = state;
		}

		public void Resume() {
			gameManager.UnPause();
		}

		public void LoadStartMenu() {
			gameManager.UnPause();
			SceneManager.LoadScene("Hauptmenue");
		}

		public void OpenRemapUI() {
			pauseMenuUI.SetActive(false);
			remapUI.SetActive(true);
			currentState = State.RebindMenu;
		}

		public void CloseRemapUI() {
			HandleStateChange(State.PauseMenu);
		}

		private enum State {
			Closed,
			PauseMenu,
			RebindMenu,
		}

	}

}

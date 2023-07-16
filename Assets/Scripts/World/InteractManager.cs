using System;
using Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace World {

	public class InteractManager : MonoBehaviour {
		[SerializeField] private float maxInteractDistance = 4f;
		[SerializeField] private GameObject player;
		[SerializeField] private GameObject interactPrompt;

		[SerializeField] private GameObject textPopupObject;
		[SerializeField] private TMP_Text titleText;
		[SerializeField] private TMP_Text contentText;

		private GameManager gameManager;
		private EFSInputManager inputManager;
		private Interactable[] interactables;
		private Interactable interactTarget;
		private bool promptEnabled;
		private bool popUpActive;

		private void Awake() {
			interactables = Resources.FindObjectsOfTypeAll<Interactable>();
		}

		private void Start() {
			gameManager = GameManager.GetInstance();
			inputManager = gameManager.GetInputManager();
			inputManager.GetButton(EGameplay_Button.Interact).AddTriggerCallback(OnInteractPressed);

			// ensure state is synced
			interactPrompt.transform.parent.gameObject.SetActive(true);
			interactPrompt.SetActive(promptEnabled);

			textPopupObject.transform.parent.gameObject.SetActive(true);
			textPopupObject.SetActive(false);
		}

		private void OnInteractPressed(InputAction.CallbackContext context) {
			if (interactTarget == null) {
				return;
			}

			popUpActive = true;
			SetPromptEnabled(false);
			titleText.text = interactTarget.TitleText;
			contentText.text = interactTarget.DisplayText;
			textPopupObject.SetActive(true);
			inputManager.SetActiveActions(inputManager.MenuActions);
		}

		public void OnTextPopupClosed() {
			popUpActive = false;
			textPopupObject.SetActive(false);
			inputManager.SetActiveActions(inputManager.GameplayActions);
		}

		private void LateUpdate() {
			if (popUpActive) {
				return;
			}

			Vector3 playerPosition = player.transform.position;
			Interactable nearestInteractable = null;
			float minDistanceSquared = float.PositiveInfinity;
			foreach (Interactable interactable in interactables) {
				float distanceSquared = (interactable.transform.position - playerPosition).sqrMagnitude;
				if (distanceSquared < minDistanceSquared) {
					minDistanceSquared = distanceSquared;
					nearestInteractable = interactable;
				}
			}

			float maxInteractDistSqr = maxInteractDistance * maxInteractDistance;
			if (nearestInteractable != null && minDistanceSquared <= maxInteractDistSqr) {
				SetPromptEnabled(true);
				interactTarget = nearestInteractable;

				Vector3 interactablePosition = nearestInteractable.transform.position;
				Vector3 promptOffset = nearestInteractable.PromptOffset;
				Vector3 towardsPlayerOffset = (playerPosition - (interactablePosition + promptOffset)).normalized;
				float distanceFraction = minDistanceSquared / maxInteractDistSqr;
				interactPrompt.transform.position = interactablePosition + promptOffset + (towardsPlayerOffset * distanceFraction);
			} else {
				SetPromptEnabled(false);
				interactTarget = null;
			}
		}

		private void SetPromptEnabled(bool enable) {
			if (promptEnabled != enable) {
				promptEnabled = enable;
				interactPrompt.SetActive(enable);
			}
		}
	}

}

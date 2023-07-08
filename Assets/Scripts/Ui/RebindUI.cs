using System;
using Input;
using UnityEngine;

namespace Ui {

	public class RebindUI : MonoBehaviour {
		[SerializeField] private GameObject targetContainer;
		[SerializeField] private GameObject singleKeyRebindPrefab;

		private static readonly RebindKeyInfo[] rebindKeys = {
				new(EGameplay_Axis.MoveHorizontal, null, "MoveLeft", "MoveRight"),
				new(EGameplay_Button.Jump, "Jump"),
				new(EGameplay_Button.Interact, "Interact"),
				new(EGameplay_Button.Pause, "Pause"),
		};

		private void Start() {
			GameManager gameManager = GameManager.GetInstance();
			EFSInputManager inputManager = gameManager.GetInputManager();

			Transform containerTransform = targetContainer.transform;

			foreach (RebindKeyInfo key in rebindKeys) {
				InputHandler handler = inputManager.FindInputHandler(key.InputManagerKey);
				for (int i = 0; i < key.bindingNames.Length; i++) {
					string bindingName = key.bindingNames[i];
					if (bindingName == null) {
						continue;
					}

					GameObject rebindGO = Instantiate(singleKeyRebindPrefab, containerTransform, false);
					RebindHandler rebindHandler = rebindGO.GetComponent<RebindHandler>();
					rebindHandler.SetRebindTarget(handler, i, bindingName);
				}
			}
		}

		private readonly struct RebindKeyInfo {
			public readonly Enum InputManagerKey;
			public readonly string[] bindingNames;

			public RebindKeyInfo(Enum inputManagerKey, params string[] bindingNames) {
				InputManagerKey = inputManagerKey;
				this.bindingNames = bindingNames;
			}
		}

	}

}

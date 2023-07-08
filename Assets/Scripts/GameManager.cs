using Input;
using Ui;
using UnityEngine;

/// <summary>
/// Contains a reference to all of the singleton Manager/Controller components
/// </summary>
public class GameManager : MonoBehaviour {
	private static GameManager instance;

	[SerializeField] private EFSInputManager inputManager;
	[SerializeField] private UiManager uiManager;

	private void Awake() {
		instance = this;
	}

	public static GameManager GetInstance() {
		return instance;
	}

	public EFSInputManager GetInputManager() {
		return inputManager;
	}

	public UiManager GetUiController() {
		return uiManager;
	}
}

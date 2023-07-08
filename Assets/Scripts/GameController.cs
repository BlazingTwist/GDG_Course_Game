using Input;
using Ui;
using UnityEngine;

/// <summary>
/// Contains a reference to all of the singleton Manager/Controller components
/// </summary>
public class GameController : MonoBehaviour {
	private static GameController instance;

	[SerializeField] private EFSInputManager inputManager;
	[SerializeField] private UiManager uiManager;

	private void Awake() {
		instance = this;
	}

	public static GameController GetInstance() {
		return instance;
	}

	public EFSInputManager GetInputManager() {
		return inputManager;
	}

	public UiManager GetUiController() {
		return uiManager;
	}
}
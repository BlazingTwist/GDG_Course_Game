using Input;
using UnityEngine;
using World;

/// <summary>
/// Contains a reference to all of the singleton Manager/Controller components
/// </summary>
public class GameManager : MonoBehaviour {
	private static GameManager instance;

	[SerializeField] private EFSInputManager inputManager;
	[SerializeField] private InteractManager interactManager;
	[SerializeField] private float pauseCooldown = 0.5f; // prevents pause buffering

	public event OnPauseChanged PauseEvent;

	private bool gameIsPaused;
	private float pauseCooldownLeft;
	private bool announcedPauseState;

	private void Awake() {
		instance = this;
	}

	private void Update() {
		if (pauseCooldownLeft > 0) {
			pauseCooldownLeft -= Time.deltaTime;
		}
		if (announcedPauseState != gameIsPaused) {
			announcedPauseState = gameIsPaused;
			PauseEvent?.Invoke(gameIsPaused);
		}
	}

	public static GameManager GetInstance() {
		return instance;
	}

	public EFSInputManager GetInputManager() {
		return inputManager;
	}

	public InteractManager GetInteractManager() {
		return interactManager;
	}

	public void Pause() {
		if (gameIsPaused) {
			return;
		}
		if (pauseCooldownLeft > 0) {
			Debug.Log("You have to wait at least " + pauseCooldownLeft + " seconds to pause again.");
			return;
		}
		pauseCooldownLeft = pauseCooldown;
		gameIsPaused = true;
		Time.timeScale = 0f;
	}

	public void UnPause() {
		if (!gameIsPaused) {
			return;
		}
		gameIsPaused = false;
		Time.timeScale = 1f;
	}

	public delegate void OnPauseChanged(bool isPaused);
}

using System;
using TMPro;
using UnityEngine;
using World;

public class LevelProgressManager : MonoBehaviour {

	[SerializeField] private GameObject levelCompleteCanvas;

	[SerializeField] private GameObject completionistCheckmark;
	[SerializeField] private TMP_Text completionistDisplayText;

	[SerializeField] private GameObject speedrunnerCheckmark;
	[SerializeField] private TMP_Text speedrunnerDisplayText;
	[SerializeField] private float speedrunThresholdSeconds;

	private int totalLevelCollectibles;
	private int obtainedCollectibles;
	private bool timerPaused;
	private float timeSpent;

	private void OnEnable() {
		levelCompleteCanvas.SetActive(false);
		totalLevelCollectibles = FindObjectsOfType<Collectible>().Length;
		obtainedCollectibles = 0;
		timerPaused = false;
		timeSpent = 0f;
	}

	private void FixedUpdate() {
		if (!timerPaused) {
			timeSpent += Time.fixedDeltaTime;
		}
	}

	public void PauseTimer(bool paused) {
		timerPaused = paused;
	}

	public void OnCollectibleGotten() {
		obtainedCollectibles++;
	}

	public void ShowLevelComplete() {
		completionistCheckmark.SetActive(obtainedCollectibles >= totalLevelCollectibles);
		string totalCollectiblesText = "" + totalLevelCollectibles;
		completionistDisplayText.text = PadNumber(obtainedCollectibles, totalCollectiblesText.Length) + " / " + totalCollectiblesText;
		
		speedrunnerCheckmark.SetActive(timeSpent <= speedrunThresholdSeconds);
		speedrunnerDisplayText.text = SecondsToString(timeSpent) + " / " + SecondsToString(speedrunThresholdSeconds);
		
		levelCompleteCanvas.SetActive(true);
	}

	private static string SecondsToString(float seconds) {
		string displayMinutes = PadNumber((int) Math.Floor(seconds / 60f), 2);
		string displaySeconds = PadNumber((int) Math.Floor(seconds % 60), 2);
		string displayMillis = PadNumber((int) Math.Floor((seconds * 1000f) % 1000), 3);
		return $"{displayMinutes}:{displaySeconds}.{displayMillis}";
	}

	private static string PadNumber(int number, int length) {
		string strValue = "" + number;
		return strValue.PadLeft(length, '0');
	}

}

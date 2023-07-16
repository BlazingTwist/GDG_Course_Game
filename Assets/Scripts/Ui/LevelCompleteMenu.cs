using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui {

	public class LevelCompleteMenu : MonoBehaviour {

		[SerializeField] private GameObject LevelCompleteText;
		[SerializeField] private GameObject GameCompleteText;
		[SerializeField] private GameObject NextLevelButton;

		private GameManager gameManager;

		private void OnEnable() {
			gameManager = GameManager.GetInstance();
			int currentLevel = SceneManager.GetActiveScene().buildIndex;
			bool hasNextLevel = (currentLevel + 1) < SceneManager.sceneCountInBuildSettings;
			LevelCompleteText.SetActive(hasNextLevel);
			GameCompleteText.SetActive(!hasNextLevel);
			NextLevelButton.SetActive(hasNextLevel);
		}

		public void LoadStartMenu() {
			gameManager.UnPause();
			SceneManager.LoadScene("Hauptmenue");
		}

		public void ResetLevel() {
			gameManager.UnPause();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		public void NextLevel() {
			gameManager.UnPause();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}

	}

}

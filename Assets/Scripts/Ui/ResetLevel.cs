using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui {

	public class ResetLevel : MonoBehaviour {
		public void Execute() {
			GameManager.GetInstance().UnPause();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

}

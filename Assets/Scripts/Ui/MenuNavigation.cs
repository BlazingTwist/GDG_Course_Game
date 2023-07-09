using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui {

	public class MenuNavigation : MonoBehaviour
	{
    
		public void PlayButton(){
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}

		public void QuitButton(){
			Debug.Log("Info: quit-button pressed");
			Application.Quit();
		}
	}

}

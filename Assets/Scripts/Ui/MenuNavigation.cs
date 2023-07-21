using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui {

	public class MenuNavigation : MonoBehaviour
	{
        protected ItemManager itemManager;

        public void PlayButton(){
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            itemManager = ItemManager.GetInstance();
            itemManager.CollectAxe(false);
            itemManager.CollectEnvelope(false);
            itemManager.CollectMap(false);
            itemManager.CollectRope(false);
		}

		public void QuitButton(){
			Debug.Log("Info: quit-button pressed");
			Application.Quit();
		}
	}

}

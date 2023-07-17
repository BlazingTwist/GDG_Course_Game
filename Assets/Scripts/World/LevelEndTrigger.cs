using UnityEngine;
using UnityEngine.SceneManagement;

namespace World
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class LevelEndTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                LoadNextLevel();
            }
        }

        private void LoadNextLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

             Debug.Log("aktuell " + currentSceneIndex + " nächste: " + nextSceneIndex);
            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more levels to load. Game complete!");
                GameManager.GetInstance().GetLevelProgressManager().ShowLevelComplete();
            }
        }
    }
}

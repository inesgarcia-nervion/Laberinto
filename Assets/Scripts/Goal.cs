using UnityEngine;

public class Goal : MonoBehaviour
{
    private bool playerInRange = false;

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void LoadNextScene()
    {
      // Aquí puedes agregar efectos visuales o de sonido antes de cambiar de escena
      // Cargar la siguiente escena en el orden de construcción
      int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
      UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
    }
}

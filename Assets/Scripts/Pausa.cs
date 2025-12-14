using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausa : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelPausa;

    // Cuando el jugador presiona la tecla de esc, se activa o desactiva el estado de pausa del juego.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
            {
                panelPausa.SetActive(true);
                Time.timeScale = 0f; // Pausa el juego
            }
            else
            {
                panelPausa.SetActive(false);
                Time.timeScale = 1f; // Reanuda el juego
            }
        }
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menú");
    }

}

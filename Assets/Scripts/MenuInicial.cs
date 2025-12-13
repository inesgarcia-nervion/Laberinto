using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MenuInicial : MonoBehaviour
{
    [Header("UI - Paneles")]
    public GameObject panelMenuPrincipal; 
    public GameObject panelTiempos;      

    [Header("UI - Texto")]
    public TextMeshProUGUI textoParaMostrarRecords; 

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetTimer();      // Pone el tiempo a 0
            GameManager.Instance.SetPlayerLives(2); // Reinicia las vidas a 3 (o las que quieras)
                                                    // Y aquí destruyes el GM si quieres uno fresco, o simplemente cargas la escena 1
        }

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelTiempos != null) panelTiempos.SetActive(false);
    }

    public void Jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Salir()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void AbrirMenuTiempos()
    {
        panelMenuPrincipal.SetActive(false); // Oculta menú principal
        panelTiempos.SetActive(true);        // Muestra menú tiempos

        MostrarTiemposLogic(); 
    }

    public void VolverAlMenu()
    {
        panelTiempos.SetActive(false);       // Oculta menú tiempos
        panelMenuPrincipal.SetActive(true);  // Muestra menú principal
    }

    // Lógica interna para escribir los tiempos
    void MostrarTiemposLogic()
    {
        string data = PlayerPrefs.GetString("TablaTiempos", "");

        if (string.IsNullOrEmpty(data))
        {
            if (textoParaMostrarRecords != null) textoParaMostrarRecords.text = "Aún no hay tiempos registrados.";
            return;
        }

        string[] arrayTiempos = data.Split(',');
        List<float> listaTiempos = new List<float>();

        foreach (string t in arrayTiempos)
        {
            if (float.TryParse(t, out float valor))
            {
                listaTiempos.Add(valor);
            }
        }

        listaTiempos.Sort(); // Ordenar menor a mayor

        string textoFinal = "";
        int cantidad = Mathf.Min(listaTiempos.Count, 5); // Máximo 5 récords

        for (int i = 0; i < cantidad; i++)
        {
            textoFinal += (i + 1) + ". " + FormatearTiempo(listaTiempos[i]) + "\n";
        }

        if (textoParaMostrarRecords != null)
        {
            textoParaMostrarRecords.text = textoFinal;
        }
    }

    string FormatearTiempo(float tiempo)
    {
        tiempo += 1;
        float min = Mathf.FloorToInt(tiempo / 60);
        float seg = Mathf.FloorToInt(tiempo % 60);
        return string.Format("{0:00}:{1:00}", min, seg);
    }
}
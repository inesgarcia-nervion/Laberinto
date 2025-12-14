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
        // Reinicia datos del juego si hay una instancia previa del GM
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EstablecerVidasJugador(2);
        }

        if (SonidoManager.Instance != null)
        {
            SonidoManager.Instance.DetenerSirena();
        }

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelTiempos != null) panelTiempos.SetActive(false);
    }

    public void Jugar()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReiniciarTemporizador();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void AbrirMenuTiempos()
    {
        panelMenuPrincipal.SetActive(false);
        panelTiempos.SetActive(true);

        LogicaMostrarTiempos();
    }

    public void VolverAlMenu()
    {
        panelTiempos.SetActive(false);
        panelMenuPrincipal.SetActive(true);
    }

    // Lee y formatea la lista de récords guardada
    void LogicaMostrarTiempos()
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

        listaTiempos.Sort(); // Ordena los tiempos de menor a mayor

        string textoFinal = "";
        int cantidad = Mathf.Min(listaTiempos.Count, 5); // Muestra solo los top 5

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
        tiempo += 1; // Ajuste visual opcional
        float min = Mathf.FloorToInt(tiempo / 60);
        float seg = Mathf.FloorToInt(tiempo % 60);
        return string.Format("{0:00}:{1:00}", min, seg);
    }
}
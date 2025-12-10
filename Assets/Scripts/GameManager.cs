using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI y Puntuación")]
    public TMP_Text textoTiempo; 
    private float tiempoInicio;

    [Header("Sistema de Fases")]
    public GameObject[] fases; 
    private int faseActual = 0;

    void Start()
    {
        tiempoInicio = Time.time;
        // Oculta todas las fases excepto la primera
        for (int i = 1; i < fases.Length; i++)
        {
            fases[i].SetActive(false);
        }
    }

    void Update()
    {
        // Cronómetro
        float tiempoTranscurrido = Time.time - tiempoInicio;
        string t = string.Format("{0:0}:{1:00}", Mathf.Floor(tiempoTranscurrido / 60), Mathf.Floor(tiempoTranscurrido % 60));
        textoTiempo.text = "Tiempo: " + t;
    }

    // Llamado por el Player al recoger una lámpara
    public void PasarDeFase()
    {
        // Desactiva la fase actual
        fases[faseActual].SetActive(false);
        faseActual++;

        if (faseActual < fases.Length)
        {
            // Activa la siguiente fase
            fases[faseActual].SetActive(true);
        }
        else
        {
            // Fin del juego y guardar Record
            Debug.Log("Juego Completado en: " + (Time.time - tiempoInicio));
        }
    }
}
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class MostrarRecord : MonoBehaviour
{
    public TextMeshProUGUI textoRecord; // UI para el mejor tiempo

    void Start()
    {
        MostrarMejorTiempo();
    }

    void MostrarMejorTiempo()
    {
        if (textoRecord == null)
        {
            return;
        }

        // Recupera la lista de tiempos guardados
        string tiemposString = PlayerPrefs.GetString("TablaTiempos", "");

        if (string.IsNullOrEmpty(tiemposString))
        {
            textoRecord.text = "Récord: --:--";
            return;
        }

        // Convierte el string guardado en una lista numérica
        string[] arrayTiempos = tiemposString.Split(',');
        List<float> listaTiempos = new List<float>();

        foreach (string t in arrayTiempos)
        {
            if (float.TryParse(t, out float tiempoNumerico))
            {
                listaTiempos.Add(tiempoNumerico);
            }
        }

        if (listaTiempos.Count == 0)
        {
            textoRecord.text = "Récord: --:--";
            return;
        }

        // Encuentra el tiempo mínimo (récord)
        float mejorTiempo = Mathf.Min(listaTiempos.ToArray());

        // Formatea y muestra el resultado
        float minutos = Mathf.FloorToInt(mejorTiempo / 60);
        float segundos = Mathf.FloorToInt(mejorTiempo % 60);

        textoRecord.text = string.Format("Récord: {0:00}:{1:00}", minutos, segundos);
    }

}
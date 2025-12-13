using UnityEngine;
using TMPro;
using System.Linq; // Necesario para ordenar la lista
using System.Collections.Generic;

public class MostrarRecord : MonoBehaviour
{
    public TextMeshProUGUI textoRecord;

    void Start()
    {
        MostrarMejorTiempo();
    }

    void MostrarMejorTiempo()
    {
        // 1. Recuperamos la cadena guardada (ej: "120.5,300,45.2")
        string tiemposString = PlayerPrefs.GetString("TablaTiempos", "");

        if (string.IsNullOrEmpty(tiemposString))
        {
            textoRecord.text = "Récord: --:--";
            return;
        }

        // 2. Convertimos el string a una lista de números
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

        // 3. Buscamos el menor tiempo (el mejor)
        float mejorTiempo = Mathf.Min(listaTiempos.ToArray());

        // 4. Formateamos y mostramos
        float minutos = Mathf.FloorToInt(mejorTiempo / 60);
        float segundos = Mathf.FloorToInt(mejorTiempo % 60);

        textoRecord.text = string.Format("Récord: {0:00}:{1:00}", minutos, segundos);
    }

}
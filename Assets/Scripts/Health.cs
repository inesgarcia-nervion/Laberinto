using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public KeyCode teclaDeActivacion = KeyCode.E; // Tecla para recoger
    public float duracionMensaje = 2.0f; // Duración del mensaje en pantalla

    private TextMeshProUGUI mensajeAlertaHud;

    private bool jugadorEstaCerca = false; // Flag de proximidad
    private Player jugadorActual;
    private Collider2D objetoCollider;

    void Start()
    {
        objetoCollider = GetComponent<Collider2D>();

        // Busca el objeto HUD para alertas
        GameObject alertaObject = GameObject.FindWithTag("GameAlert");

        if (alertaObject != null)
        {
            mensajeAlertaHud = alertaObject.GetComponent<TextMeshProUGUI>();
        }

    }


    void Update()
    {
        if (jugadorEstaCerca && Input.GetKeyDown(teclaDeActivacion))
        {
            IntentarRecolectar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        jugadorActual = other.GetComponent<Player>();

        if (jugadorActual != null)
        {
            jugadorEstaCerca = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            jugadorEstaCerca = false;
            jugadorActual = null;
        }
    }

    void IntentarRecolectar()
    {
        if (jugadorActual == null) return;

        bool vidaGanada = jugadorActual.GanarVida();

        // Si se pudo ganar vida, destruye el objeto, si no, avisa que está lleno
        if (vidaGanada)
        {
            if (objetoCollider != null) objetoCollider.enabled = false;
            Destroy(gameObject);
        }
        else
        {
            MostrarMensaje("¡Vidas Completas!", duracionMensaje);
        }
    }

    private void MostrarMensaje(string mensaje, float duracion)
    {
        if (mensajeAlertaHud != null)
        {
            mensajeAlertaHud.text = mensaje;
            StopAllCoroutines();
            StartCoroutine(LimpiarMensajeDespuesDe(duracion));
        }
    }

    System.Collections.IEnumerator LimpiarMensajeDespuesDe(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (mensajeAlertaHud != null)
        {
            mensajeAlertaHud.text = "";
        }
    }
}
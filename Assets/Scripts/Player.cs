using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Para ir al menú

public class Player : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Transform conoDeLuz;
    public float velocidadRotacion = 15f;
    public float offsetAngulo = -90f;

    [Header("Movimiento y Vidas")]
    public float speed = 5.0f;
    private Rigidbody2D rb;
    private Vector2 movimiento;

    public float salud;
    public bool dead = false;
    public TextMeshProUGUI vidasHud;

    private Vector3 puntoDeInicio;
    private GameManager gameManager;

    private Timer timerScript;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            salud = 2; // Vida inicial

            puntoDeInicio = transform.position;
            gameManager = FindObjectOfType<GameManager>();

            // Buscamos el TimerManager en la escena
            timerScript = FindObjectOfType<Timer>();

            ActualizaHud();
        }
    }

    void Update()
    {
        // MOVIMIENTO (Horizontal y Vertical)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(moveX, moveY).normalized;

        // ROTACIÓN DEL CONO DE LUZ (Apuntar en dirección de movimiento)
        if (movimiento != Vector2.zero)
        {
            // Calcula el ángulo de dirección
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            angulo += offsetAngulo;

            // Suaviza la rotación del cono de luz
            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
        }
    }

    void FixedUpdate()
    {
        // Aplicar movimiento usando Rigidbody2D.MovePosition para el control físico (2D)
        if (rb != null)
        {
            rb.MovePosition(rb.position + movimiento * speed * Time.fixedDeltaTime);
        }
    }

    public void Hit()
    {
        if (dead) return;

        salud -= 1;
        ActualizaHud();

        if (salud <= 0)
        {
            salud = 0;
            dead = true;

            // Al morir NO guardamos el tiempo (porque no terminó el nivel).
            // Simplemente cargamos el menú (Escena 0)
            SceneManager.LoadScene(0);
        }
        else
        {
            // Respawn al punto de inicio 
            transform.position = puntoDeInicio;
        }
    }


    // Intenta sanar al jugador si no está a vida máxima.
    public bool GanarVida()
    {
        float maximoDeVidas = 3f;

        if (salud < maximoDeVidas)
        {
            salud += 1;
            ActualizaHud();
            return true;
        }

        return false;
    }


    // Maneja la recolección de un objeto clave (Lámpara/Progreso).
    public void RecolectarLampara()
    {
        // Paramos el timer y guardamos récord antes de cambiar de fase
        if (timerScript != null)
        {
            timerScript.FinalizarNivel();
        }

        if (gameManager != null)
        {
            gameManager.PasarDeFase();
        }
    }


    // Actualiza el texto de vidas en la interfaz.
    private void ActualizaHud()
    {
        if (vidasHud != null)
        {
            vidasHud.text = "Vidas: " + Mathf.FloorToInt(salud).ToString();
        }
    }

}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Transform conoDeLuz; // Objeto visual de la linterna
    public float velocidadRotacion = 15f; // Velocidad de giro de la luz
    public float offsetAngulo = -90f; // Ajuste del ángulo de la luz

    [Header("Configuración de Posición de Luz")]
    public float offsetDistancia = 0.5f; // Distancia de la luz respecto al jugador

    [Header("Movimiento y Vidas")]
    public float speed = 5.0f; // Velocidad de movimiento
    private Rigidbody2D rb;
    private Vector2 movimiento; // Vector de dirección actual

    // Referencia al componente de animaciones
    private Animator animator;

    // Escala inicial para gestionar el volteo del sprite
    private float initialScaleX;
    private float initialScaleY;

    public float salud; // Vidas actuales
    public bool dead = false; // Estado de muerte
    public TextMeshProUGUI vidasHud; // Texto UI de vidas

    private Vector3 puntoDeInicio; // Posición de respawn
    private GameManager gameManager;

    private Timer timerScript;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Guarda la escala original
        initialScaleX = transform.localScale.x;
        initialScaleY = transform.localScale.y;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            puntoDeInicio = transform.position;

            // Sincroniza vidas con el GameManager
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;
                salud = gameManager.playerLives;
            }
            else
            {
                // Busca el GM usando el método nuevo
                gameManager = Object.FindFirstObjectByType<GameManager>();
                salud = 2;
            }

            // Busca el Timer usando el método nuevo
            timerScript = Object.FindFirstObjectByType<Timer>();

            ActualizaHud();
        }

        if (animator == null)
        {
            Debug.LogError("El componente Animator es requerido para las animaciones y no se encontró.");
        }
    }

    void Update()
    {
        // No procesa input si el juego está congelado
        if (Time.timeScale == 0f) return;

        // Captura input de movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(moveX, moveY).normalized;

        // Gestión de animaciones de movimiento
        if (animator != null)
        {
            bool isMoving = (moveX != 0 || moveY != 0);
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
                {
                    animator.SetFloat("MoveX", movimiento.x);
                    animator.SetFloat("MoveY", 0f);
                }
                else
                {
                    animator.SetFloat("MoveX", 0f);
                    animator.SetFloat("MoveY", movimiento.y);
                }

                if (moveX != 0) animator.SetFloat("LastMoveX", moveX);
                if (moveY != 0) animator.SetFloat("LastMoveY", moveY);

                // Volteo del sprite según dirección X
                float SCALE_X = initialScaleX;
                float SCALE_Y = initialScaleY;

                if (moveX > 0) transform.localScale = new Vector3(SCALE_X, SCALE_Y, 1f);
                else if (moveX < 0) transform.localScale = new Vector3(-SCALE_X, SCALE_Y, 1f);
            }
            else
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }

        // Rotación de la linterna según movimiento
        if (movimiento != Vector2.zero && conoDeLuz != null)
        {
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            angulo += offsetAngulo;

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
        }

        // Posicionamiento de la linterna
        if (conoDeLuz != null)
        {
            Vector3 direccionLuz = conoDeLuz.transform.up;
            Vector3 direccionHaciaAtras = -direccionLuz;
            Vector3 posicionObjetivo = transform.position + direccionHaciaAtras * offsetDistancia;
            conoDeLuz.position = posicionObjetivo;
        }
    }

    void FixedUpdate()
    {
        // Aplicación física del movimiento
        if (rb != null)
        {
            rb.MovePosition(rb.position + movimiento * speed * Time.fixedDeltaTime);
        }
    }

    public void RecibirGolpe()
    {
        if (dead) return;

        salud -= 1;
        ActualizaHud();

        // Reproduce sonido de daño y guarda vidas en GameManager
        if (SonidoManager.Instance != null) SonidoManager.Instance.ReproducirAtacado();

        if (gameManager != null)
            gameManager.playerLives = Mathf.FloorToInt(salud);

        if (salud <= 0)
        {
            salud = 0;
            dead = true;

            SceneManager.LoadScene(0);
        }
        else
        {
            transform.position = puntoDeInicio;
        }
    }


    public bool GanarVida()
    {
        float maximoDeVidas = 3f;

        // Añade vida si no está al máximo y actualiza HUD
        if (salud < maximoDeVidas)
        {
            salud += 1;
            ActualizaHud();

            if (gameManager != null)
                gameManager.playerLives = Mathf.FloorToInt(salud);

            return true;
        }

        return false;
    }


    public void RecolectarLampara()
    {
        if (timerScript != null)
        {
            timerScript.FinalizarNivel();
        }

        if (gameManager != null)
        {
            gameManager.PasarDeFase();
        }
    }


    public void ActualizaHud()
    {
        // Busca el texto de vidas si se ha perdido la referencia
        if (vidasHud == null)
        {
            // Usamos FindObjectsByType para buscar en la escena de forma moderna
            foreach (var t in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                if (t.text.Contains("Vidas") || t.name.ToLower().Contains("vida") || t.name.ToLower().Contains("vidas"))
                {
                    vidasHud = t;
                    break;
                }
            }
        }

        if (vidasHud != null)
        {
            vidasHud.text = "Vidas: " + Mathf.FloorToInt(salud).ToString();
        }
    }
}
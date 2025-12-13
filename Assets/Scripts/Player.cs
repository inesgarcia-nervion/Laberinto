using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Transform conoDeLuz;
    public float velocidadRotacion = 15f;
    public float offsetAngulo = -90f;

    [Header("Configuración de Posición de Luz")]
    public float offsetDistancia = 0.5f;

    [Header("Movimiento y Vidas")]
    public float speed = 5.0f;
    private Rigidbody2D rb;
    private Vector2 movimiento;

    // VARIABLES DEL ANIMATOR
    private Animator animator;

    // VARIABLES AÑADIDAS PARA CORREGIR LA ESCALA AL HACER FLIPPING
    private float initialScaleX;
    private float initialScaleY;

    public float salud;
    public bool dead = false;
    public TextMeshProUGUI vidasHud;

    private Vector3 puntoDeInicio;
    private GameManager gameManager;

    private Timer timerScript;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // GUARDA LA ESCALA DE DISEÑO AL INICIO
        initialScaleX = transform.localScale.x;
        initialScaleY = transform.localScale.y;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            puntoDeInicio = transform.position;

            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance; // Conectamos con el GM Maestro
                salud = gameManager.playerLives;    // Recuperamos las vidas guardadas
            }
            else
            {
                // Solo entra aquí si probamos la Fase 2 suelta sin pasar por el menú/fase1
                gameManager = FindObjectOfType<GameManager>();
                salud = 2;
            }

            timerScript = FindObjectOfType<Timer>();

            ActualizaHud();
        }

        if (animator == null)
        {
            Debug.LogError("El componente Animator es requerido para las animaciones y no se encontró.");
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(moveX, moveY).normalized;

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

        if (movimiento != Vector2.zero && conoDeLuz != null)
        {
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            angulo += offsetAngulo;

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
        }

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

        // Guardar vidas en GameManager persistente
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
        if (vidasHud == null)
        {
            // Intento de reasignar el HUD de vidas en la escena (heurística por texto/nombre)
            foreach (var t in FindObjectsOfType<TextMeshProUGUI>())
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
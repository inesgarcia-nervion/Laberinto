using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // ... [Variables de Configuración de Luz y Movimiento] ...
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
            salud = 2; // Vida inicial

            puntoDeInicio = transform.position;
            gameManager = FindObjectOfType<GameManager>();

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
        // -----------------------------------------------------------
        // 1. INPUT DEL JUGADOR
        // -----------------------------------------------------------
        float moveX = Input.GetAxisRaw("Horizontal"); // Raw para la prioridad de animación
        float moveY = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(moveX, moveY).normalized; // Normalizado para el movimiento físico


        // -----------------------------------------------------------
        // 2. LÓGICA DEL ANIMATOR (Priorización de Diagonales)
        // -----------------------------------------------------------
        if (animator != null)
        {
            bool isMoving = (moveX != 0 || moveY != 0);

            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                // LÓGICA DE PRIORIZACIÓN DE DIAGONALES:
                // Solo enviamos MoveX o MoveY, anulando el eje menos dominante para evitar el glitch diagonal.
                if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
                {
                    // PRIORIDAD X (Lateral)
                    animator.SetFloat("MoveX", movimiento.x);
                    animator.SetFloat("MoveY", 0f);
                }
                else
                {
                    // PRIORIDAD Y (Vertical)
                    animator.SetFloat("MoveX", 0f);
                    animator.SetFloat("MoveY", movimiento.y);
                }

                // c) Guardar la ÚLTIMA dirección (Importante para Idle -> Idle)
                if (moveX != 0)
                {
                    animator.SetFloat("LastMoveX", moveX);
                }
                if (moveY != 0)
                {
                    animator.SetFloat("LastMoveY", moveY);
                }

                // d) Flipping del Sprite para Lateral
                float SCALE_X = initialScaleX;
                float SCALE_Y = initialScaleY;

                if (moveX > 0)
                {
                    transform.localScale = new Vector3(SCALE_X, SCALE_Y, 1f);
                }
                else if (moveX < 0)
                {
                    transform.localScale = new Vector3(-SCALE_X, SCALE_Y, 1f);
                }
            }
            else // Si isMoving es False (el personaje se detiene)
            {
                // Limpieza de los valores de movimiento
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }

        // -----------------------------------------------------------
        // 3. ROTACIÓN DEL CONO DE LUZ (Rotación suave al moverse)
        // -----------------------------------------------------------
        if (movimiento != Vector2.zero && conoDeLuz != null)
        {
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            angulo += offsetAngulo;

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
        }

        // -----------------------------------------------------------
        // 4. POSICIONAMIENTO DEL CONO DE LUZ (Offset trasero)
        // -----------------------------------------------------------
        if (conoDeLuz != null)
        {
            // Usamos la dirección local 'up' del cono (vector hacia adelante)
            // y la invertimos para el offset trasero.
            Vector3 direccionLuz = conoDeLuz.transform.up;
            Vector3 direccionHaciaAtras = -direccionLuz;

            // Posición objetivo = Posición del Player + Vector de Desplazamiento
            Vector3 posicionObjetivo = transform.position + direccionHaciaAtras * offsetDistancia;

            // Aplicar la nueva posición
            conoDeLuz.position = posicionObjetivo;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Movimiento físico basado en el vector normalizado
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


    private void ActualizaHud()
    {
        if (vidasHud != null)
        {
            vidasHud.text = "Vidas: " + Mathf.FloorToInt(salud).ToString();
        }
    }
}
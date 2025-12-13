using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    [Header("Referencias")]
    [SerializeField]
    public GameObject player;

    [Header("Movimiento y Rangos")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rangoAlerta = 3.0f; // Distancia para empezar a perseguir
    [SerializeField] private float rangoDetener = 0.5f; // Distancia para detenerse y atacar
    [SerializeField] private float attackCooldown = 1f; // Tiempo entre ataques

    [Header("Colisiones")]
    [SerializeField] private LayerMask obstacleLayer = ~0; // Ajusta en el Inspector para solo paredes/obstáculos

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D myCollider;

    [Header("Estado Interno")]
    private float ultimoAtaque = 0;

    // Almacena la última dirección de movimiento (para el Flipping)
    private float initialScaleX;

    private Player playerScript;

    // --- Nuevos campos para evitar quedarse atascado ---
    [Header("Evitar atascos")]
    [Tooltip("Tiempo (s) que el enemigo se desplaza a lo largo de la pared tras chocar.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float bumpDuration = 0.25f; // tiempo que se mueve a lo largo de la pared

    [Tooltip("Multiplicador de velocidad durante el movimiento lateral (bump).")]
    [Range(0.5f, 2f)]
    [SerializeField] private float bumpSpeedMultiplier = 1.0f; // multiplicador de velocidad durante bump

    private float bumpTimer = 0f;
    private Vector2 bumpDirection = Vector2.zero;
    // ---------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();

        // Guardamos la escala inicial para el Flipping
        initialScaleX = transform.localScale.x;

        if (player == null)
        {
            playerScript = FindAnyObjectByType<Player>();
            if (playerScript != null)
            {
                player = playerScript.gameObject;
            }
        }
        else
        {
            playerScript = player.GetComponent<Player>();
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        if (playerScript == null || playerScript.dead)
        {
            // Si el jugador está muerto o no existe, detenemos la animación.
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 direccionTotal = player.transform.position - transform.position;
        float distancia = direccionTotal.magnitude;

        // Dirección de movimiento normalizada (para los parámetros MoveX/Y)
        Vector2 direccionNormalizada = direccionTotal.normalized;

        // -----------------------------------------------------------
        // LÓGICA DE DETECCIÓN Y MOVIMIENTO
        // -----------------------------------------------------------
        bool isMoving = false; // El estado base

        if (distancia <= rangoAlerta)
        {
            if (distancia <= rangoDetener)
            {
                // ATACAR: Pararse
                if (Time.time > ultimoAtaque + attackCooldown)
                {
                    Pegar();
                    ultimoAtaque = Time.time;
                }
            }
            else
            {
                // PERSEGUIR: Moverse
                isMoving = true;
            }
        }

        // -----------------------------------------------------------
        // SINCRONIZACIÓN CON EL ANIMATOR (CORRECCIÓN DE DIAGONALES)
        // -----------------------------------------------------------
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                // ******************************************************
                // LÓGICA DE PRIORIZACIÓN DE DIAGONALES (SOLUCIÓN)
                // ******************************************************
                if (Mathf.Abs(direccionNormalizada.x) > Mathf.Abs(direccionNormalizada.y))
                {
                    // PRIORIDAD X (Lateral)
                    animator.SetFloat("MoveX", direccionNormalizada.x);
                    animator.SetFloat("MoveY", 0f); // Anula Y para forzar la animación lateral
                }
                else
                {
                    // PRIORIDAD Y (Vertical)
                    animator.SetFloat("MoveX", 0f); // Anula X para forzar la animación vertical
                    animator.SetFloat("MoveY", direccionNormalizada.y);
                }

                // 2. Guardar la ÚLTIMA dirección (para Idle)
                // Se sigue usando la dirección normalizada completa para el Idle
                if (direccionNormalizada.x != 0)
                {
                    animator.SetFloat("LastMoveX", direccionNormalizada.x);
                }
                if (direccionNormalizada.y != 0)
                {
                    animator.SetFloat("LastMoveY", direccionNormalizada.y);
                }

                // 3. Flipping del Sprite
                GirarSprite(direccionNormalizada.x);
            }
            else // Si no se está moviendo (Idle/Attack), limpiamos MoveX/Y
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }

        // al detectar que el jugador está en rango de alerta (puede agregarse dentro de Update/FixedUpdate)
        if (SonidoManager.Instance != null)
        {
            if (distancia <= rangoAlerta)
                SonidoManager.Instance.StartSiren(); // o SetSirenIntensity(...) para control fino
            else
                SonidoManager.Instance.StopSiren();
        }
    }

    void FixedUpdate()
    {
        if (playerScript == null || playerScript.dead) return;

        // Si estamos en "bump" (reciente choque con pared), nos movemos a lo largo de bumpDirection
        if (bumpTimer > 0f)
        {
            Vector2 bumpMove = bumpDirection * speed * bumpSpeedMultiplier * Time.fixedDeltaTime;
            if (CanMove(bumpMove))
            {
                rb.MovePosition(rb.position + bumpMove);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            bumpTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 direccion = player.transform.position - transform.position;
        float distancia = direccion.magnitude;

        // Solo moverse si está persiguiendo (distancia > rangoDetener)
        if (distancia <= rangoAlerta && distancia > rangoDetener)
        {
            Vector2 direccionNormalizada = direccion.normalized;
            Vector2 desiredMove = direccionNormalizada * speed * Time.fixedDeltaTime;

            // Intentamos la trayectoria completa; si está bloqueada intentamos eje X o Y por separado
            if (CanMove(desiredMove))
            {
                rb.MovePosition(rb.position + desiredMove);
            }
            else
            {
                Vector2 moveX = new Vector2(desiredMove.x, 0f);
                Vector2 moveY = new Vector2(0f, desiredMove.y);

                if (CanMove(moveX))
                {
                    rb.MovePosition(rb.position + moveX);
                }
                else if (CanMove(moveY))
                {
                    rb.MovePosition(rb.position + moveY);
                }
                else
                {
                    // No se puede mover en ninguna dirección: detener velocidad para evitar "residuo"
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            // Si no persigue, nos aseguramos de que no haya velocidad residual
            rb.linearVelocity = Vector2.zero;
        }
    }


    private void Pegar()
    {
        playerScript.Hit();
    }

    private void GirarSprite(float direccionX)
    {
        // Usamos la escala inicial guardada en Start()
        if (direccionX > 0.0f)
        {
            // Mira a la derecha
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, 1.0f);
        }
        else if (direccionX < 0.0f)
        {
            // Mira a la izquierda
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, 1.0f);
        }
    }

    // Comprueba si se puede desplazar la distancia 'move' sin chocar con obstáculos
    // Ahora usa Rigidbody2D.Cast, que respeta la forma del collider y evita falsos positivos por solapamiento.
    private bool CanMove(Vector2 move)
    {
        if (move.sqrMagnitude < Mathf.Epsilon) return true;

        float distance = move.magnitude + 0.01f;
        Vector2 dir = move.normalized;

        // Usamos ContactFilter2D para respetar el obstacleLayer
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[6];
        int hitCount = rb.Cast(dir, filter, hits, distance);

        for (int i = 0; i < hitCount; i++)
        {
            var col = hits[i].collider;
            if (col == null) continue;
            // Ignorar colisiones con el propio Player (si por alguna razón está en la misma capa)
            if (player != null && col.gameObject == player) continue;
            // Si cualquier otro collider está en la ruta, no podemos movernos
            return false;
        }

        return true;
    }

    // Método público llamado por la pared cuando detecta colisión.
    // 'contactNormal' debe ser la normal de la colisión (desde la pared hacia el enemigo).
    public void OnWallHit(Vector2 contactNormal)
    {
        // Calculamos dos perpendiculares y elegimos la que "apunta" más hacia el jugador
        Vector2 perp1 = new Vector2(-contactNormal.y, contactNormal.x).normalized;
        Vector2 perp2 = -perp1;

        Vector2 toPlayer = Vector2.zero;
        if (player != null)
        {
            toPlayer = (player.transform.position - transform.position).normalized;
        }

        Vector2 chosen = Vector2.Dot(perp1, toPlayer) > Vector2.Dot(perp2, toPlayer) ? perp1 : perp2;

        bumpDirection = chosen;
        bumpTimer = bumpDuration;
    }

    // Si quieres cambiar los parámetros en tiempo de ejecución desde otro script:
    public void SetBumpParameters(float duration, float speedMultiplier)
    {
        bumpDuration = duration;
        bumpSpeedMultiplier = speedMultiplier;
    }
}
using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    [Header("Referencias")]
    [SerializeField]
    public GameObject player; // Referencia al jugador

    [Header("Movimiento y Rangos")]
    [SerializeField] private float speed = 1f; // Velocidad de persecución
    [SerializeField] private float rangoAlerta = 3.0f; // Distancia para empezar a perseguir
    [SerializeField] private float rangoDetener = 0.5f; // Distancia para atacar
    [SerializeField] private float attackCooldown = 1f; // Tiempo de espera entre ataques

    [Header("Colisiones")]
    [SerializeField] private LayerMask obstacleLayer = ~0; // Capas consideradas obstáculos

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D myCollider;

    [Header("Estado Interno")]
    private float ultimoAtaque = 0;

    // Almacena la última dirección para el volteo de sprite
    private float initialScaleX;

    private Player playerScript;

    // --- Evitar atascos en paredes ---
    [Header("Evitar atascos")]
    [Tooltip("Tiempo (s) que el enemigo se desliza por la pared tras chocar.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float bumpDuration = 0.25f;

    [Tooltip("Multiplicador de velocidad al deslizarse por la pared.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float bumpSpeedMultiplier = 1.0f;

    private float bumpTimer = 0f;
    private Vector2 bumpDirection = Vector2.zero;
    // ---------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();

        // Guardamos la escala inicial para controlar hacia dónde mira
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
            // Detiene animación si el jugador muere
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 direccionTotal = player.transform.position - transform.position;
        float distancia = direccionTotal.magnitude;

        Vector2 direccionNormalizada = direccionTotal.normalized;

        // -----------------------------------------------------------
        // LÓGICA DE DETECCIÓN Y MOVIMIENTO
        // -----------------------------------------------------------
        bool isMoving = false;

        if (distancia <= rangoAlerta)
        {
            if (distancia <= rangoDetener)
            {
                // ATACAR
                if (Time.time > ultimoAtaque + attackCooldown)
                {
                    Pegar();
                    ultimoAtaque = Time.time;
                }
            }
            else
            {
                // PERSEGUIR
                isMoving = true;
            }
        }

        // -----------------------------------------------------------
        // ANIMACIÓN Y DIRECCIÓN
        // -----------------------------------------------------------
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                // Prioriza el eje con mayor movimiento para la animación
                if (Mathf.Abs(direccionNormalizada.x) > Mathf.Abs(direccionNormalizada.y))
                {
                    animator.SetFloat("MoveX", direccionNormalizada.x);
                    animator.SetFloat("MoveY", 0f);
                }
                else
                {
                    animator.SetFloat("MoveX", 0f);
                    animator.SetFloat("MoveY", direccionNormalizada.y);
                }

                // Guarda la última dirección para el estado Idle
                if (direccionNormalizada.x != 0)
                {
                    animator.SetFloat("LastMoveX", direccionNormalizada.x);
                }
                if (direccionNormalizada.y != 0)
                {
                    animator.SetFloat("LastMoveY", direccionNormalizada.y);
                }

                GirarSprite(direccionNormalizada.x);
            }
            else
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }

        // Control del sonido de sirena según la distancia
        if (SonidoManager.Instance != null)
        {
            if (distancia <= rangoAlerta)
                SonidoManager.Instance.IniciarSirena();
            else
                SonidoManager.Instance.DetenerSirena();
        }
    }

    void FixedUpdate()
    {
        if (playerScript == null || playerScript.dead) return;

        // Movimiento de deslizamiento tras chocar con pared
        if (bumpTimer > 0f)
        {
            Vector2 bumpMove = bumpDirection * speed * bumpSpeedMultiplier * Time.fixedDeltaTime;
            if (PuedeMoverse(bumpMove))
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

        // Movimiento normal de persecución
        if (distancia <= rangoAlerta && distancia > rangoDetener)
        {
            Vector2 direccionNormalizada = direccion.normalized;
            Vector2 desiredMove = direccionNormalizada * speed * Time.fixedDeltaTime;

            // Intenta mover directo, si no, intenta por ejes separados
            if (PuedeMoverse(desiredMove))
            {
                rb.MovePosition(rb.position + desiredMove);
            }
            else
            {
                Vector2 moveX = new Vector2(desiredMove.x, 0f);
                Vector2 moveY = new Vector2(0f, desiredMove.y);

                if (PuedeMoverse(moveX))
                {
                    rb.MovePosition(rb.position + moveX);
                }
                else if (PuedeMoverse(moveY))
                {
                    rb.MovePosition(rb.position + moveY);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }


    private void Pegar()
    {
        playerScript.RecibirGolpe();
    }

    private void GirarSprite(float direccionX)
    {
        // Voltea el sprite según la dirección horizontal
        if (direccionX > 0.0f)
        {
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, 1.0f);
        }
        else if (direccionX < 0.0f)
        {
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, 1.0f);
        }
    }

    // Comprueba si hay obstáculos en la dirección deseada mediante Raycast
    private bool PuedeMoverse(Vector2 move)
    {
        if (move.sqrMagnitude < Mathf.Epsilon) return true;

        float distance = move.magnitude + 0.01f;
        Vector2 dir = move.normalized;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[6];
        int hitCount = rb.Cast(dir, filter, hits, distance);

        for (int i = 0; i < hitCount; i++)
        {
            var col = hits[i].collider;
            if (col == null) continue;
            // Ignora al propio jugador si está en la capa
            if (player != null && col.gameObject == player) continue;
            return false;
        }

        return true;
    }

    // Calcula una dirección de rebote al chocar con una pared
    public void AlGolpearPared(Vector2 contactNormal)
    {
        Vector2 perp1 = new Vector2(-contactNormal.y, contactNormal.x).normalized;
        Vector2 perp2 = -perp1;

        Vector2 toPlayer = Vector2.zero;
        if (player != null)
        {
            toPlayer = (player.transform.position - transform.position).normalized;
        }

        // Elige la dirección perpendicular que acerca más al jugador
        Vector2 chosen = Vector2.Dot(perp1, toPlayer) > Vector2.Dot(perp2, toPlayer) ? perp1 : perp2;

        bumpDirection = chosen;
        bumpTimer = bumpDuration;
    }

    // Configura duración y velocidad del rebote
    public void ConfigurarParametrosRebote(float duration, float speedMultiplier)
    {
        bumpDuration = duration;
        bumpSpeedMultiplier = speedMultiplier;
    }
}
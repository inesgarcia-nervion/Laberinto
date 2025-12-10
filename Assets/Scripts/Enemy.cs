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

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Estado Interno")]
    private float ultimoAtaque = 0; 

    private Player playerScript;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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
            if (animator != null) animator.SetBool("running", false);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 direccion = player.transform.position - transform.position;
        float distancia = direccion.magnitude;


        if (distancia <= rangoAlerta)
        {
            if (distancia <= rangoDetener)
            {
                if (animator != null) animator.SetBool("running", false);
                rb.linearVelocity = Vector2.zero; 

                if (Time.time > ultimoAtaque + attackCooldown)
                {
                    Pegar();
                    ultimoAtaque = Time.time;
                }
            }
            else
            {
                if (animator != null) animator.SetBool("running", true);
            }
        }
        else
        {
            // FUERA DE RANGO: Detenerse
            if (animator != null) animator.SetBool("running", false);
            rb.linearVelocity = Vector2.zero; 
        }
        // Girarse hacia el jugador
        GirarSprite(direccion.x);
    }

    void FixedUpdate()
    {
        if (playerScript == null || playerScript.dead) return;

        Vector3 direccion = player.transform.position - transform.position;
        float distancia = direccion.magnitude;

        // Solo moverse si está persiguiendo (distancia > rangoDetener)
        if (distancia <= rangoAlerta && distancia > rangoDetener)
        {
            Vector2 direccionNormalizada = direccion.normalized;

            Vector2 targetPosition = rb.position + direccionNormalizada * speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
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
        if (direccionX > 0.0f)
        {
            // Mira a la derecha
            transform.localScale = new Vector3(0.25f, 0.25f, 1.0f);
        }
        else if (direccionX < 0.0f)
        {
            // Mira a la izquierda
            transform.localScale = new Vector3(-0.25f, 0.25f, 1.0f);
        }
    }
}
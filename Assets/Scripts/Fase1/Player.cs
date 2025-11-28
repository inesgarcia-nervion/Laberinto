using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Transform conoDeLuz;
    public float velocidadRotacion = 15f;
    // AJUSTA ESTO SI APUNTA MAL: Prueba con -90, 90 o 180 si no sale a la primera
    public float offsetAngulo = -90f; 

    [Header("Movimiento")]
    public float speed = 5.0f; // Subí un poco la velocidad base
    private Rigidbody2D rb;
    private Vector2 movimiento; // Guardamos el input aquí para usarlo en FixedUpdate

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 1. CAPTURAR INPUT (En Update siempre)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movimiento = new Vector2(moveX, moveY).normalized;

        // 2. ROTACIÓN DEL CONO (Lógica visual en Update)
        if (movimiento != Vector2.zero)
        {
            // Calculamos el ángulo matemático (0º es Derecha)
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            
            // APLICAMOS LA CORRECCIÓN (OFFSET)
            // Esto corrige que tu luz apunte hacia el lado equivocado
            angulo += offsetAngulo;

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
        }
    }

    // 3. MOVIMIENTO FÍSICO (Siempre en FixedUpdate para evitar temblores)
    void FixedUpdate()
    {
        if (rb != null)
        {
            // MovePosition es mucho más sólido que Translate para colisiones
            rb.MovePosition(rb.position + movimiento * speed * Time.fixedDeltaTime);
        }
    }
}
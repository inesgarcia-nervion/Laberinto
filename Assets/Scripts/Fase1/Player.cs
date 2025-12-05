using TMPro;
using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            salud = 2;

            puntoDeInicio = transform.position;
            gameManager = FindObjectOfType<GameManager>();

            ActualizaHud();
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(moveX, moveY).normalized;

        if (movimiento != Vector2.zero)
        {
            float angulo = Mathf.Atan2(movimiento.y, movimiento.x) * Mathf.Rad2Deg;
            angulo += offsetAngulo;
            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);
            conoDeLuz.rotation = Quaternion.Lerp(conoDeLuz.rotation, rotacionObjetivo, Time.deltaTime * velocidadRotacion);
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

        if (salud <= 0)
        {
            salud = 0;
            dead = true;
            Destroy(gameObject);
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
            return true; // Nueva vida
        }

        return false; // Ya estaba al máximo y no lo recolecta
    }

    public void RecolectarLampara()
    {
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
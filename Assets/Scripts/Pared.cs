using UnityEngine;

public class Pared : MonoBehaviour
{
    [Header("Ajustes")]
    [SerializeField] private bool enforceLayerNamedPared = true; // Fuerza la capa "Pared"

    void OnValidate()
    {
        // Asigna la capa automáticamente en el editor
        if (enforceLayerNamedPared)
        {
            int layer = LayerMask.NameToLayer("Pared");
            if (layer != -1)
            {
                gameObject.layer = layer;
            }
        }
    }

    void Awake()
    {
        // Advierte si falta el Collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"[Pared] '{name}' no tiene Collider2D. Añade uno para que funcione como muro.", this);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Notifica al enemigo que ha chocado con la pared
        if (collision.gameObject.TryGetComponent<EnemyScript>(out var enemy))
        {
            Vector2 normal = Vector2.zero;
            if (collision.contacts != null && collision.contacts.Length > 0)
            {
                // Calcula la normal para el rebote
                normal = collision.contacts[0].normal;
                Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
                if (Vector2.Dot(normal, toEnemy) < 0f)
                {
                    normal = -normal;
                }
            }
            else
            {
                normal = (enemy.transform.position - transform.position).normalized;
            }

            enemy.AlGolpearPared(normal);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si es trigger, también notifica al enemigo
        if (other.gameObject.TryGetComponent<EnemyScript>(out var enemy))
        {
            Vector2 normal = (enemy.transform.position - transform.position).normalized;
            enemy.AlGolpearPared(normal);
        }
    }
}
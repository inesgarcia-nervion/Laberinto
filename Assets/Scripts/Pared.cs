using UnityEngine;

public class Pared : MonoBehaviour
{
    [Header("Ajustes")]
    [SerializeField] private bool enforceLayerNamedPared = true;

    void OnValidate()
    {
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
        // Comprobación opcional: avisar si falta collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"[Pared] '{name}' no tiene Collider2D. Añade uno para que funcione como muro.", this);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Intentamos notificar al EnemyScript si existe
        if (collision.gameObject.TryGetComponent<EnemyScript>(out var enemy))
        {
            Vector2 normal = Vector2.zero;
            if (collision.contacts != null && collision.contacts.Length > 0)
            {
                // contact.normal es la normal del contacto (apunta desde el otro collider hacia este)
                normal = collision.contacts[0].normal;
                // Queremos la normal desde la pared hacia el enemigo -> invertir si es necesario
                // Si la normal apunta desde enemy hacia wall, invertimos para que sea wall->enemy.
                // Comprobamos aproximación simple: si la normal apunta hacia el centro del enemigo, lo usamos.
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

            enemy.OnWallHit(normal);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si la pared es trigger, calculamos normal aproximada
        if (other.gameObject.TryGetComponent<EnemyScript>(out var enemy))
        {
            Vector2 normal = (enemy.transform.position - transform.position).normalized;
            enemy.OnWallHit(normal);
        }
    }
}

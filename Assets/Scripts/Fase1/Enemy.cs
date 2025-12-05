using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

 using System.Collections; // Necesario si se usa StartCoroutine, aunque no en este ejemplo.

    namespace Fase1
    {
        // 💡 NOTA: Se asume que existe una clase Player con una propiedad bool 'dead' y un método 'Hit()'.
        // Si no tienes esta clase en el proyecto, el código no compilará hasta que la crees.

        // Si no se usa NavMeshAgent, no se necesita el using UnityEngine.AI, 
        // pero si lo dejas en el código y el componente no existe, no pasa nada.

        public class Enemy : MonoBehaviour
        {
            // -------------------------------------------------------------------
            //                       PROPIEDADES PÚBLICAS
            // -------------------------------------------------------------------
            [Header("Zonas")]
            [Tooltip("Centro de la zona de patrulla (si está vacío usa la posición inicial)")]
            public Transform zonaCentro;
            [Tooltip("Radio dentro del que patrulla")]
            public float patrolRadius = 3f;
            [Tooltip("Radio en el que detecta al jugador (medido desde el borde del enemigo)")]
            public float detectionRadius = 5f;
            [Tooltip("Radio en el que puede golpear al jugador (medido desde el borde del enemigo)")]
            public float attackRange = 1f;

            [Header("Movimiento")]
            public float patrolSpeed = 2f;
            public float chaseSpeed = 3.5f;
            [Tooltip("Tiempo que espera en un punto de patrulla antes de elegir otro")]
            public float waitAtPoint = 1f;

            [Header("Ataque")]
            [Tooltip("Segundos entre ataques")]
            public float attackCooldown = 1f;

            [Header("Evasión de Obstáculos (Fallback)")]
            public LayerMask obstacleMask;
            [Tooltip("Distancia a la que empieza a esquivar obstáculos")]
            public float obstacleDetectDistance = 1.5f; // Aumentado para mejor reacción
            [Tooltip("Número de rayos para buscar rutas alternativas")]
            public int avoidanceSamples = 18; // Aumentado para mayor precisión

            // -------------------------------------------------------------------
            //                       PROPIEDADES PRIVADAS
            // -------------------------------------------------------------------
            Rigidbody2D rb;

            // Se declara aunque no se use, para compatibilidad con el código original,
            // pero se mantiene nulo para forzar el fallback.
            // public NavMeshAgent navAgent; 

            Vector2 centro;
            Vector2 puntoObjetivo;
            float waitTimer = 0f;

            float ultimoAtaque = -Mathf.Infinity;

            // Referencias al jugador y sus colliders
            Player playerRef;
            Collider2D playerCollider;
            Collider2D enemyCollider;
            float playerRadius = 0f;
            float enemyRadius = 0f;

            enum State { Patrolling, Chasing }
            State estado = State.Chasing; // Inicia en Chasing para no patrullar por defecto

            // campo auxiliar usado en Start (existía en el original)
            Vector3 lastPlayerPos;

            // -------------------------------------------------------------------
            //                             MÉTODOS
            // -------------------------------------------------------------------

            void Start()
            {
                rb = GetComponent<Rigidbody2D>();
                enemyCollider = GetComponent<Collider2D>();
                centro = (zonaCentro != null) ? (Vector2)zonaCentro.position : (rb != null ? rb.position : (Vector2)transform.position);

                // Encontrar Player en escena
                playerRef = FindObjectOfType<Player>();
                if (playerRef != null)
                    playerCollider = playerRef.GetComponent<Collider2D>();

                // Si se usara NavMeshAgent, aquí se obtendría, pero lo ignoramos.
                // navAgent = GetComponent<NavMeshAgent>(); 

                ActualizaRadios();
                EligeNuevoPuntoPatrulla();
                if (playerRef != null) lastPlayerPos = playerRef.transform.position;

                estado = State.Chasing; // Se queda en estado de persecución
            }

            void Update()
            {
                // Mantener radios actualizados (si cambian escala/coliders en runtime)
                if (playerRef != null && playerCollider == null)
                    playerCollider = playerRef.GetComponent<Collider2D>();

                ActualizaRadios();

                if (playerRef != null && !playerRef.dead)
                {
                    float distanciaCentros = Vector2.Distance(transform.position, playerRef.transform.position);
                    float separation = distanciaCentros - (enemyRadius + playerRadius); // distancia entre bordes

                    // 1. DENTRO DEL RANGO DE ATAQUE
                    if (separation <= attackRange)
                    {
                        estado = State.Chasing; // Asegura que se acerca antes de atacar
                        TryAttack(separation);
                        return;
                    }

                    // 2. DENTRO DEL RADIO DE DETECCIÓN
                    if (separation <= detectionRadius)
                    {
                        estado = State.Chasing;
                        return;
                    }

                    // 3. FUERA DE RANGO: Si sale del rango de detección, el enemigo podría 
                    // volver a Patrolling (si se desea) o quedarse en Chasing
                }
                // Si el jugador muere o desaparece, no se mueve (sigue en Chasing sin objetivo)
                // Para volver a Patrolling, descomenta esta línea:
                // else { estado = State.Patrolling; } 
            }

            void FixedUpdate()
            {
                if (rb == null) return;

                // 💡 No hay sincronización de NavMeshAgent

                switch (estado)
                {
                    case State.Patrolling:
                        MoverPatrulla();
                        break;
                    case State.Chasing:
                        if (playerRef != null && !playerRef.dead)
                            PersigueJugador();
                        // Si el jugador no existe o está muerto, se detiene
                        break;
                }
            }

            void MoverPatrulla()
            {
                Vector2 posicionActual = rb.position;
                Vector2 destino = puntoObjetivo;

                // 💡 Usamos la lógica de evasión también en Patrulla para evitar obstáculos
                Vector2 toTarget = destino - posicionActual;
                Vector2 desiredDir = toTarget.normalized;
                Vector2 moveDir = GetUnblockedDirection(desiredDir, toTarget.magnitude);

                if (moveDir != Vector2.zero)
                {
                    Vector2 nuevaPos = rb.position + moveDir * patrolSpeed * Time.fixedDeltaTime;
                    rb.MovePosition(nuevaPos);
                }

                // Si está cerca del punto objetivo (0.1f) Y la dirección deseada apunta cerca de él
                if (Vector2.Distance(posicionActual, destino) < 0.1f)
                {
                    waitTimer += Time.fixedDeltaTime;
                    if (waitTimer >= waitAtPoint)
                    {
                        waitTimer = 0f;
                        EligeNuevoPuntoPatrulla();
                    }
                }
            }

            void PersigueJugador()
            {
                if (playerRef == null) return;

                // 💡 Usamos el FALLBACK (evasión por raycasts)
                Vector2 posicionJugador = playerRef.transform.position;
                Vector2 toTarget = posicionJugador - rb.position;
                float distanceToTarget = toTarget.magnitude;

                if (distanceToTarget < 0.001f) return;

                Vector2 desiredDir = toTarget.normalized;

                // Obtener la dirección no bloqueada
                Vector2 moveDir = GetUnblockedDirection(desiredDir, distanceToTarget);

                // Mover el Rigidbody2D
                Vector2 nuevaPosFallback = rb.position + moveDir * chaseSpeed * Time.fixedDeltaTime;
                rb.MovePosition(nuevaPosFallback);
            }

            // -------------------------------------------------------------------
            //                         Evasión de Obstáculos
            // -------------------------------------------------------------------

            Vector2 GetUnblockedDirection(Vector2 desiredDir, float checkDistance)
            {
                // 1. Revisa si la dirección deseada está bloqueada
                // Usamos Mathf.Max para que la distancia de chequeo sea al menos obstacleDetectDistance
                if (!Physics2D.Raycast(rb.position, desiredDir, Mathf.Max(checkDistance, obstacleDetectDistance), obstacleMask))
                    return desiredDir; // Si está libre, va hacia allá.

                // 2. Si está bloqueada, prueba direcciones rotadas
                for (int i = 1; i <= avoidanceSamples / 2; i++)
                {
                    float angleStep = 360f / avoidanceSamples;

                    // Prueba a la derecha
                    float angleRight = angleStep * i;
                    Vector2 d1 = Rotate(desiredDir, angleRight);
                    if (!Physics2D.Raycast(rb.position, d1, obstacleDetectDistance, obstacleMask))
                        return d1;

                    // Prueba a la izquierda
                    float angleLeft = -angleStep * i;
                    Vector2 d2 = Rotate(desiredDir, angleLeft);
                    if (!Physics2D.Raycast(rb.position, d2, obstacleDetectDistance, obstacleMask))
                        return d2;
                }

                // 💥 MODIFICACIÓN CLAVE: Si todas las direcciones están bloqueadas, DEVUELVE CERO.
                // Esto detiene al enemigo, evitando que se empuje a sí mismo contra las paredes.
                return Vector2.zero;
            }

            Vector2 Rotate(Vector2 v, float degrees)
            {
                float rad = degrees * Mathf.Deg2Rad;
                float sin = Mathf.Sin(rad);
                float cos = Mathf.Cos(rad);
                return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
            }

            // -------------------------------------------------------------------
            //                            Utilidades
            // -------------------------------------------------------------------

            void TryAttack(float separation)
            {
                if (playerRef == null || playerRef.dead) return;

                float ahora = Time.time;
                if (ahora - ultimoAtaque < attackCooldown) return;

                if (separation <= attackRange)
                {
                    // 💡 Asumimos que la clase Player tiene un método Hit()
                    // Si tu player tiene un método diferente, modifícalo aquí.
                    playerRef.Hit();
                    ultimoAtaque = ahora;
                }
            }

            void EligeNuevoPuntoPatrulla()
            {
                Vector2 aleatorio = UnityEngine.Random.insideUnitCircle * patrolRadius;
                puntoObjetivo = centro + aleatorio;
            }

            void ActualizaRadios()
            {
                if (enemyCollider != null)
                    enemyRadius = Mathf.Max(enemyCollider.bounds.extents.x, enemyCollider.bounds.extents.y);
                else
                    enemyRadius = 0.5f * Mathf.Max(transform.localScale.x, transform.localScale.y);

                if (playerRef == null)
                {
                    playerRadius = 0f;
                    return;
                }

                if (playerCollider != null)
                    playerRadius = Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y);
                else
                    playerRadius = 0.5f * Mathf.Max(playerRef.transform.localScale.x, playerRef.transform.localScale.y);
            }

            // -------------------------------------------------------------------
            //                          Gizmos (Debug)
            // -------------------------------------------------------------------

            void OnDrawGizmosSelected()
            {
                Vector3 c = (zonaCentro != null) ? zonaCentro.position : transform.position;

                // Detección (Azul claro)
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
                // Dibujado desde el centro, suma enemyRadius para reflejar la detección desde el borde
                Gizmos.DrawWireSphere(transform.position, detectionRadius + enemyRadius);

                // Ataque (Rojo)
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, attackRange + enemyRadius);

                // Patrulla (Verde)
                Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
                Gizmos.DrawWireSphere(c, patrolRadius);
            }
        }
    }

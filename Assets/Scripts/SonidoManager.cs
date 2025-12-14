using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoManager : MonoBehaviour
{
    public static SonidoManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip atacadoClip; // Sonido al ser golpeado
    [SerializeField] private AudioClip pasosEnemigoClip; // Sonido de pasos del enemigo
    [SerializeField] private AudioClip sirenaClip; // Sonido de alerta/persecución
    [SerializeField] private AudioClip ruidoExtrañoClip; // Sonido ambiental de fondo
    [SerializeField] private AudioClip mazmorraThemeClip; // Música principal

    // Controla el volumen de los diferentes tipos de audio
    [Header("Volúmenes")]
    [Range(0f, 1f)] public float musicVolume = 0.6f; 
    [Range(0f, 1f)] public float ambientVolume = 0.5f; 
    [Range(0f, 1f)] public float sfxVolume = 1f; 
    [Range(0f, 1f)] public float sirenMaxVolume = 0.9f; 
    [Tooltip("Tiempo de fade para sirena/música (segundos)")]
    public float fadeTime = 0.25f; // Tiempo de transición de volumen

    // Fuentes de audio principales
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sirenSource;
    private AudioSource sfxSource;

    // Diccionario para gestionar los pasos de cada enemigo individualmente
    private readonly Dictionary<GameObject, AudioSource> footstepSources = new Dictionary<GameObject, AudioSource>();

    private Coroutine sirenFadeCoroutine;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        // Gestión del patrón Singleton: destruye duplicados
        if (Instance != null && Instance != this)
        {
            Instance.FusionarAjustesInspector(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializa los AudioSources necesarios
        CrearFuentesSiEsNecesario();

        // Inicia música y ambiente si no están sonando
        if (musicSource.clip != null) ReproducirMusica();
        if (ambientSource.clip != null) ReproducirAmbiente();
    }

    // Copia la configuración de una nueva instancia a la actual sin cortar el audio
    private void FusionarAjustesInspector(SonidoManager other)
    {
        if (other == null) return;

        // Asigna clips si faltan en la instancia actual
        if (other.atacadoClip != null) this.atacadoClip = other.atacadoClip;
        if (other.pasosEnemigoClip != null) this.pasosEnemigoClip = other.pasosEnemigoClip;
        if (other.sirenaClip != null) this.sirenaClip = other.sirenaClip;
        if (other.ruidoExtrañoClip != null) this.ruidoExtrañoClip = other.ruidoExtrañoClip;

        // Si cambia el tema musical, lo actualiza
        if (this.mazmorraThemeClip == null && other.mazmorraThemeClip != null)
        {
            this.mazmorraThemeClip = other.mazmorraThemeClip;
            if (musicSource != null) musicSource.clip = this.mazmorraThemeClip;
        }

        // Copia los valores de volumen y tiempo de transición
        this.musicVolume = other.musicVolume;
        this.ambientVolume = other.ambientVolume;
        this.sfxVolume = other.sfxVolume;
        this.sirenMaxVolume = other.sirenMaxVolume;
        this.fadeTime = other.fadeTime;

        // Actualiza el volumen de las fuentes activas
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(musicSource.volume) * Mathf.Clamp01(musicVolume);
        if (ambientSource != null) ambientSource.volume = ambientVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (sirenSource != null) sirenSource.clip = sirenaClip;
    }

    // Crea los componentes AudioSource hijos si no existen
    private void CrearFuentesSiEsNecesario()
    {
        if (musicSource != null) return;

        musicSource = CrearFuenteHija("MusicSource", true, false, mazmorraThemeClip, musicVolume);
        ambientSource = CrearFuenteHija("AmbientSource", true, false, ruidoExtrañoClip, ambientVolume);
        sirenSource = CrearFuenteHija("SirenSource", true, false, sirenaClip, 0f);
        sfxSource = CrearFuenteHija("SfxSource", false, false, null, sfxVolume);
    }

    // Método auxiliar para instanciar un GameObject con AudioSource
    private AudioSource CrearFuenteHija(string name, bool loop, bool playOnAwake, AudioClip clip, float vol)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = playOnAwake;
        src.clip = clip;
        src.volume = vol;
        src.spatialBlend = 0f; // Sonido 2D
        return src;
    }

    // Música y ambiente 

    public void ReproducirMusica()
    {
        if (musicSource == null || musicSource.clip == null) return;
        if (musicSource.isPlaying) return;

        musicSource.volume = 0f;
        musicSource.Play();
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(RealizarFadeAudio(musicSource, musicVolume, fadeTime));
    }

    public void DetenerMusica(bool fade = true)
    {
        if (musicSource == null) return;
        if (fade)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(RealizarFadeAudio(musicSource, 0f, fadeTime, stopAtEnd: true));
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void ReproducirAmbiente()
    {
        if (ambientSource == null || ambientSource.clip == null) return;
        if (!ambientSource.isPlaying)
        {
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }
    }

    public void DetenerAmbiente()
    {
        if (ambientSource == null) return;
        ambientSource.Stop();
    }

    // SFX one-shot 

    // Reproduce un sonido una sola vez
    public void ReproducirUnDisparoSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    public void ReproducirAtacado() => ReproducirUnDisparoSFX(atacadoClip);
    public void ReproducirRuidoExtraño() => ReproducirUnDisparoSFX(ruidoExtrañoClip);

    // Sirena

    // Inicia el sonido de sirena con fade in
    public void IniciarSirena(float targetVolume = -1f)
    {
        if (sirenSource == null || sirenSource.clip == null) return;
        if (targetVolume < 0f) targetVolume = sirenMaxVolume;
        sirenSource.volume = Mathf.Max(sirenSource.volume, 0f);
        if (!sirenSource.isPlaying) sirenSource.Play();
        if (sirenFadeCoroutine != null) StopCoroutine(sirenFadeCoroutine);
        sirenFadeCoroutine = StartCoroutine(RealizarFadeAudio(sirenSource, targetVolume, fadeTime));
    }

    // Detiene la sirena con fade out
    public void DetenerSirena()
    {
        if (sirenSource == null) return;
        if (sirenFadeCoroutine != null) StopCoroutine(sirenFadeCoroutine);
        sirenFadeCoroutine = StartCoroutine(RealizarFadeAudio(sirenSource, 0f, fadeTime, stopAtEnd: true));
    }

    // Controla el volumen de la sirena manualmente (0 a 1)
    public void EstablecerIntensidadSirena(float normalized)
    {
        if (sirenSource == null) return;
        float vol = Mathf.Clamp01(normalized) * sirenMaxVolume;
        if (!sirenSource.isPlaying && vol > 0f) sirenSource.Play();
        sirenSource.volume = vol;
        if (vol == 0f && sirenSource.isPlaying) sirenSource.Stop();
    }

    // Pasos de enemigo (loop por enemigo)

    // Asigna y reproduce sonido de pasos en un enemigo específico
    public void ReproducirPasosEnemigo(GameObject enemy)
    {
        if (enemy == null || pasosEnemigoClip == null) return;
        if (!footstepSources.TryGetValue(enemy, out var src) || src == null)
        {
            src = enemy.AddComponent<AudioSource>();
            src.clip = pasosEnemigoClip;
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0.7f * sfxVolume;
            footstepSources[enemy] = src;
        }

        if (!src.isPlaying) src.Play();
    }

    public void DetenerPasosEnemigo(GameObject enemy)
    {
        if (enemy == null) return;
        if (footstepSources.TryGetValue(enemy, out var src) && src != null)
        {
            src.Stop();
        }
    }

    // Elimina el AudioSource cuando un enemigo muere
    public void DesregistrarEnemigo(GameObject enemy)
    {
        if (enemy == null) return;
        if (footstepSources.TryGetValue(enemy, out var src) && src != null)
        {
            src.Stop();
            Destroy(src);
        }
        footstepSources.Remove(enemy);
    }


    // Utilidades 

    // Corrutina para subir o bajar el volumen progresivamente
    private IEnumerator RealizarFadeAudio(AudioSource src, float targetVolume, float duration, bool stopAtEnd = false)
    {
        if (src == null) yield break;
        float start = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(start, targetVolume, duration <= 0f ? 1f : t / duration);
            yield return null;
        }
        src.volume = targetVolume;
        if (stopAtEnd && Mathf.Approximately(targetVolume, 0f))
        {
            src.Stop();
        }
    }
}
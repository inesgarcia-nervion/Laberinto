using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoManager : MonoBehaviour
{
    public static SonidoManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip atacadoClip;
    [SerializeField] private AudioClip pasosEnemigoClip;
    [SerializeField] private AudioClip sirenaClip;
    [SerializeField] private AudioClip ruidoExtrañoClip;
    [SerializeField] private AudioClip mazmorraThemeClip;

    [Header("Volúmenes")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float sirenMaxVolume = 0.9f;
    [Tooltip("Tiempo de fade para sirena/música (segundos)")]
    public float fadeTime = 0.25f;

    // Fuentes principales
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sirenSource;
    private AudioSource sfxSource;

    // Footsteps por enemigo (AudioSource creado en el GameObject del enemigo)
    private readonly Dictionary<GameObject, AudioSource> footstepSources = new Dictionary<GameObject, AudioSource>();

    private Coroutine sirenFadeCoroutine;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        // Si ya existe un singleton distinto, transferimos ajustes útiles y destruimos esta instancia.
        if (Instance != null && Instance != this)
        {
            Instance.MergeInspectorSettings(this);
            Destroy(gameObject);
            return;
        }

        // Primera (y única) instancia
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear AudioSources base sólo una vez
        CreateAudioSourcesIfNeeded();

        // Si la música no está sonando, arrancarla (no reiniciará si ya está sonando)
        if (musicSource.clip != null) PlayMusic();
        if (ambientSource.clip != null) PlayAmbient();
    }

    // Si una nueva instancia aparece en otra escena, copiamos los clips/volúmenes que el diseñador haya configurado
    // sin reiniciar la reproducción actual.
    private void MergeInspectorSettings(SonidoManager other)
    {
        if (other == null) return;

        // Copiar clips si el singleton no los tiene (o si quieres forzar, cambia la lógica)
        if (other.atacadoClip != null) this.atacadoClip = other.atacadoClip;
        if (other.pasosEnemigoClip != null) this.pasosEnemigoClip = other.pasosEnemigoClip;
        if (other.sirenaClip != null) this.sirenaClip = other.sirenaClip;
        if (other.ruidoExtrañoClip != null) this.ruidoExtrañoClip = other.ruidoExtrañoClip;

        // Si el singleton no tiene tema asignado pero el nuevo sí, asignarlo (sin tocar reproducción si ya suena)
        if (this.mazmorraThemeClip == null && other.mazmorraThemeClip != null)
        {
            this.mazmorraThemeClip = other.mazmorraThemeClip;
            if (musicSource != null) musicSource.clip = this.mazmorraThemeClip;
        }

        // Copiar volúmenes/ajustes menores
        this.musicVolume = other.musicVolume;
        this.ambientVolume = other.ambientVolume;
        this.sfxVolume = other.sfxVolume;
        this.sirenMaxVolume = other.sirenMaxVolume;
        this.fadeTime = other.fadeTime;

        // Actualizar volúmenes de fuentes ya existentes (sin pausar la música)
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(musicSource.volume) * Mathf.Clamp01(musicVolume);
        if (ambientSource != null) ambientSource.volume = ambientVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (sirenSource != null) sirenSource.clip = sirenaClip;
    }

    private void CreateAudioSourcesIfNeeded()
    {
        // Evitar recrear si ya existen (por ejemplo tras pasar por Awake anterior)
        if (musicSource != null) return;

        musicSource = CreateChildAudioSource("MusicSource", true, false, mazmorraThemeClip, musicVolume);
        ambientSource = CreateChildAudioSource("AmbientSource", true, false, ruidoExtrañoClip, ambientVolume);
        sirenSource = CreateChildAudioSource("SirenSource", true, false, sirenaClip, 0f);
        sfxSource = CreateChildAudioSource("SfxSource", false, false, null, sfxVolume);
    }

    private AudioSource CreateChildAudioSource(string name, bool loop, bool playOnAwake, AudioClip clip, float vol)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = playOnAwake;
        src.clip = clip;
        src.volume = vol;
        src.spatialBlend = 0f; // 2D
        return src;
    }

    // ---------- Música y ambiente ----------
    public void PlayMusic()
    {
        if (musicSource == null || musicSource.clip == null) return;
        // Si ya está sonando, no reiniciar
        if (musicSource.isPlaying) return;

        musicSource.volume = 0f;
        musicSource.Play();
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeAudio(musicSource, musicVolume, fadeTime));
    }

    public void StopMusic(bool fade = true)
    {
        if (musicSource == null) return;
        if (fade)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(FadeAudio(musicSource, 0f, fadeTime, stopAtEnd: true));
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void PlayAmbient()
    {
        if (ambientSource == null || ambientSource.clip == null) return;
        if (!ambientSource.isPlaying)
        {
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }
    }

    public void StopAmbient()
    {
        if (ambientSource == null) return;
        ambientSource.Stop();
    }

    // ---------- SFX one-shot ----------
    public void PlayOneShotAtSfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // Conveniencia por nombre (usa los clips asignados en el Inspector)
    public void PlayAtacado() => PlayOneShotAtSfx(atacadoClip);
    public void PlayRuidoExtraño() => PlayOneShotAtSfx(ruidoExtrañoClip);

    // ---------- Sirena ----------
    public void StartSiren(float targetVolume = -1f)
    {
        if (sirenSource == null || sirenSource.clip == null) return;
        if (targetVolume < 0f) targetVolume = sirenMaxVolume;
        sirenSource.volume = Mathf.Max(sirenSource.volume, 0f);
        if (!sirenSource.isPlaying) sirenSource.Play();
        if (sirenFadeCoroutine != null) StopCoroutine(sirenFadeCoroutine);
        sirenFadeCoroutine = StartCoroutine(FadeAudio(sirenSource, targetVolume, fadeTime));
    }

    public void StopSiren()
    {
        if (sirenSource == null) return;
        if (sirenFadeCoroutine != null) StopCoroutine(sirenFadeCoroutine);
        sirenFadeCoroutine = StartCoroutine(FadeAudio(sirenSource, 0f, fadeTime, stopAtEnd: true));
    }

    // Ajusta intensidad 0..1 (volumen relativo a sirenMaxVolume)
    public void SetSirenIntensity(float normalized)
    {
        if (sirenSource == null) return;
        float vol = Mathf.Clamp01(normalized) * sirenMaxVolume;
        if (!sirenSource.isPlaying && vol > 0f) sirenSource.Play();
        sirenSource.volume = vol;
        if (vol == 0f && sirenSource.isPlaying) sirenSource.Stop();
    }

    // ---------- Pasos de enemigo (loop por enemigo) ----------
    // Crea/activa una fuente de audio en el enemigo para reproducir pasos en loop
    public void PlayEnemyFootsteps(GameObject enemy)
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

    public void StopEnemyFootsteps(GameObject enemy)
    {
        if (enemy == null) return;
        if (footstepSources.TryGetValue(enemy, out var src) && src != null)
        {
            src.Stop();
        }
    }

    // Limpia referencias cuando un enemigo se destruye
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (footstepSources.TryGetValue(enemy, out var src) && src != null)
        {
            src.Stop();
            Destroy(src);
        }
        footstepSources.Remove(enemy);
    }

    // ---------- Utilidades ----------
    private IEnumerator FadeAudio(AudioSource src, float targetVolume, float duration, bool stopAtEnd = false)
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

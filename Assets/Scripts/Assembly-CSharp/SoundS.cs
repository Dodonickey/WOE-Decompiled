using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class SoundS
{
    private static bool m_mute;

    public static GameObject m_mainCamera;

    public static DynamicArray<SoundC> m_components;

    private static GameObject m_listenerGO;

    public static void Initialize(GameObject _mainCamera)
    {
        FMODManager.Initialize();
        m_components = new DynamicArray<SoundC>();
        m_mainCamera = _mainCamera;
        SetListener(m_mainCamera.gameObject);
    }

    // Replaces FMOD_Listener.m_listener. Do NOT leave FMOD_Listener or a StudioListener
    // on the camera, or they will fight over listener 0. Position/orientation/velocity
    // are pushed from Update() below.
    public static void SetListener(GameObject _go)
    {
        m_listenerGO = _go;
        Debug.Log("Listener set to: " + _go.transform.name);
    }

    public static void PlaySingleShot(string _event, Vector3 _pos, float _volume = 1f)
    {
        FMODManager.PlayOneShot("event:" + _event, _pos, _volume);
    }

    public static void PlaySingleShotWithParameter(string _event, Vector3 _pos, string _parameter, float _value, float _volume = 1f)
    {
        FMODManager.PlayOneShotWithParameter("event:" + _event, _pos, _parameter, _value, _volume);
    }

    public static void PlaySound(SoundC _c)
    {
        if (!_c.isPlaying)
        {
            FMODManager.ERRCHECK(_c.eventInstance.start());
            _c.isPlaying = true;
        }
        else
        {
            FMODManager.ERRCHECK(_c.eventInstance.setTimelinePosition(0));
        }
    }

    public static void StopSound(SoundC _c)
    {
        if (_c.isPlaying)
        {
            // Old parameterless stop() == STOP_MODE.IMMEDIATE
            FMODManager.ERRCHECK(_c.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
            _c.isPlaying = false;
        }
    }

    public static void PauseSound(SoundC _c)
    {
        if (!_c.isPaused && _c.isPlaying)
        {
            FMODManager.ERRCHECK(_c.eventInstance.setPaused(true));
            _c.isPaused = true;
        }
    }

    public static void ResumeSound(SoundC _c)
    {
        if (_c.isPaused && _c.isPlaying)
        {
            FMODManager.ERRCHECK(_c.eventInstance.setPaused(false));
            _c.isPaused = false;
        }
    }

    public static void SetVolume(SoundC _c, float _volume)
    {
        FMODManager.ERRCHECK(_c.eventInstance.setVolume(_volume));
    }

    public static void SetSoundParameter(SoundC _c, string _parameterName, float _value)
    {
        FMODManager.SetParameter(_c.eventInstance, _parameterName, _value);
    }

    private static void UpdatePosition(SoundC _c, Vector3 _pos)
    {
        var attributes = RuntimeUtils.To3DAttributes(_pos);
        FMODManager.ERRCHECK(_c.eventInstance.set3DAttributes(attributes));
    }

    public static void SetPauseForAllSounds(bool _pause)
    {
        int aliveCount = m_components.m_aliveCount;
        for (int i = 0; i < aliveCount; i++)
        {
            int num = m_components.m_aliveIndices[i];
            SoundC c = m_components.m_array[num];
            if (_pause)
            {
                PauseSound(c);
            }
            else
            {
                ResumeSound(c);
            }
        }
    }

    public static void RemoveAllSounds()
    {
        while (m_components.m_aliveCount > 0)
        {
            SoundC c = m_components.m_array[m_components.m_aliveIndices[0]];
            RemoveComponent(c);
        }
    }

    public static void SetMute(bool _on)
    {
        if (_on != m_mute)
        {
            FMODManager.MixerSetPause(_on);
            m_mute = _on;
        }
    }

    public static void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FMODManager.Update(); // applies a pending mute/pause once the banks have finished loading
#endif

        // Replaces FMOD_Listener.Update (listener attributes incl. rigidbody velocity)
        RuntimeManager.SetListenerLocation(m_listenerGO, m_listenerGO.GetComponent<Rigidbody>());

        int aliveCount = m_components.m_aliveCount;
        for (int i = 0; i < aliveCount; i++)
        {
            int num = m_components.m_aliveIndices[i];
            SoundC soundC = m_components.m_array[num];
            TransformC p_TC = soundC.p_TC;
            if (soundC.forceAtListenerPosition)
            {
                UpdatePosition(soundC, m_listenerGO.transform.position);
            }
            else if (p_TC.updatedPosition)
            {
                UpdatePosition(soundC, p_TC.transform.position);
            }
        }
    }

    public static SoundC AddComponent(TransformC _parentTC, string _event, float _volume = 1f, bool _forceAtListenersPosition = false)
    {
        SoundC soundC = m_components.AddItem();
        soundC.eventInstance = FMODManager.NewEvent("event:" + _event, _parentTC.transform.position, false, _volume);
        soundC.p_TC = _parentTC;
        soundC.forceAtListenerPosition = _forceAtListenersPosition;
        EntityManager.AddComponentToEntity(_parentTC.p_entity, soundC);
        return soundC;
    }

    public static void RemoveComponent(SoundC _c)
    {
        if (_c.p_entity == null)
        {
            Debug.LogWarning("Trying to remove component that has already been removed");
            return;
        }
        if (_c.eventInstance.isValid())
        {
            StopSound(_c);
            FMODManager.ReleaseEvent(_c.eventInstance);
            _c.eventInstance = default(EventInstance);
        }
        EntityManager.RemoveComponentFromEntity(_c);
        m_components.RemoveItem(_c);
    }
}
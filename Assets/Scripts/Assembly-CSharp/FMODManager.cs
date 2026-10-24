using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class FMODManager
{
    public static FMOD.Studio.System m_system;

    public static Bus m_mixer;

    // Old getEvent() used LOADING_MODE.BEGIN_NOW (started loading sample data on first lookup).
    // 1.10 has no such parameter, so we do it once per event path ourselves.
    private static readonly HashSet<string> s_sampleLoaded = new HashSet<string>();

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL downloads the banks asynchronously, so "bus:/" does not exist yet when Initialize() runs
    // (getBus fails with ERR_EVENT_NOTFOUND). Mute/pause requests made before the mixer is available
    // are remembered and applied from Update() once the banks have finished loading.
    private static bool s_pausePending;
    private static bool s_pauseValue;
#endif

    public static void Initialize()
    {
        // RuntimeManager creates/initializes itself (and loads banks from FMOD Settings) on first access
        m_system = RuntimeManager.StudioSystem;
#if UNITY_WEBGL && !UNITY_EDITOR
        TryGetMixer(); // usually not available yet; retried from Update()
#else
        ERRCHECK(m_system.getBus("bus:/", out m_mixer));
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // Call once per frame (SoundS.Update does this).
    public static void Update()
    {
        if (s_pausePending && TryGetMixer())
        {
            s_pausePending = false;
            ERRCHECK(m_mixer.setPaused(s_pauseValue));
        }
    }

    // Returns true once the master bus exists. Stays silent while the banks are still loading.
    private static bool TryGetMixer()
    {
        if (m_mixer.isValid())
        {
            return true;
        }
        if (!m_system.isValid())
        {
            return false;
        }
        return m_system.getBus("bus:/", out m_mixer) == FMOD.RESULT.OK;
    }
#endif

    private static EventInstance CreateInstance(string _event)
    {
        if (s_sampleLoaded.Add(_event))
        {
            ERRCHECK(RuntimeManager.GetEventDescription(_event).loadSampleData());
        }
        return RuntimeManager.CreateInstance(_event);
    }

    public static void PlayOneShot(string _event, Vector3 _position, float _volume = 1f)
    {
        EventInstance eventInstance = CreateInstance(_event);
        var attributes = RuntimeUtils.To3DAttributes(_position);
        ERRCHECK(eventInstance.set3DAttributes(attributes));
        ERRCHECK(eventInstance.setVolume(_volume));
        ERRCHECK(eventInstance.start());
        ERRCHECK(eventInstance.release());
    }

    public static void PlayOneShotWithParameter(string _event, Vector3 _position, string _parameter, float _value, float _volume = 1f)
    {
        EventInstance eventInstance = CreateInstance(_event);
        SetParameter(eventInstance, _parameter, _value);
        var attributes = RuntimeUtils.To3DAttributes(_position);
        ERRCHECK(eventInstance.set3DAttributes(attributes));
        ERRCHECK(eventInstance.setVolume(_volume));
        ERRCHECK(eventInstance.start());
        ERRCHECK(eventInstance.release());
    }

    public static EventInstance NewEvent(string _event, Vector3 _position, bool _startImmediately = true, float _volume = 1f)
    {
        EventInstance eventInstance = CreateInstance(_event);
        var attributes = RuntimeUtils.To3DAttributes(_position);
        ERRCHECK(eventInstance.set3DAttributes(attributes));
        ERRCHECK(eventInstance.setVolume(_volume));
        if (_startImmediately)
        {
            ERRCHECK(eventInstance.start());
        }
        return eventInstance;
    }

    public static void ReleaseEvent(EventInstance _eventInstance)
    {
        // isValid() guard: SoundS.RemoveComponent stops then releases, so this avoids
        // error spam on already-invalid handles. Remove for exact parity with the old code.
        if (_eventInstance.isValid())
        {
            // Old parameterless stop() == STOP_MODE.IMMEDIATE
            ERRCHECK(_eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
            ERRCHECK(_eventInstance.release());
        }
    }

    public static void SetParameter(EventInstance _eventInstance, string _parameterName, float _value)
    {
        ERRCHECK(_eventInstance.setParameterValue(_parameterName, _value));
    }

    public static void MixerSetPause(bool _pause)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (TryGetMixer())
        {
            s_pausePending = false;
            ERRCHECK(m_mixer.setPaused(_pause));
        }
        else
        {
            // Banks still loading: remember the request, Update() applies it once "bus:/" exists.
            s_pauseValue = _pause;
            s_pausePending = true;
        }
#else
        ERRCHECK(m_mixer.setPaused(_pause));
#endif
    }

    public static void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError("FMOD Error (" + result.ToString() + "): " + FMOD.Error.String(result));
        }
    }
}
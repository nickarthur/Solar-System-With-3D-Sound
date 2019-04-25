//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Q3DAudioSource))]
[CanEditMultipleObjects]
public class Q3DAudioSourceEditor : Editor
{
    [MenuItem("GameObject/Audio/Q3DAudioSource")]
    static void CreateQ3DAudioSource()
    {
        string Q3DAudioSource = "Q3DAudioSource";
        GameObject go = new GameObject(Q3DAudioSource);
        Undo.RegisterCreatedObjectUndo(go, "Created " + Q3DAudioSource);//this must be done before adding the component for undo to work!
        go.AddComponent<AudioSource>();
        go.AddComponent<Q3DAudioSource>();
    }
    [MenuItem("GameObject/Audio/Q3DTools/AudioSourcesToQ3DAudioSources")]
    static void AudioSourcesToQ3DAudioRooms()
    {
        string Q3DAudioSource = "Q3DAudioSource";
        AudioSource[] audioSources = Q3DAudioManager.FindObjectsOfTypeAllWrapper<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.GetComponent<Q3DAudioSource>())
            {
                Undo.RegisterCreatedObjectUndo(audioSource, "Created " + Q3DAudioSource);//this must be done before adding the component for undo to work!
                audioSource.gameObject.AddComponent<Q3DAudioSource>();
            }
        }
    }

    SerializedProperty
        mGainSerializedProperty,
        mDistanceRolloffOverrideAudioSourcesRangeSerializedProperty,
        mDistanceRolloffMinSerializedProperty,
        mDistanceRolloffMaxSerializedProperty,
        mDistanceRolloffModelSerializedProperty;
    void OnEnable()
    {
        mGainSerializedProperty = serializedObject.FindProperty("mGain");
        mDistanceRolloffOverrideAudioSourcesRangeSerializedProperty = serializedObject.FindProperty("mDistanceRolloffOverrideAudioSourcesRange");
        mDistanceRolloffMinSerializedProperty = serializedObject.FindProperty("mDistanceRolloffMin");
        mDistanceRolloffMaxSerializedProperty = serializedObject.FindProperty("mDistanceRolloffMax");
        mDistanceRolloffModelSerializedProperty = serializedObject.FindProperty("mDistanceRolloffModel");
    }

    private void DistanceRolloffRangeValuesGUI(Q3DAudioSource q3dAudioSource)
    {
        mDistanceRolloffOverrideAudioSourcesRangeSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Distance Rolloff Range Override AudioSource",
                "If true, then the Q3DAudioSource will have its own distance rolloff min/max distances; otherwise it will inherit its underlying AudioSource's values"
            ),
            q3dAudioSource.DistanceRolloffOverrideAudioSourcesRange);

        if (!q3dAudioSource.DistanceRolloffOverrideAudioSourcesRange)
        {
            GUI.enabled = false;
        }

        float mDistanceRolloffMinSerializedPropertyFloatValue = EditorGUILayout.FloatField(
            new GUIContent
            (
                "Distance Rolloff Minimum",
                "If the listener is this far away (in meters) from the sound then distance rolloff will not attenuate the sound"//see #Q3DAudioSourceInSync
            ),
            q3dAudioSource.DistanceRolloffOverrideAudioSourcesRange ? q3dAudioSource.DistanceRolloffMin : q3dAudioSource.mAudioSource.minDistance);

        float mDistanceRolloffMaxSerializedPropertyFloatValue = EditorGUILayout.FloatField(
            new GUIContent
            (
                "Distance Rolloff Maximum",
                "If the listener is this far away (in meters) from the sound then distance rolloff will attenuate the sound to silence"//see #Q3DAudioSourceInSync
            ),
            q3dAudioSource.DistanceRolloffOverrideAudioSourcesRange ? q3dAudioSource.DistanceRolloffMax : q3dAudioSource.mAudioSource.maxDistance);

        //duplicate range enforcement logic, since Unity's serialization code can only reference raw datafields, not C# get/set properties
        Q3DAudioSource.DistanceRolloffEnforceRange(
            ref mDistanceRolloffMinSerializedPropertyFloatValue,
            ref mDistanceRolloffMaxSerializedPropertyFloatValue);
        mDistanceRolloffMinSerializedProperty.floatValue = mDistanceRolloffMinSerializedPropertyFloatValue;
        mDistanceRolloffMaxSerializedProperty.floatValue = mDistanceRolloffMaxSerializedPropertyFloatValue;

        if (!q3dAudioSource.DistanceRolloffOverrideAudioSourcesRange)
        {
            GUI.enabled = true;
        }
    }

    public override void OnInspectorGUI()
    {
        Q3DAudioSource q3dAudioSource = (Q3DAudioSource)target;
        q3dAudioSource.InitializeAudioSource();//maintain invariant of Q3DAudioSource accessing its underlying AudioSource
        AudioSource audioSource = q3dAudioSource.mAudioSource;

        int numChannels = (audioSource && audioSource.clip) ? audioSource.clip.channels : 0;
        bool isObjectSound = Q3DAudioSource.IsSoundObject(numChannels);
        bool isSoundfield = Q3DAudioSource.IsFirstOrSecondOrderAmbisonic(numChannels);

        Q3DAudioSource[] q3dAudioSources = q3dAudioSource.GetComponents<Q3DAudioSource>();
        AudioSource[] audioSources = q3dAudioSource.GetComponents<AudioSource>();
        Q3DAudioSource.LogIfQ3DAudioSourceNumIsNotCorrect(q3dAudioSources.Length, audioSources.Length, q3dAudioSource.gameObject.name);

        if (audioSource)
        {
            if (audioSource)
            {
                GUI.enabled = false;
                string takenFromUnderlyingAudioSource = "Taken from underlying AudioSource";
                {
                    EditorGUILayout.TextField(
                        new GUIContent
                        (
                            "AudioClip",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.clip ? audioSource.clip.name : "None"
                    );
                    EditorGUILayout.Toggle(
                        new GUIContent
                        (
                            "Mute",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.mute
                    );

                    if(isObjectSound)
                    {
                        EditorGUILayout.Toggle(
                            new GUIContent
                            (
                                "Spatialize Post Effects",
                                takenFromUnderlyingAudioSource
                            ),
                            audioSource.spatializePostEffects
                        );
                        EditorGUILayout.Toggle(
                            new GUIContent
                            (
                                "Bypass Effects",
                                takenFromUnderlyingAudioSource
                            ),
                            audioSource.bypassEffects
                        );
                    }

                    EditorGUILayout.Toggle(
                        new GUIContent
                        (
                            "Play On Awake",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.playOnAwake
                    );
                    EditorGUILayout.Toggle(
                        new GUIContent
                        (
                            "Loop",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.loop
                    );
                    EditorGUILayout.IntField(
                        new GUIContent
                        (
                            "Priority",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.priority
                    );
                    EditorGUILayout.FloatField(
                        new GUIContent
                        (
                            "Volume",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.volume
                    );
                    EditorGUILayout.FloatField(
                        new GUIContent
                        (
                            "Pitch",
                            takenFromUnderlyingAudioSource
                        ),
                        audioSource.pitch
                    );
                }
                GUI.enabled = true;
            }
            if (isObjectSound || isSoundfield)
            { 
                /*  #Q3DAudioSourceInSync: keep GUIContent.tooltip text in sync with the description strings in 
                 *  Q3DAudioPlugin.cpp:InternalRegisterEffectDefinitionSoundShared() */
                mGainSerializedProperty.floatValue = EditorGUILayout.Slider(new GUIContent
                    (
                        "Gain",
                        "Linear volume attenuation; 0 means silence"//see #Q3DAudioSourceQ3DAudioSourceInSync
                    ),
                    mGainSerializedProperty.floatValue,
                    Q3DAudioSource.GainMin,
                    Q3DAudioSource.GainMax);

                if (isSoundfield)
                {
                    EditorGUILayout.LabelField("SoundField (First Order Ambisonics)", "4 channels found");
                }
                else if (isObjectSound)
                {
                    DistanceRolloffRangeValuesGUI(q3dAudioSource);

                    GUIContent[] distanceRolloffOptions = new GUIContent[]
                    {
                        //keep in sync with Q3DAudioSource.vr_audio_distance_rolloff_model
                        new GUIContent("Logarithmic",   "distance attenuation follows a curve that loses roughly 80% of the sound's volume when " + 
                                                        "distanceFromSound is roughly 15% of the way from minDistance to maxDistance, and loses " + 
                                                        "roughly 90% of the sound's volume when distanceFromSound is roughly 28% of the way from " + 
                                                        "minDistance to maxDistance, and gradually loses the rest of the sound's remaining 10% of " +
                                                        "volume as distanceFromSound approaches maxDistance"),
                        new GUIContent("Linear", "distanceAttenuation = 1 - (distanceFromSound - min)/(max-min)"),
                        new GUIContent("None", "Distance rolloff never attenuates this sound")
                    };
                    mDistanceRolloffModelSerializedProperty.intValue =
                        EditorGUILayout.Popup(
                            new GUIContent
                            (
                                "Distance Rolloff Interpolation",
                                "If the listener's distance from the sound is between the maximum and minimum limits, then distance rolloff will attenuate the sound according to this function"//see #Q3DAudioSourceInSync
                            ),
                            (int)q3dAudioSource.DistanceRolloffModel,
                            distanceRolloffOptions
                    );

                    EditorGUILayout.LabelField("SoundObject", "1 channel found");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Disabled", "AudioSource's clip does not have 1, 4 or 9 channels, so it can't be for a sound object, first-order-ambisonics soundfield or second-order-ambisonics soundfield!");
            }

            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.LabelField("Disabled", "Must be placed on a GameObject that has an AudioSource, but no AudioSource is found");
        }
    }
}

#endif//#if UNITY_EDITOR
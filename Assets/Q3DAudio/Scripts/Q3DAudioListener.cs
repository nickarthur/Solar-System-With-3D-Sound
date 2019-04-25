//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;

//BEG_DEBUG_VS_RELEASE_PROTOTYPE
//using UnityEditor;
//END_DEBUG_VS_RELEASE_PROTOTYPE

using System.Collections;
using System.Runtime.InteropServices;


//see #QualcommAudioMenu
[AddComponentMenu("QualcommAudio/Q3DAudioListener")]
public class Q3DAudioListener : MonoBehaviour
{
    public static ArrayList mQ3DAudioRoomList = new ArrayList();   
    private bool InEditor()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif//#if UNITY_EDITOR
    }

    static private Q3DAudioManager smQ3DAudioManagerInstance;
    static public Q3DAudioManager Q3DAudioManagerInstance
    {
        get { return smQ3DAudioManagerInstance; }
    }
    void Awake()
    {
        if (Q3DAudioManager.DisableIfQ3DNotActive(this))
        {
            return;
        }

        //create singleton if necessary -- do this as early as possible so logging mechanism on Windows can be setup
        if (!smQ3DAudioManagerInstance)
        {
            GameObject go = new GameObject("Q3DAudioManager");
            smQ3DAudioManagerInstance = go.AddComponent<Q3DAudioManager>();
        }

        GetComponent<AudioListener>().enabled = enabled;
    }

    static float Square(float f) { return f * f; }
    void Update()
    {
        //process reverb zones
        Q3DAudioManager.Q3DAudioRoomSettings defaultSettings = smQ3DAudioManagerInstance.mQ3DAudioRoomSettingsDefault;
        Q3DAudioManager.Q3DAudioRoomSettings currentSettings = defaultSettings;
        AudioListener audioListener = GetComponent<AudioListener>();
        foreach (Q3DAudioRoom q3dAudioRoom in mQ3DAudioRoomList)
        {
            if(!q3dAudioRoom.isActiveAndEnabled)
            {
                continue;
            }

            float listenerToReverbZoneDistanceSquared =
                (q3dAudioRoom.mAudioReverbZone.transform.position - audioListener.transform.position).sqrMagnitude;

            if (listenerToReverbZoneDistanceSquared < Square(q3dAudioRoom.mAudioReverbZone.maxDistance))
            {
                //integral values are immediately set upon entering a reverb zone
                currentSettings.roomMaterialWalls = q3dAudioRoom.RoomMaterialWalls;
                currentSettings.roomMaterialCeiling = q3dAudioRoom.RoomMaterialCeiling;
                currentSettings.roomMaterialFloor = q3dAudioRoom.RoomMaterialFloor;

                //floating-point values are linearly scaled from default values to Q3DAudioRoom values as listener approaches minDistance
                if (listenerToReverbZoneDistanceSquared <= Square(q3dAudioRoom.mAudioReverbZone.minDistance))
                {
                    currentSettings.gainAdjust = q3dAudioRoom.Gain;
                    currentSettings.brightAdjust = q3dAudioRoom.BrightAdjust;
                    currentSettings.timeAdjust = q3dAudioRoom.TimeAdjust;
                    currentSettings.wetMix = q3dAudioRoom.WetMix;
                    currentSettings.roomDimensionsX_meters = q3dAudioRoom.RoomDimensionsX_meters;
                    currentSettings.roomDimensionsY_meters = q3dAudioRoom.RoomDimensionsY_meters;
                    currentSettings.roomDimensionsZ_meters = q3dAudioRoom.RoomDimensionsZ_meters;
                }
                else
                {
                    float listenerToReverbZoneDistance = Mathf.Sqrt(listenerToReverbZoneDistanceSquared);
                    float barycentricScalar =
                        1.0f -  (listenerToReverbZoneDistance - q3dAudioRoom.mAudioReverbZone.minDistance) /
                                (q3dAudioRoom.mAudioReverbZone.maxDistance - q3dAudioRoom.mAudioReverbZone.minDistance);
                    float oneMinusBarycentricScalar = 1.0f - barycentricScalar;

                    currentSettings.gainAdjust = q3dAudioRoom.Gain * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.gainAdjust;
                    currentSettings.brightAdjust = q3dAudioRoom.BrightAdjust * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.brightAdjust;
                    currentSettings.timeAdjust = q3dAudioRoom.TimeAdjust * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.timeAdjust;
                    currentSettings.wetMix = 
                        (int)(q3dAudioRoom.WetMix * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.wetMix + 0.5f);
                    currentSettings.roomDimensionsX_meters = q3dAudioRoom.RoomDimensionsX_meters * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.roomDimensionsX_meters;
                    currentSettings.roomDimensionsY_meters = q3dAudioRoom.RoomDimensionsY_meters * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.roomDimensionsY_meters; ;
                    currentSettings.roomDimensionsZ_meters = q3dAudioRoom.RoomDimensionsZ_meters * barycentricScalar + oneMinusBarycentricScalar * defaultSettings.roomDimensionsZ_meters; ;
                }
                break;//no support for overlapping reverb zones
            }
        }

        smQ3DAudioManagerInstance.SetAudioRoomSettings(currentSettings);
    }
}
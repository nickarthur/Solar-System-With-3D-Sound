//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

using System.Runtime.InteropServices;
using System;
using UnityEngine;

//see #QualcommAudioMenu
[AddComponentMenu("QualcommAudio/Q3DAudioRoom")]
public class Q3DAudioRoom : MonoBehaviour
{
    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void SetReverb(
        float gain,
        float brightAdjust,
        float timeAdjust,
        int wetMix,
        float roomDimensionsX_meters,
        float roomDimensionsY_meters,
        float roomDimensionsZ_meters,
        /*vr_audio_room_material_type*/int roomMaterialWalls,
        /*vr_audio_room_material_type*/int roomMaterialCeiling,
        /*vr_audio_room_material_type*/int roomMaterialFloor);


    //keep in sync with unnamed C++ enum in Q3DAudioPlugin.cpp
    private enum ReverbParameter
    {
        gain,
        timeAdjust,
        brightnessAdjust,
        wetMix,
        roomDimensionX,
        roomDimensionY,
        roomDimensionZ,
        roomMaterialX,
        roomMaterialY,
        roomMaterialZ
    }

    ///keep in sync with C++ enum of the same name defined in public/common.h of the q3d_audio SAM code
    public enum vr_audio_room_material_type
    {
        VR_AUDIO_MATERIAL_TRANSPARENT = 0,              /**< Acoustically transparent material, reflects no sound. */
        VR_AUDIO_MATERIAL_ACOUSTIC_CEILING_TILES,       /**< Acoustic ceiling tiles, absorbs most frequencies.*/
        VR_AUDIO_MATERIAL_BRICK_BARE,                   /**< Bare brick, relatively reflective. */
        VR_AUDIO_MATERIAL_BRICK_PAINTED,                /**< Painted brick */
        VR_AUDIO_MATERIAL_CONCRETE_BLOCK_COARSE,        /**< Coarse surface concrete block. */
        VR_AUDIO_MATERIAL_CONCRETE_BLOCK_PAINTED,       /**< Painted concrete block. */
        VR_AUDIO_MATERIAL_CURTAIN_HEAVY,                /**< Heavy curtains. */
        VR_AUDIO_MATERIAL_FIBER_GLASS_INSULATION,       /**< Fiber glass insulation. */
        VR_AUDIO_MATERIAL_GLASS_THICK,                  /**< Thin glass. */
        VR_AUDIO_MATERIAL_GLASS_THIN,                   /**< Thick glass. */
        VR_AUDIO_MATERIAL_GRASS,                        /**< Grass. */
        VR_AUDIO_MATERIAL_LINOLEUM_ON_CONCRETE,         /**< Linoleum on concrete. */
        VR_AUDIO_MATERIAL_MARBLE,                       /**< Marble. */
        VR_AUDIO_MATERIAL_METAL,                        /**< Galvanized sheet metal. */
        VR_AUDIO_MATERIAL_PARQUET_ON_CONCRETE,        /**< Wooden parquet on concrete. */
        VR_AUDIO_MATERIAL_PLASTER_ROUGH,                  /**< Rough plaster surface. */
        VR_AUDIO_MATERIAL_PLASTER_SMOOTH,                 /**< Smooth plaster surface. */
        VR_AUDIO_MATERIAL_PLYWOOD_PANEL,                  /**< Plywood panel. */
        VR_AUDIO_MATERIAL_POLISHED_CONCRETE_OR_TILE,      /**< Polished concrete OR tile surface. */
        VR_AUDIO_MATERIAL_SHEET_ROCK,                     /**< Sheet rock. */
        VR_AUDIO_MATERIAL_WATER_OR_ICE_SURFACE,       /**< Surface of water or ice. */
        VR_AUDIO_MATERIAL_WOOD_ON_CEILING,            /**< Wooden ceiling. */
        VR_AUDIO_MATERIAL_WOOD_PANEL					  /**< Wood paneling. */
    };

    public AudioReverbZone mAudioReverbZone = null;

    private void DisableBecauseUninitialized()
    {
        enabled = false;
        if (!mAudioReverbZone)
        {
            Q3DAudioManager.DebugLog("Q3DAudioRoom found no AudioReverbZone on its GameObject -- disabling.");
        }
    }

    void Awake()
    {
        if (Q3DAudioManager.DisableIfQ3DNotActive(this))
        {
            return;
        }

        mAudioReverbZone = GetComponent<AudioReverbZone>();
        if (!mAudioReverbZone)
        {
            DisableBecauseUninitialized();
        }
        else
        {
            Q3DAudioListener.mQ3DAudioRoomList.Add(this);
        }
    }

    private void OnDestroy()
    {
        Q3DAudioListener.mQ3DAudioRoomList.Remove(this);
    }

    private void OnEnable()
    {
        if(!mAudioReverbZone)
        {
            DisableBecauseUninitialized();
        }
    }

    //#Q3DAudioRoomInSync: keep minimum and maximum values in sync with Q3DAudioPlugin.cpp:InternalRegisterEffectDefinitionReverb()
    public const float GainMin = 0.0f;
    public const float GainMax = 1.0f;
    [SerializeField]
    private float mGain;
    public float Gain
    {
        get
        {
            return mGain;
        }
        set
        {
            mGain = Mathf.Clamp(value, GainMin, GainMax);
        }
    }
    //#Q3DAudioRoomInSync
    public const float BrightAdjustMin = 0.0f;
    public const float BrightAdjustMax = 120.0f;
    [SerializeField]
    private float mBrightAdjust;
    public float BrightAdjust
    {
        get
        {
            return mBrightAdjust;
        }
        set
        {
            mBrightAdjust = Mathf.Clamp(value, BrightAdjustMin, BrightAdjustMax);
        }
    }
    //#Q3DAudioRoomInSync
    public const float TimeAdjustMin = 0.0f;
    public const float TimeAdjustMax = 120.0f;
    [SerializeField]
    private float mTimeAdjust;
    public float TimeAdjust
    {
        get
        {
            return mTimeAdjust;
        }
        set
        {
            mTimeAdjust = Mathf.Clamp(value,TimeAdjustMin,TimeAdjustMax);
        }
    }
    //#Q3DAudioRoomInSync
    public const int WetMixMin = 0;
    public const int WetMixMax = 1000;
    [SerializeField]
    private int mWetMix;
    public int WetMix
    {
        get
        {
            return mWetMix;
        }
        set
        {
            mWetMix = Mathf.Clamp(value, WetMixMin, WetMixMax);
        }
    }
    //#Q3DAudioRoomInSync
    public const float RoomDimensionsXMin_meters = 0.0f;
    public const float RoomDimensionsXMax_meters = 199.0f;
    [SerializeField]
    private float mRoomDimensionsX_meters = RoomDimensionsXMax_meters;
    public float RoomDimensionsX_meters
    {
        get
        {
            return mRoomDimensionsX_meters;
        }
        set
        {
            mRoomDimensionsX_meters = Mathf.Clamp(value, RoomDimensionsXMin_meters, RoomDimensionsXMax_meters);
        }
    }
    //#Q3DAudioRoomInSync
    public const float RoomDimensionsYMin_meters = 0.0f;
    public const float RoomDimensionsYMax_meters = 19.0f;
    [SerializeField]
    private float mRoomDimensionsY_meters = RoomDimensionsYMax_meters;
    public float RoomDimensionsY_meters
    {
        get
        {
            return mRoomDimensionsY_meters;
        }
        set
        {
            mRoomDimensionsY_meters = Mathf.Clamp(value, RoomDimensionsYMin_meters, RoomDimensionsYMax_meters);
        }
    }
    //#Q3DAudioRoomInSync
    public const float RoomDimensionsZMin_meters = 0.0f;
    public const float RoomDimensionsZMax_meters = 99.0f;
    [SerializeField]
    private float mRoomDimensionsZ_meters = RoomDimensionsZMax_meters;
    public float RoomDimensionsZ_meters
    {
        get
        {
            return mRoomDimensionsZ_meters;
        }
        set
        {
            mRoomDimensionsZ_meters = Mathf.Clamp(value, RoomDimensionsZMin_meters, RoomDimensionsZMax_meters);
        }
    }
    [SerializeField]
    private vr_audio_room_material_type mRoomMaterialWalls;
    public vr_audio_room_material_type RoomMaterialWalls
    {
        get
        {
            return mRoomMaterialWalls;
        }
        set
        {
            mRoomMaterialWalls = value;
        }
    }
    [SerializeField]
    private vr_audio_room_material_type mRoomMaterialCeiling;
    public vr_audio_room_material_type RoomMaterialCeiling
    {
        get
        {
            return mRoomMaterialCeiling;
        }
        set
        {
            mRoomMaterialCeiling = value;
        }
    }
    [SerializeField]
    private vr_audio_room_material_type mRoomMaterialFloor;
    public vr_audio_room_material_type RoomMaterialFloor
    {
        get
        {
            return mRoomMaterialFloor;
        }
        set
        {
            mRoomMaterialFloor = value;
        }
    }
}
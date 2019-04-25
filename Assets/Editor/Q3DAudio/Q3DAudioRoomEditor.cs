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

[CustomEditor(typeof(Q3DAudioRoom))]
[CanEditMultipleObjects]
public class Q3DAudioRoomEditor : Editor
{
    [MenuItem("GameObject/Audio/Q3DAudioRoom")]
    static void CreateQ3DAudioSource()
    {
        string Q3DAudioRoom = "Q3DAudioRoom";
        GameObject go = new GameObject(Q3DAudioRoom);
        Undo.RegisterCreatedObjectUndo(go, "Created " + Q3DAudioRoom);//this must be done before adding the component for undo to work!
        go.AddComponent<AudioReverbZone>();
        go.AddComponent<Q3DAudioRoom>();
    }

    [MenuItem("GameObject/Audio/Q3DTools/ReverbZonesToQ3DAudioRooms")]
    static void ReverbZonesToQ3DAudioRooms()
    {
        string Q3DAudioRoom = "Q3DAudioRoom";
        AudioReverbZone[] reverbZones = Q3DAudioManager.FindObjectsOfTypeAllWrapper<AudioReverbZone>();
        foreach(AudioReverbZone reverbZone in reverbZones)
        {
            if(!reverbZone.GetComponent<Q3DAudioRoom>())
            {
                Undo.RegisterCreatedObjectUndo(reverbZone, "Created " + Q3DAudioRoom);//this must be done before adding the component for undo to work!
                reverbZone.gameObject.AddComponent<Q3DAudioRoom>();
            }
        }
    }

    SerializedProperty
        mGainSerializedProperty,
        mTimeAdjustSerializedProperty,
        mBrightAdjustSerializedProperty,
        mWetMixSerializedProperty,
        mRoomDimensionsX_metersSerializedProperty,
        mRoomDimensionsY_metersSerializedProperty,
        mRoomDimensionsZ_metersSerializedProperty,
        mRoomMaterialWallsSerializedProperty,
        mRoomMaterialCeilingSerializedProperty,
        mRoomMaterialFloorSerializedProperty;
    void OnEnable()
    {
        mGainSerializedProperty = serializedObject.FindProperty("mGain");
        mTimeAdjustSerializedProperty = serializedObject.FindProperty("mTimeAdjust");
        mBrightAdjustSerializedProperty = serializedObject.FindProperty("mBrightAdjust");
        mWetMixSerializedProperty = serializedObject.FindProperty("mWetMix");
        mRoomDimensionsX_metersSerializedProperty = serializedObject.FindProperty("mRoomDimensionsX_meters");
        mRoomDimensionsY_metersSerializedProperty = serializedObject.FindProperty("mRoomDimensionsY_meters");
        mRoomDimensionsZ_metersSerializedProperty = serializedObject.FindProperty("mRoomDimensionsZ_meters");
        mRoomMaterialWallsSerializedProperty = serializedObject.FindProperty("mRoomMaterialWalls");
        mRoomMaterialCeilingSerializedProperty = serializedObject.FindProperty("mRoomMaterialCeiling");
        mRoomMaterialFloorSerializedProperty = serializedObject.FindProperty("mRoomMaterialFloor");
    }

    public override void OnInspectorGUI()
    {
        Q3DAudioRoom q3dAudioRoom = (Q3DAudioRoom)target;
        AudioReverbZone audioReverbZone = q3dAudioRoom.mAudioReverbZone = q3dAudioRoom.GetComponent<AudioReverbZone>();//maintain invariant of Q3DAudioRoom accessing its underlying AudioReverbZone
        if (audioReverbZone)
        {
            GUI.enabled = false;
            string takenFromUnderlyingReverbZone = "Taken from underlying ReverbZone";
            {
                EditorGUILayout.FloatField(
                    new GUIContent
                    (
                        "Min Distance",
                        takenFromUnderlyingReverbZone
                    ),
                    audioReverbZone.minDistance
                );
                EditorGUILayout.FloatField(
                    new GUIContent
                    (
                        "Max Distance",
                        takenFromUnderlyingReverbZone
                    ),
                    audioReverbZone.maxDistance
                );
            }
            GUI.enabled = true;

            //#Q3DAudioRoomInSync: keep GUIContent.tooltip text in sync with the description strings in Q3DAudioTestPlugin.cpp
            mGainSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "Gain",//see #Q3DAudioRoomInSync
                    "Linear volume gain; less than 0 attenuates"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.Gain,
                Q3DAudioRoom.GainMin,
                Q3DAudioRoom.GainMax);

            mTimeAdjustSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "TimeAdjust",//see #Q3DAudioRoomInSync
                    "Scales the reverb tail; larger values mean a longer tail"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.TimeAdjust,
                Q3DAudioRoom.TimeAdjustMin,
                Q3DAudioRoom.TimeAdjustMax);

            mBrightAdjustSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "BrightAdjust",//see #Q3DAudioRoomInSync
                    "Reverberation ratio across high and low frequencies"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.BrightAdjust,
                Q3DAudioRoom.BrightAdjustMin,
                Q3DAudioRoom.BrightAdjustMax);

            mWetMixSerializedProperty.intValue = EditorGUILayout.IntSlider(
                new GUIContent
                (
                    "WetMix",//see #Q3DAudioRoomInSync
                    "Ratio between wet and dry mix -- larger values are wetter"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.WetMix,
                Q3DAudioRoom.WetMixMin,
                Q3DAudioRoom.WetMixMax);

            mRoomDimensionsX_metersSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "RoomDimensionsX_meters",//see #Q3DAudioRoomInSync
                    "Shoebox model distance from wall-to-wall along the player's left-right axis"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.RoomDimensionsX_meters,
                Q3DAudioRoom.RoomDimensionsXMin_meters,
                Q3DAudioRoom.RoomDimensionsXMax_meters);

            mRoomDimensionsY_metersSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "RoomDimensionsY_meters",//see #Q3DAudioRoomInSync
                    "Shoebox model distance from floor-to-ceiling relative to the player"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.RoomDimensionsY_meters,
                Q3DAudioRoom.RoomDimensionsYMin_meters,
                Q3DAudioRoom.RoomDimensionsYMax_meters);

            mRoomDimensionsZ_metersSerializedProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent
                (
                    "RoomDimensionsZ_meters",//see #Q3DAudioRoomInSync
                    "Shoebox model distance from wall-to-wall along the player's forward-back axis"//see #Q3DAudioRoomInSync
                ),
                q3dAudioRoom.RoomDimensionsZ_meters,
                Q3DAudioRoom.RoomDimensionsZMin_meters,
                Q3DAudioRoom.RoomDimensionsZMax_meters);

            GUIContent[] materialOptions = new GUIContent[]
            {
                //keep in sync with Q3DAudioRoom.vr_audio_room_material_type
                new GUIContent("TRANSPARENT",               "Acoustically transparent material, reflects no sound"),
                new GUIContent("ACOUSTIC_CEILING_TILES",    "Acoustic ceiling tiles, absorbs most frequencies"),
                new GUIContent("BRICK_BARE",                "Bare brick, relatively reflective"),
                new GUIContent("BRICK_PAINTED",             "Painted brick"),
                new GUIContent("CONCRETE_BLOCK_COARSE",     "Coarse surface concrete block"),
                new GUIContent("CONCRETE_BLOCK_PAINTED",    "Painted concrete block"),
                new GUIContent("CURTAIN_HEAVY",             "Heavy curtains"),
                new GUIContent("FIBER_GLASS_INSULATION",    "Fiber glass insulation"),
                new GUIContent("GLASS_THICK",               "Thin glass"),
                new GUIContent("GLASS_THIN",                "Thick glass"),
                new GUIContent("GRASS",                     "Grass"),
                new GUIContent("LINOLEUM_ON_CONCRETE",      "Linoleum on concrete"),
                new GUIContent("MARBLE",                    "Marble"),
                new GUIContent("METAL",                     "Galvanized sheet metal"),
                new GUIContent("PARQUET_ON_CONCRETE",       "Wooden parquet on concrete"),
                new GUIContent("PLASTER_ROUGH",             "Rough plaster surface"),
                new GUIContent("PLASTER_SMOOTH",            "Smooth plaster surface"),
                new GUIContent("PLYWOOD_PANEL",             "Plywood panel"),
                new GUIContent("POLISHED_CONCRETE_OR_TILE", "Polished concrete OR tile surface"),
                new GUIContent("SHEET_ROCK",                "Sheet rock"),
                new GUIContent("WATER_OR_ICE_SURFACE",      "Surface of water or ice"),
                new GUIContent("WOOD_ON_CEILING",           "Wooden ceiling"),
                new GUIContent("WOOD_PANEL",	            "Wood paneling")
            };
            mRoomMaterialWallsSerializedProperty.enumValueIndex =
                (int)(Q3DAudioRoom.vr_audio_room_material_type)EditorGUILayout.Popup(
                    new GUIContent
                    (
                        "RoomMaterialWalls",
                        "Shoebox material type for the walls always in front, back and to the sides of the player (that conceptually follows the player's translation and orientation)"//see #Q3DAudioRoomInSync
                    ),
                    (int)q3dAudioRoom.RoomMaterialWalls,
                    materialOptions
            );
            mRoomMaterialCeilingSerializedProperty.enumValueIndex =
                (int)(Q3DAudioRoom.vr_audio_room_material_type)EditorGUILayout.Popup(
                    new GUIContent
                    (
                        "RoomMaterialCeiling",
                        "Shoebox material type for the ceiling always above the player (that conceptually follows the player's translation and orientation)"//see #Q3DAudioRoomInSync
                    ),
                    (int)q3dAudioRoom.RoomMaterialCeiling,
                    materialOptions
            );
            mRoomMaterialFloorSerializedProperty.enumValueIndex =
                (int)(Q3DAudioRoom.vr_audio_room_material_type)EditorGUILayout.Popup(
                    new GUIContent
                    (
                        "RoomMaterialFloor",
                        "Shoebox material type for the floor always below the player (that conceptually follows the player's translation and orientation)"//see #Q3DAudioRoomInSync
                    ),
                    (int)q3dAudioRoom.RoomMaterialFloor,
                    materialOptions
            );

            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.LabelField("Disabled", "Must be placed on a GameObject that has an AudioReverbZone, but no AudioReverbZone is found");
        }
    }
}

#endif//#if UNITY_EDITOR
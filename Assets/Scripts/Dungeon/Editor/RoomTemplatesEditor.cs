using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(RoomTemplates))]
public class RoomTemplatesEditor : Editor
{
    private bool showRoomArrays = true;
    private bool showLargeRoomSettings = false;
    private bool showManagerReferences = false;

    public override void OnInspectorGUI()
    {
        RoomTemplates roomTemplates = (RoomTemplates)target;
        serializedObject.Update();

        // Room Generation Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Generation Settings", EditorStyles.boldLabel);

        // Standard settings
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumRoomsToGenerate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EntryRoom"));

        // Room Arrays Section
        EditorGUILayout.Space();
        showRoomArrays = EditorGUILayout.Foldout(showRoomArrays, "Room Templates", true, EditorStyles.foldoutHeader);
        if (showRoomArrays)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bottomRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("closedRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorRooms"), true);
            EditorGUI.indentLevel--;
        }

        // Large Room Settings
        EditorGUILayout.Space();
        showLargeRoomSettings = EditorGUILayout.Foldout(showLargeRoomSettings, "Large Room Settings", true, EditorStyles.foldoutHeader);
        if (showLargeRoomSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("largeRooms"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("largeRoomSpawnChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLargeRoomsPerDungeon"));
            EditorGUI.indentLevel--;
        }

        // Manager References
        EditorGUILayout.Space();
        showManagerReferences = EditorGUILayout.Foldout(showManagerReferences, "Manager References", true, EditorStyles.foldoutHeader);
        if (showManagerReferences)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemySpawnManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("specialRoomManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dungeonStateManager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
            EditorGUI.indentLevel--;
        }

        // Runtime Status (Read Only)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("Spawned Boss", roomTemplates.spawnedBoss);
        if (roomTemplates.dungeonStateManager != null)
        {
            EditorGUILayout.Toggle("Enemy Rooms Complete", roomTemplates.enemyRoomsComplete);
            EditorGUILayout.IntField("Large Rooms Spawned", roomTemplates.dungeonStateManager.largeRoomsSpawned);
            EditorGUILayout.Toggle("Is Setting Up", roomTemplates.dungeonStateManager.isSettingUp);
            EditorGUILayout.Toggle("Is Prefab Dungeon", roomTemplates.dungeonStateManager.isPrefabDungeon);
            EditorGUILayout.FloatField("Wait Time", roomTemplates.dungeonStateManager.waitTime);
        }
        EditorGUILayout.IntField("Room Count", roomTemplates.rooms.Count);
        EditorGUI.EndDisabledGroup();

        // Display special room status if available
        if (roomTemplates.specialRoomManager != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Special Room Status", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Boss Room Spawned", roomTemplates.specialRoomManager.spawnedBoss);
            EditorGUILayout.Toggle("Store Room Spawned", roomTemplates.specialRoomManager.spawnedStore);
            EditorGUILayout.Toggle("Casino Room Spawned", roomTemplates.specialRoomManager.spawnedCasino);
            EditorGUILayout.Toggle("Chest Rooms Spawned", roomTemplates.specialRoomManager.spawnedChestRooms);
            EditorGUILayout.Toggle("Cauldron Room Spawned", roomTemplates.specialRoomManager.spawnedCauldron);

            // Display custom special rooms if any
            if (roomTemplates.specialRoomManager.specialRooms.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Custom Special Rooms", EditorStyles.boldLabel);

                foreach (SpecialRoom specialRoom in roomTemplates.specialRoomManager.specialRooms)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(specialRoom.roomName);
                    EditorGUILayout.Toggle("Spawned", specialRoom.hasSpawned);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        // Enemy spawn information if available
        if (roomTemplates.enemySpawnManager != null && EditorApplication.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Enemy Configuration", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);

            // We can't display the enemy configs directly since they're likely private
            // But we can show a summary
            EditorGUILayout.LabelField("Enemy Configurations Available",
                $"{roomTemplates.enemySpawnManager.GetType().Name} is attached");

            EditorGUI.EndDisabledGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
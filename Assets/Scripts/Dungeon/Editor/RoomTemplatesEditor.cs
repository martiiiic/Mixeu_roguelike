#if UNITY_EDITOR
using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomTemplates))]
public class RoomTemplatesEditor : Editor
{
    private SerializedProperty largeRoomsProperty;
    private SerializedProperty largeRoomSpawnChanceProperty;
    private SerializedProperty maxLargeRoomsPerDungeonProperty;
    private SerializedProperty bottomRoomsProperty;
    private SerializedProperty topRoomsProperty;
    private SerializedProperty leftRoomsProperty;
    private SerializedProperty rightRoomsProperty;
    private SerializedProperty closedRoomsProperty;
    private SerializedProperty entryRoomProperty;
    private SerializedProperty bossProperty;
    private SerializedProperty chestRoomProperty;
    private SerializedProperty storeProperty;
    private SerializedProperty casinoProperty;
    private SerializedProperty cauldronProperty;
    private SerializedProperty specialRoomsProperty;
    private SerializedProperty minimumRoomsToGenerateProperty;
    private SerializedProperty enemySpawnManagerProperty;

    // Add the missing properties
    private SerializedProperty handProperty;
    private SerializedProperty objectMenuProperty;
    private SerializedProperty prefabDungeonsProperty;

    private bool showLargeRoomSettings = true;
    private bool showRoomSettings = true;
    private bool showSpecialRoomSettings = true;
    private bool showEnemySettings = true;
    private bool showGameObjectReferences = true;  // New section for references

    private Editor enemySpawnManagerEditor;

    private void OnEnable()
    {
        largeRoomsProperty = serializedObject.FindProperty("largeRooms");
        largeRoomSpawnChanceProperty = serializedObject.FindProperty("largeRoomSpawnChance");
        maxLargeRoomsPerDungeonProperty = serializedObject.FindProperty("maxLargeRoomsPerDungeon");
        bottomRoomsProperty = serializedObject.FindProperty("bottomRooms");
        topRoomsProperty = serializedObject.FindProperty("topRooms");
        leftRoomsProperty = serializedObject.FindProperty("leftRooms");
        rightRoomsProperty = serializedObject.FindProperty("rightRooms");
        closedRoomsProperty = serializedObject.FindProperty("closedRooms");
        entryRoomProperty = serializedObject.FindProperty("EntryRoom");
        bossProperty = serializedObject.FindProperty("boss");
        chestRoomProperty = serializedObject.FindProperty("chestRoom");
        storeProperty = serializedObject.FindProperty("store");
        casinoProperty = serializedObject.FindProperty("Casino");
        cauldronProperty = serializedObject.FindProperty("Cauldron");
        specialRoomsProperty = serializedObject.FindProperty("specialRooms");
        minimumRoomsToGenerateProperty = serializedObject.FindProperty("minimumRoomsToGenerate");
        enemySpawnManagerProperty = serializedObject.FindProperty("enemySpawnManager");

        // Initialize the new properties
        handProperty = serializedObject.FindProperty("Hand");
        //wcProperty = serializedObject.FindProperty("WC");
        objectMenuProperty = serializedObject.FindProperty("ObjectMenu");
        prefabDungeonsProperty = serializedObject.FindProperty("PrefabDungeons");

        RoomTemplates roomTemplates = (RoomTemplates)target;
        if (roomTemplates.enemySpawnManager != null)
        {
            enemySpawnManagerEditor = CreateEditor(roomTemplates.enemySpawnManager);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RoomTemplates roomTemplates = (RoomTemplates)target;

        EditorGUILayout.Space();

        // Room Generation Settings
        showRoomSettings = EditorGUILayout.Foldout(showRoomSettings, "Room Generation Settings", true, EditorStyles.foldoutHeader);
        if (showRoomSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(minimumRoomsToGenerateProperty);
            EditorGUILayout.PropertyField(entryRoomProperty);
            EditorGUILayout.PropertyField(bottomRoomsProperty, true);
            EditorGUILayout.PropertyField(topRoomsProperty, true);
            EditorGUILayout.PropertyField(leftRoomsProperty, true);
            EditorGUILayout.PropertyField(rightRoomsProperty, true);
            EditorGUILayout.PropertyField(closedRoomsProperty, true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Special Rooms Settings
        showSpecialRoomSettings = EditorGUILayout.Foldout(showSpecialRoomSettings, "Special Rooms Settings", true, EditorStyles.foldoutHeader);
        if (showSpecialRoomSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(bossProperty);
            EditorGUILayout.PropertyField(chestRoomProperty);
            EditorGUILayout.PropertyField(storeProperty);
            EditorGUILayout.PropertyField(casinoProperty);
            EditorGUILayout.PropertyField(cauldronProperty);
            EditorGUILayout.PropertyField(specialRoomsProperty, true);

            // Add New Special Room button
            if (GUILayout.Button("Add New Special Room"))
            {
                Undo.RecordObject(roomTemplates, "Add Special Room");
                SpecialRoom newRoom = new SpecialRoom();
                roomTemplates.specialRooms.Add(newRoom);
                EditorUtility.SetDirty(roomTemplates);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Large Room Settings
        showLargeRoomSettings = EditorGUILayout.Foldout(showLargeRoomSettings, "Large Room Settings", true, EditorStyles.foldoutHeader);
        if (showLargeRoomSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(largeRoomsProperty, true);
            EditorGUILayout.PropertyField(largeRoomSpawnChanceProperty);
            EditorGUILayout.PropertyField(maxLargeRoomsPerDungeonProperty);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // New section for GameObject references
        showGameObjectReferences = EditorGUILayout.Foldout(showGameObjectReferences, "GameObject References", true, EditorStyles.foldoutHeader);
        if (showGameObjectReferences)
        {
            EditorGUI.indentLevel++;

            // Draw the Hand property with a helpful label
            EditorGUILayout.PropertyField(handProperty, new GUIContent("Hand", "Reference to the hand animation GameObject"));

            // Draw other GameObject references
            //EditorGUILayout.PropertyField(wcProperty, new GUIContent("Weapon Collectible", "Reference to the weapon collectible prefab"));
            EditorGUILayout.PropertyField(objectMenuProperty, new GUIContent("Object Menu", "Reference to the object stats menu"));
            EditorGUILayout.PropertyField(prefabDungeonsProperty, new GUIContent("Prefab Dungeons", "Array of prefab dungeons"));

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Enemy Settings
        showEnemySettings = EditorGUILayout.Foldout(showEnemySettings, "Enemy Settings", true, EditorStyles.foldoutHeader);
        if (showEnemySettings)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(enemySpawnManagerProperty);

            if (roomTemplates.enemySpawnManager == null)
            {
                EditorGUILayout.HelpBox("Assign an Enemy Spawn Manager to configure enemy spawning.", MessageType.Info);
                if (GUILayout.Button("Create Enemy Spawn Manager"))
                {
                    roomTemplates.enemySpawnManager = roomTemplates.gameObject.AddComponent<EnemySpawnManager>();
                    enemySpawnManagerEditor = CreateEditor(roomTemplates.enemySpawnManager);
                    EditorUtility.SetDirty(roomTemplates);
                }
            }
            else if (enemySpawnManagerEditor != null)
            {
                // Draw a line to separate the embedded editor
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Enemy Spawn Configuration", EditorStyles.boldLabel);

                // Use the editor to draw its inspector
                enemySpawnManagerEditor.OnInspectorGUI();
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnDisable()
    {
        if (enemySpawnManagerEditor != null)
        {
            DestroyImmediate(enemySpawnManagerEditor);
        }
    }
}
#endif
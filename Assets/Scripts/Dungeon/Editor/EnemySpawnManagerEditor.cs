using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemySpawnManager))]
public class EnemySpawnManagerEditor : Editor
{
    private bool showGeneralSettings = true;
    private List<bool> showEnemySettings;
    private SerializedProperty enemyConfigsProperty;

    private void OnEnable()
    {
        enemyConfigsProperty = serializedObject.FindProperty("enemyConfigs");
        InitializeEnemySettings();
    }

    private void InitializeEnemySettings()
    {
        if (showEnemySettings == null)
        {
            showEnemySettings = new List<bool>();
            for (int i = 0; i < enemyConfigsProperty.arraySize; i++)
            {
                showEnemySettings.Add(false);
            }
        }
        while (showEnemySettings.Count < enemyConfigsProperty.arraySize)
        {
            showEnemySettings.Add(false);
        }
        while (showEnemySettings.Count > enemyConfigsProperty.arraySize)
        {
            showEnemySettings.RemoveAt(showEnemySettings.Count - 1);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (enemyConfigsProperty == null)
        {
            enemyConfigsProperty = serializedObject.FindProperty("enemyConfigs");
            InitializeEnemySettings();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "Enemy Configuration", true, EditorStyles.foldoutHeader);
        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            enemyConfigsProperty.arraySize++;
            showEnemySettings.Add(true);
        }
        EditorGUILayout.EndHorizontal();
        if (showGeneralSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Configure which enemies can spawn in different dungeon contexts", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            DrawEnemyConfigs();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEnemyConfigs()
    {
        for (int i = 0; i < enemyConfigsProperty.arraySize; i++)
        {
            SerializedProperty enemyConfigProperty = enemyConfigsProperty.GetArrayElementAtIndex(i);
            SerializedProperty enemyPrefabProperty = enemyConfigProperty.FindPropertyRelative("enemyPrefab");
            EditorGUILayout.BeginHorizontal();
            string enemyName = enemyPrefabProperty.objectReferenceValue != null
                ? enemyPrefabProperty.objectReferenceValue.name
                : "New Enemy Config";
            showEnemySettings[i] = EditorGUILayout.Foldout(showEnemySettings[i], enemyName, true);
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                enemyConfigsProperty.DeleteArrayElementAtIndex(i);
                showEnemySettings.RemoveAt(i);
                // Return early to avoid processing the deleted element
                return;
            }
            EditorGUILayout.EndHorizontal();
            if (showEnemySettings[i])
            {
                EditorGUI.indentLevel++;
                DrawEnemyConfigProperties(enemyConfigProperty);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
        }
    }

    private void DrawEnemyConfigProperties(SerializedProperty enemyConfigProperty)
    {
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("enemyPrefab"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("minDungeonLevel"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnProbability"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("maxEnemiesPerRoom"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Locations", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnInBossRooms"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnInStoreRooms"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnInChestRooms"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnInCauldronRooms"));
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("spawnInSpecialRooms"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Compatibility", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("compatibleSpecialDungeons"), true);
        EditorGUILayout.PropertyField(enemyConfigProperty.FindPropertyRelative("incompatibleEnemyTags"), true);
    }
}
#endif
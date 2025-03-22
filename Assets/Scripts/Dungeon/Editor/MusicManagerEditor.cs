using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(Music))]
public class MusicEditor : Editor
{
    private bool showLegacyClips = false;
    private bool showTrackList = true;
    private bool showAudioSettings = false;
    private bool showCurrentState = false;
    private Music musicManager;
    private SerializedProperty musicTracks;
    private Vector2 scrollPosition;

    // Track being edited
    private int selectedTrackIndex = -1;
    private bool isAddingNewTrack = false;
    private string newTrackName = "New Track";
    private AudioClip newTrackClip;
    private bool newTrackCanPlayRandomly = true;
    private List<int> newTrackDungeons = new List<int>();
    private List<MusicContext> newTrackContexts = new List<MusicContext>();
    private int newTrackBossID = -1;
    private int dungeonToAdd = 1;

    private void OnEnable()
    {
        musicManager = (Music)target;
        musicTracks = serializedObject.FindProperty("musicTracks");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Music Manager", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Audio Sources
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Audio Sources", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientSource"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("musicSource"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Legacy Clips Section
        showLegacyClips = EditorGUILayout.Foldout(showLegacyClips, "Legacy Clips (Backwards Compatibility)", true);
        if (showLegacyClips)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientClips"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("musicClips"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zoneClips"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("combatClips"), true);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(5);

        // Music Settings Section
        showAudioSettings = EditorGUILayout.Foldout(showAudioSettings, "Music Settings", true);
        if (showAudioSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CanStartPlaying"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFadeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("musicVolume"));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(5);

        // Current State
        showCurrentState = EditorGUILayout.Foldout(showCurrentState, "Current State", true);
        if (showCurrentState)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentDungeonNumber"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentContext"));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        // Track List Section
        showTrackList = EditorGUILayout.Foldout(showTrackList, "Music Tracks", true);
        if (showTrackList)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Music Tracks Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure each track with its play context and dungeon settings", MessageType.Info);

            // Tracks Count
            EditorGUILayout.LabelField($"Total Tracks: {musicTracks.arraySize}");
            EditorGUILayout.Space(5);

            // Track List
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            if (musicTracks.arraySize == 0)
            {
                GUILayout.Label("No tracks added yet. Click 'Add New Track' to begin.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < musicTracks.arraySize; i++)
                {
                    SerializedProperty track = musicTracks.GetArrayElementAtIndex(i);
                    SerializedProperty trackName = track.FindPropertyRelative("trackName");
                    SerializedProperty clip = track.FindPropertyRelative("clip");
                    SerializedProperty contexts = track.FindPropertyRelative("allowedContexts");
                    SerializedProperty bossID = track.FindPropertyRelative("bossID");

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {trackName.stringValue}", EditorStyles.boldLabel);

                    GUI.backgroundColor = new Color(0.9f, 0.9f, 1f);
                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                    {
                        selectedTrackIndex = i;
                        isAddingNewTrack = false;
                    }

                    GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Track",
                            $"Are you sure you want to delete the track '{trackName.stringValue}'?",
                            "Delete", "Cancel"))
                        {
                            musicTracks.DeleteArrayElementAtIndex(i);
                            if (selectedTrackIndex == i) selectedTrackIndex = -1;
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();

                    // Display basic info
                    EditorGUILayout.LabelField("Clip:", clip.objectReferenceValue ? clip.objectReferenceValue.name : "None");

                    // Display contexts
                    EditorGUI.indentLevel++;
                    if (contexts.arraySize > 0)
                    {
                        string contextList = "Contexts: ";
                        for (int c = 0; c < contexts.arraySize; c++)
                        {
                            contextList += contexts.GetArrayElementAtIndex(c).enumDisplayNames[contexts.GetArrayElementAtIndex(c).enumValueIndex];
                            if (c < contexts.arraySize - 1) contextList += ", ";
                        }
                        EditorGUILayout.LabelField(contextList);
                    }

                    // Display boss ID if relevant
                    if (bossID.intValue >= 0)
                    {
                        EditorGUILayout.LabelField($"Boss ID: {bossID.intValue}");
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            // Add New Track Button
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button("Add New Track", GUILayout.Height(30)))
            {
                isAddingNewTrack = true;
                selectedTrackIndex = -1;
                newTrackName = "New Track";
                newTrackClip = null;
                newTrackCanPlayRandomly = true;
                newTrackDungeons.Clear();
                newTrackContexts.Clear();
                newTrackBossID = -1;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        // Track Edit Panel
        if (selectedTrackIndex >= 0 || isAddingNewTrack)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (isAddingNewTrack)
            {
                GUILayout.Label("Add New Track", EditorStyles.boldLabel);

                newTrackName = EditorGUILayout.TextField("Track Name", newTrackName);
                newTrackClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", newTrackClip, typeof(AudioClip), false);
                newTrackCanPlayRandomly = EditorGUILayout.Toggle("Can Play Randomly", newTrackCanPlayRandomly);

                EditorGUILayout.Space(5);

                // Dungeon Numbers
                EditorGUILayout.LabelField("Dungeon Numbers (empty = all dungeons)", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                dungeonToAdd = EditorGUILayout.IntField("Dungeon to Add", dungeonToAdd);

                GUI.enabled = dungeonToAdd > 0;
                if (GUILayout.Button("Add Dungeon", GUILayout.Width(100)))
                {
                    if (!newTrackDungeons.Contains(dungeonToAdd))
                    {
                        newTrackDungeons.Add(dungeonToAdd);
                    }
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                // Display dungeon list
                if (newTrackDungeons.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Current Dungeons:");

                    string dungeonList = "";
                    for (int i = 0; i < newTrackDungeons.Count; i++)
                    {
                        dungeonList += newTrackDungeons[i].ToString();
                        if (i < newTrackDungeons.Count - 1) dungeonList += ", ";
                    }
                    EditorGUILayout.LabelField(dungeonList);

                    if (GUILayout.Button("Clear All", GUILayout.Width(70)))
                    {
                        newTrackDungeons.Clear();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5);

                // Contexts
                EditorGUILayout.LabelField("Music Contexts", EditorStyles.boldLabel);

                foreach (MusicContext context in System.Enum.GetValues(typeof(MusicContext)))
                {
                    bool isSelected = newTrackContexts.Contains(context);
                    bool newValue = EditorGUILayout.Toggle(context.ToString(), isSelected);

                    if (newValue && !isSelected)
                    {
                        newTrackContexts.Add(context);
                    }
                    else if (!newValue && isSelected)
                    {
                        newTrackContexts.Remove(context);
                    }
                }

                EditorGUILayout.Space(5);

                // Boss ID
                EditorGUILayout.LabelField("Boss Settings", EditorStyles.boldLabel);

                bool isBossMusic = EditorGUILayout.Toggle("Is Boss Music", newTrackBossID >= 0);
                if (isBossMusic)
                {
                    newTrackBossID = Mathf.Max(0, EditorGUILayout.IntField("Boss ID", newTrackBossID >= 0 ? newTrackBossID : 0));
                }
                else
                {
                    newTrackBossID = -1;
                }

                EditorGUILayout.Space(10);

                // Add Button
                GUI.enabled = !string.IsNullOrEmpty(newTrackName) && newTrackClip != null && newTrackContexts.Count > 0;
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);

                if (GUILayout.Button("Add Track", GUILayout.Height(30)))
                {
                    // Add the new track
                    musicTracks.arraySize++;
                    SerializedProperty newTrack = musicTracks.GetArrayElementAtIndex(musicTracks.arraySize - 1);

                    newTrack.FindPropertyRelative("trackName").stringValue = newTrackName;
                    newTrack.FindPropertyRelative("clip").objectReferenceValue = newTrackClip;
                    newTrack.FindPropertyRelative("canPlayRandomly").boolValue = newTrackCanPlayRandomly;
                    newTrack.FindPropertyRelative("bossID").intValue = newTrackBossID;

                    // Set dungeon numbers
                    SerializedProperty dungeonsProp = newTrack.FindPropertyRelative("dungeonNumbers");
                    dungeonsProp.arraySize = newTrackDungeons.Count;
                    for (int i = 0; i < newTrackDungeons.Count; i++)
                    {
                        dungeonsProp.GetArrayElementAtIndex(i).intValue = newTrackDungeons[i];
                    }

                    // Set contexts
                    SerializedProperty contextsProp = newTrack.FindPropertyRelative("allowedContexts");
                    contextsProp.arraySize = newTrackContexts.Count;
                    for (int i = 0; i < newTrackContexts.Count; i++)
                    {
                        contextsProp.GetArrayElementAtIndex(i).enumValueIndex = (int)newTrackContexts[i];
                    }

                    isAddingNewTrack = false;
                    serializedObject.ApplyModifiedProperties();
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;

                EditorGUILayout.Space(5);

                // Cancel Button
                if (GUILayout.Button("Cancel", GUILayout.Height(25)))
                {
                    isAddingNewTrack = false;
                }
            }
            else if (selectedTrackIndex >= 0 && selectedTrackIndex < musicTracks.arraySize)
            {
                // Edit existing track
                SerializedProperty track = musicTracks.GetArrayElementAtIndex(selectedTrackIndex);

                GUILayout.Label($"Edit Track: {track.FindPropertyRelative("trackName").stringValue}", EditorStyles.boldLabel);

                // Basic properties
                EditorGUILayout.PropertyField(track.FindPropertyRelative("trackName"));
                EditorGUILayout.PropertyField(track.FindPropertyRelative("clip"));
                EditorGUILayout.PropertyField(track.FindPropertyRelative("canPlayRandomly"));

                EditorGUILayout.Space(5);

                // Dungeon numbers
                EditorGUILayout.PropertyField(track.FindPropertyRelative("dungeonNumbers"), true);

                EditorGUILayout.Space(5);

                // Contexts
                EditorGUILayout.PropertyField(track.FindPropertyRelative("allowedContexts"), true);

                EditorGUILayout.Space(5);

                // Boss ID
                EditorGUILayout.PropertyField(track.FindPropertyRelative("bossID"));

                EditorGUILayout.Space(10);

                // Done button
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
                {
                    selectedTrackIndex = -1;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(5);

                // Cancel button
                if (GUILayout.Button("Cancel", GUILayout.Height(25)))
                {
                    selectedTrackIndex = -1;
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        // Testing Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Test Music Playback (Editor Only)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play Ambient"))
        {
            if (Application.isPlaying)
                musicManager.PlayContextMusic(MusicContext.Ambient);
        }

        if (GUILayout.Button("Play Combat"))
        {
            if (Application.isPlaying)
                musicManager.PlayContextMusic(MusicContext.Combat);
        }

        if (GUILayout.Button("Play Boss"))
        {
            if (Application.isPlaying)
                musicManager.PlayContextMusic(MusicContext.Boss);
        }

        if (GUILayout.Button("Stop All"))
        {
            if (Application.isPlaying)
                musicManager.StopAllMusic();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
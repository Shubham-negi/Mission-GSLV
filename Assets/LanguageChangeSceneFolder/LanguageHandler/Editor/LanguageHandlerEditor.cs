using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LanguageHandler))]
public class LanguageHandlerEditor : Editor
{
    string[] _choices;
    string filePath = "Assets/LanguageChangeSceneFolder/LanguageHandler/Script/";
    string fileName = "LanguageID";

    public List<string> languageIDs;

    public override void OnInspectorGUI ()
    {
        LanguageHandler _LH = target as LanguageHandler;
        _choices = new string[_LH.Languages.Count];
        for (int i = 0; i < _LH.Languages.Count; i++)
        {
            _choices[i] = _LH.Languages[i].DisplayName;
        }

      

        GUILayout.Space(10);
        DrawDefaultInspector();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Current Language :");
        EditorGUILayout.LabelField(PlayerPrefs.GetString("currentLanguage"),EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        Undo.RecordObject(target,"Change Default Language");

        GUILayout.Space(5);
        filePath = EditorGUILayout.TextField("Script Path", filePath);
        fileName = EditorGUILayout.TextField("Script Name", fileName);

        GUILayout.Space(5);
        if (GUILayout.Button("Save"))
        {
            EdiorMethods.WriteToEnum(filePath, fileName, _LH.languageIDs);
        }

    }
}

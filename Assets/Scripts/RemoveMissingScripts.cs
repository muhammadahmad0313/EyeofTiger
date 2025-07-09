using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RemoveMissingScripts : EditorWindow
{
    [MenuItem("Tools/Remove Missing Scripts")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<RemoveMissingScripts>("Remove Missing Scripts");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Remove Missing Scripts in Selected Assets"))
        {
            RemoveMissingScriptsInSelection();
        }

        if (GUILayout.Button("Remove Missing Scripts in All Prefabs"))
        {
            RemoveMissingScriptsInAllPrefabs();
        }
    }

    private void RemoveMissingScriptsInSelection()
    {
        GameObject[] selection = Selection.gameObjects;
        int componentsRemoved = 0;
        
        foreach (GameObject gameObject in selection)
        {
            componentsRemoved += ProcessGameObject(gameObject);
        }
        
        Debug.Log($"Removed {componentsRemoved} missing scripts from {selection.Length} GameObjects");
    }

    private void RemoveMissingScriptsInAllPrefabs()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths();
        List<string> prefabs = new List<string>();
        int componentsRemoved = 0;
        
        foreach (string path in prefabPaths)
        {
            if (path.EndsWith(".prefab"))
            {
                prefabs.Add(path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    componentsRemoved += ProcessGameObject(prefab);
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Removed {componentsRemoved} missing scripts from {prefabs.Count} prefabs");
    }

    private int ProcessGameObject(GameObject gameObject)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        
        // Process children
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            count += ProcessGameObject(transform.GetChild(i).gameObject);
        }
        
        return count;
    }
}

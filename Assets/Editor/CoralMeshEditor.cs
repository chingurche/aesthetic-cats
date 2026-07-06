using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ProceduralCoralMesh))]
public class CoralMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProceduralCoralMesh coralGenerator = (ProceduralCoralMesh)target;

        GUILayout.Space(15);

        if (GUILayout.Button("1. Сгенерировать Коралл", GUILayout.Height(35)))
        {
            coralGenerator.GenerateCoral();
            EditorUtility.SetDirty(coralGenerator);
        }

        if (GUILayout.Button("Очистить меш", GUILayout.Height(25)))
        {
            coralGenerator.ClearCoral();
            EditorUtility.SetDirty(coralGenerator);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("2. Сохранить как Префаб", GUILayout.Height(35)))
        {
            SaveAsPrefab(coralGenerator);
        }
    }

    private void SaveAsPrefab(ProceduralCoralMesh generator)
    {
        MeshFilter meshFilter = generator.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Сначала сгенерируйте коралл!", "ОК");
            return;
        }

        string folderPath = "Assets/ProceduralCorals";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string uniqueName = $"Coral_Seed_{generator.randomSeed}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        string meshPath = $"{folderPath}/{uniqueName}_Mesh.asset";
        string prefabPath = $"{folderPath}/{uniqueName}.prefab";

        Mesh meshToSave = Instantiate(meshFilter.sharedMesh);
        AssetDatabase.CreateAsset(meshToSave, meshPath);
        AssetDatabase.SaveAssets();

        GameObject prefabRoot = new GameObject(uniqueName);

        MeshFilter pFilter = prefabRoot.AddComponent<MeshFilter>();
        MeshRenderer pRenderer = prefabRoot.AddComponent<MeshRenderer>();

        pFilter.sharedMesh = meshToSave;
        pRenderer.sharedMaterial = generator.GetComponent<MeshRenderer>().sharedMaterial;

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

        DestroyImmediate(prefabRoot);

        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(savedPrefab);

        EditorUtility.DisplayDialog("Успех!", $"Коралл успешно сохранен!\n\nПрефаб: {prefabPath}\nМеш: {meshPath}", "Отлично");
    }
}
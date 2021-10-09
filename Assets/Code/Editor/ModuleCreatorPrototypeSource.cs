using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(ModuleCreatorSource))]
public class ModuleCreatorSourceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Add Meshes"))
		{
			(target as ModuleCreatorSource).AddMeshes();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (GUILayout.Button("Sort Children"))
		{
			(target as ModuleCreatorSource).SortChildren();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		DrawDefaultInspector();
	}
}
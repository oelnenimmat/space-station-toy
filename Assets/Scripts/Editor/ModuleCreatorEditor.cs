using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(ModuleCreator))]
public class ModuleCreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Create"))
		{
			(target as ModuleCreator).Create();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			EditorUtility.SetDirty((target as ModuleCreator).collection);
		}

		DrawDefaultInspector();
	}
}
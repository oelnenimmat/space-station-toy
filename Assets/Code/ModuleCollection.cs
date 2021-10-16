using UnityEngine;

[CreateAssetMenu]
public class ModuleCollection : ScriptableObject
{
	public Module[] modules;
	public int emptyModuleIndex;
	public int temporaryVisualModuleIndex;

	public int [] structureModuleIndices;
	public int [] solarArrayModuleIndices;
}
using UnityEngine;

[CreateAssetMenu]
public class ModuleCollection : ScriptableObject
{
	public Module[] modules;
	public int emptyModuleIndex;
}
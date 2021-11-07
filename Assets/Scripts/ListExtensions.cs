using System.Collections.Generic;

public static class ListExtensions
{
	public static void SwapRemove<T>(this List<T> list, int index)
	{
		int lastIndex = list.Count - 1;
		list[index] = list[lastIndex];
		list.RemoveAt(lastIndex);
	}
}
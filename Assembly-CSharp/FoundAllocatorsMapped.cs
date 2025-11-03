using System;
using System.Collections.Generic;

[Serializable]
public class FoundAllocatorsMapped
{
	public string path;

	public List<ViewsAndAllocator> allocators = new List<ViewsAndAllocator>();

	public List<FoundAllocatorsMapped> subGroups = new List<FoundAllocatorsMapped>();
}

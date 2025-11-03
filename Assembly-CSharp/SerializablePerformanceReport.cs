using System;
using System.Collections.Generic;

[Serializable]
public class SerializablePerformanceReport<T>
{
	public string reportDate;

	public string version;

	public List<T> results;
}

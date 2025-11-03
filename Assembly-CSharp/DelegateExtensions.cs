using System;
using System.Collections.Generic;

public static class DelegateExtensions
{
	public static List<string> ToStringList(this Delegate[] invocationList)
	{
		List<string> list = new List<string>();
		if (invocationList != null)
		{
			foreach (Delegate @delegate in invocationList)
			{
				string name = @delegate.Method.Name;
				string text = ((@delegate.Target != null) ? @delegate.Target.GetType().FullName : "Static Method");
				list.Add(text + "." + name);
			}
		}
		return list;
	}

	public static string ToText(this Delegate[] invocationList)
	{
		return string.Join(", ", invocationList.ToStringList());
	}
}

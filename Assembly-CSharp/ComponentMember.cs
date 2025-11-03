using System;

public class ComponentMember
{
	public string Name { get; }

	public string Value
	{
		get
		{
			return this.getValue();
		}
	}

	public bool IsStarred { get; }

	public string Color { get; }

	public ComponentMember(string name, Func<string> getValue, bool isStarred, string color)
	{
		this.Name = name;
		this.getValue = getValue;
		this.IsStarred = isStarred;
		this.Color = color;
	}

	private Func<string> getValue;

	public string computedPrefix;

	public string computedSuffix;
}

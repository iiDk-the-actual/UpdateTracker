using System;

public class InvalidType : ProxyType
{
	public override string Name
	{
		get
		{
			return this._self.Name;
		}
	}

	public override string FullName
	{
		get
		{
			return this._self.FullName;
		}
	}

	public override string AssemblyQualifiedName
	{
		get
		{
			return this._self.AssemblyQualifiedName;
		}
	}

	private Type _self = typeof(InvalidType);
}

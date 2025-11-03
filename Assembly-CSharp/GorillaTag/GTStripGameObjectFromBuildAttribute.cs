using System;

namespace GorillaTag
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class GTStripGameObjectFromBuildAttribute : Attribute
	{
		public string Condition { get; }

		public GTStripGameObjectFromBuildAttribute(string condition = "")
		{
			this.Condition = condition;
		}
	}
}

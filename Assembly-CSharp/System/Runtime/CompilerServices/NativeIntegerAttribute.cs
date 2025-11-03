using System;
using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NativeIntegerAttribute : Attribute
	{
		public NativeIntegerAttribute()
		{
			this.TransformFlags = new bool[] { true };
		}

		public NativeIntegerAttribute(bool[] A_1)
		{
			this.TransformFlags = A_1;
		}

		public readonly bool[] TransformFlags;
	}
}

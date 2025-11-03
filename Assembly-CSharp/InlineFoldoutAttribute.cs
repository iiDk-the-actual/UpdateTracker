using System;
using System.Diagnostics;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.All)]
public class InlineFoldoutAttribute : Attribute
{
}

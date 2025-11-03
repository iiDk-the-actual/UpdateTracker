using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[Conditional("UNITY_EDITOR")]
[IncludeMyAttributes]
public class HideAlwaysAttribute : Attribute
{
}

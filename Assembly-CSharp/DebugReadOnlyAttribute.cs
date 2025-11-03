using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[IncludeMyAttributes]
[Conditional("UNITY_EDITOR")]
public class DebugReadOnlyAttribute : Attribute
{
}

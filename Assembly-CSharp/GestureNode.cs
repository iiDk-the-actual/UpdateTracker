using System;
using UnityEngine;

[Serializable]
public class GestureNode
{
	public bool track;

	public GestureHandState state;

	public GestureDigitFlexion flexion;

	public GestureAlignment alignment;

	[Space]
	public GestureNodeFlags flags;
}

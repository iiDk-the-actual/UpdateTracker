using System;
using UnityEngine;

public interface IRangedVariable<T> : IVariable<T>, IVariable
{
	T Min { get; set; }

	T Max { get; set; }

	T Range { get; }

	AnimationCurve Curve { get; }
}

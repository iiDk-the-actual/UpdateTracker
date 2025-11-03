using System;
using UnityEngine;

public abstract class HandTapBehaviour : MonoBehaviour
{
	internal abstract void OnTap(HandEffectContext handContext);
}

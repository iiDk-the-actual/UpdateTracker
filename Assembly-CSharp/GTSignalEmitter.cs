using System;
using UnityEngine;

public class GTSignalEmitter : MonoBehaviour
{
	public virtual void Emit()
	{
		GTSignal.Emit(this.emitMode, this.signal, Array.Empty<object>());
	}

	public virtual void Emit(int targetActor)
	{
		GTSignal.Emit(targetActor, this.signal, Array.Empty<object>());
	}

	public virtual void Emit(params object[] data)
	{
		GTSignal.Emit(this.emitMode, this.signal, data);
	}

	[Space]
	public GTSignalID signal;

	public GTSignal.EmitMode emitMode;
}

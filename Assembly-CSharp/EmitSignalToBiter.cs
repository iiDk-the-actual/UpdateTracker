using System;
using UnityEngine;

public class EmitSignalToBiter : GTSignalEmitter
{
	public override void Emit()
	{
		if (this.onEdibleState == EmitSignalToBiter.EdibleState.None)
		{
			return;
		}
		if (!this.targetEdible)
		{
			return;
		}
		if (this.targetEdible.lastBiterActorID == -1)
		{
			return;
		}
		TransferrableObject.ItemStates itemState = this.targetEdible.itemState;
		if (itemState - TransferrableObject.ItemStates.State0 <= 1 || itemState == TransferrableObject.ItemStates.State2 || itemState == TransferrableObject.ItemStates.State3)
		{
			int num = (int)itemState;
			if ((this.onEdibleState & (EmitSignalToBiter.EdibleState)num) == (EmitSignalToBiter.EdibleState)num)
			{
				GTSignal.Emit(this.targetEdible.lastBiterActorID, this.signal, Array.Empty<object>());
			}
		}
	}

	public override void Emit(int targetActor)
	{
	}

	public override void Emit(params object[] data)
	{
	}

	[Space]
	public EdibleHoldable targetEdible;

	[Space]
	[SerializeField]
	private EmitSignalToBiter.EdibleState onEdibleState;

	[Flags]
	private enum EdibleState
	{
		None = 0,
		State0 = 1,
		State1 = 2,
		State2 = 4,
		State3 = 8
	}
}

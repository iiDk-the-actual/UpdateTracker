using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.Events;

public class RadioButtonGroupWearable : MonoBehaviour, ISpawnable
{
	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	private void Start()
	{
		this.stateBitsWriteInfo = VRRig.WearablePackedStatesBitWriteInfos[(int)this.assignedSlot];
		if (!this.ownerRig.isLocal)
		{
			GorillaPressableButton[] array = this.buttons;
			for (int i = 0; i < array.Length; i++)
			{
				Collider component = array[i].GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
		}
	}

	private void OnEnable()
	{
		this.SharedRefreshState();
	}

	private int GetCurrentState()
	{
		return GTBitOps.ReadBits(this.ownerRig.WearablePackedStates, this.stateBitsWriteInfo.index, this.stateBitsWriteInfo.valueMask);
	}

	private void Update()
	{
		if (this.ownerRig.isLocal)
		{
			return;
		}
		if (this.lastReportedState != this.GetCurrentState())
		{
			this.SharedRefreshState();
		}
	}

	public void SharedRefreshState()
	{
		int currentState = this.GetCurrentState();
		int num = (this.AllowSelectNone ? (currentState - 1) : currentState);
		for (int i = 0; i < this.buttons.Length; i++)
		{
			this.buttons[i].isOn = num == i;
			this.buttons[i].UpdateColor();
		}
		if (this.lastReportedState != currentState)
		{
			this.lastReportedState = currentState;
			this.OnSelectionChanged.Invoke(currentState);
		}
	}

	public void OnPress(GorillaPressableButton button)
	{
		int currentState = this.GetCurrentState();
		int num = Array.IndexOf<GorillaPressableButton>(this.buttons, button);
		if (this.AllowSelectNone)
		{
			num++;
		}
		int num2 = num;
		if (this.AllowSelectNone && num == currentState)
		{
			num2 = 0;
		}
		this.ownerRig.WearablePackedStates = GTBitOps.WriteBits(this.ownerRig.WearablePackedStates, this.stateBitsWriteInfo, num2);
		this.SharedRefreshState();
	}

	public void OnSpawn(VRRig rig)
	{
		this.ownerRig = rig;
	}

	public void OnDespawn()
	{
	}

	[SerializeField]
	private bool AllowSelectNone = true;

	[SerializeField]
	private GorillaPressableButton[] buttons;

	[SerializeField]
	private UnityEvent<int> OnSelectionChanged;

	[Tooltip("This is to determine what bit to change in VRRig.WearablesPackedStates.")]
	[SerializeField]
	private VRRig.WearablePackedStateSlots assignedSlot = VRRig.WearablePackedStateSlots.Pants1;

	private int lastReportedState;

	private VRRig ownerRig;

	private GTBitOps.BitWriteInfo stateBitsWriteInfo;
}

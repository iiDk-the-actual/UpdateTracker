using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

public class CompositeTriggerEvents : MonoBehaviour
{
	private Dictionary<Collider, int> CollderMasks
	{
		get
		{
			return this.overlapMask;
		}
	}

	public event CompositeTriggerEvents.TriggerEvent CompositeTriggerEnter;

	public event CompositeTriggerEvents.TriggerEvent CompositeTriggerExit;

	private void Awake()
	{
		if (this.individualTriggerColliders.Count > 32)
		{
			Debug.LogError("The max number of triggers was exceeded in this composite trigger event sender on GameObject: " + base.gameObject.name + ".");
		}
		for (int i = 0; i < this.individualTriggerColliders.Count; i++)
		{
			TriggerEventNotifier triggerEventNotifier = this.individualTriggerColliders[i].gameObject.AddComponent<TriggerEventNotifier>();
			triggerEventNotifier.maskIndex = i;
			triggerEventNotifier.TriggerEnterEvent += this.TriggerEnterReceiver;
			triggerEventNotifier.TriggerExitEvent += this.TriggerExitReceiver;
			this.triggerEventNotifiers.Add(triggerEventNotifier);
		}
	}

	public void AddCollider(Collider colliderToAdd)
	{
		if (this.individualTriggerColliders.Count >= 32)
		{
			Debug.LogError("The max number of triggers are already present in this composite trigger event sender on GameObject: " + base.gameObject.name + ".");
			return;
		}
		this.individualTriggerColliders.Add(colliderToAdd);
		TriggerEventNotifier triggerEventNotifier = colliderToAdd.gameObject.AddComponent<TriggerEventNotifier>();
		triggerEventNotifier.maskIndex = this.GetNextMaskIndex();
		triggerEventNotifier.TriggerEnterEvent += this.TriggerEnterReceiver;
		triggerEventNotifier.TriggerExitEvent += this.TriggerExitReceiver;
		this.triggerEventNotifiers.Add(triggerEventNotifier);
		this.triggerEventNotifiers.Sort((TriggerEventNotifier a, TriggerEventNotifier b) => a.maskIndex.CompareTo(b.maskIndex));
	}

	public void RemoveCollider(Collider colliderToRemove)
	{
		TriggerEventNotifier component = colliderToRemove.gameObject.GetComponent<TriggerEventNotifier>();
		if (component.IsNotNull())
		{
			foreach (KeyValuePair<Collider, int> keyValuePair in new Dictionary<Collider, int>(this.overlapMask))
			{
				this.TriggerExitReceiver(component, keyValuePair.Key);
			}
			component.maskIndex = -1;
			component.TriggerEnterEvent -= this.TriggerEnterReceiver;
			component.TriggerExitEvent -= this.TriggerExitReceiver;
			this.triggerEventNotifiers.Remove(component);
		}
		this.individualTriggerColliders.Remove(colliderToRemove);
	}

	public void ResetColliders(bool sendExitEvent = true)
	{
		this.individualTriggerColliders.Clear();
		for (int i = this.triggerEventNotifiers.Count - 1; i >= 0; i--)
		{
			if (this.triggerEventNotifiers[i].IsNull())
			{
				this.triggerEventNotifiers.RemoveAt(i);
			}
			else
			{
				this.triggerEventNotifiers[i].maskIndex = -1;
				this.triggerEventNotifiers[i].TriggerEnterEvent -= this.TriggerEnterReceiver;
				this.triggerEventNotifiers[i].TriggerExitEvent -= this.TriggerExitReceiver;
				this.triggerEventNotifiers.RemoveAt(i);
			}
		}
		if (sendExitEvent)
		{
			foreach (KeyValuePair<Collider, int> keyValuePair in this.overlapMask)
			{
				CompositeTriggerEvents.TriggerEvent compositeTriggerExit = this.CompositeTriggerExit;
				if (compositeTriggerExit != null)
				{
					compositeTriggerExit(keyValuePair.Key);
				}
			}
		}
		this.overlapMask.Clear();
	}

	public int GetNumColliders()
	{
		return this.individualTriggerColliders.Count;
	}

	public int GetNextMaskIndex()
	{
		if (this.individualTriggerColliders.Count >= 32)
		{
			Debug.LogError("The max number of triggers are already present in this composite trigger event sender on GameObject: " + base.gameObject.name + ".");
			return -1;
		}
		int num = 0;
		int num2 = 0;
		while (num2 < this.triggerEventNotifiers.Count && this.triggerEventNotifiers[num2].maskIndex == num)
		{
			num++;
			num2++;
		}
		return num;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < this.triggerEventNotifiers.Count; i++)
		{
			if (this.triggerEventNotifiers[i] != null)
			{
				this.triggerEventNotifiers[i].TriggerEnterEvent -= this.TriggerEnterReceiver;
				this.triggerEventNotifiers[i].TriggerExitEvent -= this.TriggerExitReceiver;
			}
		}
	}

	public void TriggerEnterReceiver(TriggerEventNotifier notifier, Collider other)
	{
		int num;
		if (this.overlapMask.TryGetValue(other, out num))
		{
			num = this.SetMaskIndexTrue(num, notifier.maskIndex);
			this.overlapMask[other] = num;
			return;
		}
		int num2 = this.SetMaskIndexTrue(0, notifier.maskIndex);
		this.overlapMask.Add(other, num2);
		CompositeTriggerEvents.TriggerEvent compositeTriggerEnter = this.CompositeTriggerEnter;
		if (compositeTriggerEnter == null)
		{
			return;
		}
		compositeTriggerEnter(other);
	}

	public void TriggerExitReceiver(TriggerEventNotifier notifier, Collider other)
	{
		int num;
		if (this.overlapMask.TryGetValue(other, out num))
		{
			num = this.SetMaskIndexFalse(num, notifier.maskIndex);
			if (num == 0)
			{
				this.overlapMask.Remove(other);
				CompositeTriggerEvents.TriggerEvent compositeTriggerExit = this.CompositeTriggerExit;
				if (compositeTriggerExit == null)
				{
					return;
				}
				compositeTriggerExit(other);
				return;
			}
			else
			{
				this.overlapMask[other] = num;
			}
		}
	}

	public void ResetColliderMask(Collider other)
	{
		int num;
		if (this.overlapMask.TryGetValue(other, out num))
		{
			if (num != 0)
			{
				CompositeTriggerEvents.TriggerEvent compositeTriggerExit = this.CompositeTriggerExit;
				if (compositeTriggerExit != null)
				{
					compositeTriggerExit(other);
				}
			}
			this.overlapMask.Remove(other);
		}
	}

	public void CompositeTriggerEnterReceiver(Collider other)
	{
		CompositeTriggerEvents.TriggerEvent compositeTriggerEnter = this.CompositeTriggerEnter;
		if (compositeTriggerEnter == null)
		{
			return;
		}
		compositeTriggerEnter(other);
	}

	public void CompositeTriggerExitReceiver(Collider other)
	{
		CompositeTriggerEvents.TriggerEvent compositeTriggerExit = this.CompositeTriggerExit;
		if (compositeTriggerExit == null)
		{
			return;
		}
		compositeTriggerExit(other);
	}

	private bool TestMaskIndex(int mask, int index)
	{
		return (mask & (1 << index)) != 0;
	}

	private int SetMaskIndexTrue(int mask, int index)
	{
		return mask | (1 << index);
	}

	private int SetMaskIndexFalse(int mask, int index)
	{
		return mask & ~(1 << index);
	}

	private string MaskToString(int mask)
	{
		string text = "";
		for (int i = 31; i >= 0; i--)
		{
			text += (this.TestMaskIndex(mask, i) ? "1" : "0");
		}
		return text;
	}

	[SerializeField]
	private List<Collider> individualTriggerColliders = new List<Collider>();

	private List<TriggerEventNotifier> triggerEventNotifiers = new List<TriggerEventNotifier>();

	private Dictionary<Collider, int> overlapMask = new Dictionary<Collider, int>();

	public delegate void TriggerEvent(Collider collider);
}

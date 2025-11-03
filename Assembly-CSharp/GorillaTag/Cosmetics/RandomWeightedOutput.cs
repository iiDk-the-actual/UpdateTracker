using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(NetworkedRandomProvider))]
	public class RandomWeightedOutput : MonoBehaviour
	{
		private void Awake()
		{
			if (this.networkProvider == null)
			{
				this.networkProvider = base.GetComponentInParent<NetworkedRandomProvider>();
			}
		}

		public void PickNextRandom()
		{
			int deterministicPickIndex = this.GetDeterministicPickIndex();
			if (deterministicPickIndex >= 0)
			{
				UnityEvent onPick = this.outputs[deterministicPickIndex].onPick;
				if (onPick != null)
				{
					onPick.Invoke();
				}
				UnityEvent<int> unityEvent = this.onAnyPick;
				if (unityEvent != null)
				{
					unityEvent.Invoke(deterministicPickIndex);
				}
				if (this.debugLog)
				{
					Debug.Log(string.Format("[RandomWeightedOutput] Picked '{0}' (idx={1})", this.outputs[deterministicPickIndex].name, deterministicPickIndex));
				}
			}
		}

		private int GetDeterministicPickIndex()
		{
			if (this.networkProvider == null)
			{
				return -1;
			}
			List<int> list = new List<int>(this.outputs.Count);
			for (int i = 0; i < this.outputs.Count; i++)
			{
				RandomWeightedOutput.WeightedOutput weightedOutput = this.outputs[i];
				if (weightedOutput != null && weightedOutput.enabled && weightedOutput.weight > 0f)
				{
					list.Add(i);
				}
			}
			if (list.Count == 0)
			{
				return -1;
			}
			double num = 0.0;
			foreach (int num2 in list)
			{
				num += (double)this.outputs[num2].weight;
			}
			if (num <= 0.0)
			{
				return list[0];
			}
			double num3 = (double)this.networkProvider.GetSelectedAsFloat() * num;
			double num4 = 0.0;
			for (int j = 0; j < list.Count; j++)
			{
				int num5 = list[j];
				num4 += (double)this.outputs[num5].weight;
				if (num3 < num4)
				{
					return num5;
				}
			}
			List<int> list2 = list;
			return list2[list2.Count - 1];
		}

		[Header("Network Provider")]
		[Tooltip("For best result, pick Float01 or Double01 as the output mode in your NetworkedRandomProvider")]
		[SerializeField]
		private NetworkedRandomProvider networkProvider;

		[Header("Weighted Outputs")]
		[SerializeField]
		private List<RandomWeightedOutput.WeightedOutput> outputs = new List<RandomWeightedOutput.WeightedOutput>();

		[Header("Event")]
		[SerializeField]
		public UnityEvent<int> onAnyPick = new UnityEvent<int>();

		[SerializeField]
		private bool debugLog;

		[Serializable]
		public class WeightedOutput
		{
			[SerializeField]
			public string name = "Event";

			[SerializeField]
			[Range(0f, 100f)]
			public float weight = 1f;

			[SerializeField]
			public bool enabled = true;

			[SerializeField]
			public UnityEvent onPick = new UnityEvent();
		}
	}
}

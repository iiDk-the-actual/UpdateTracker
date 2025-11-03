using System;
using System.Collections;
using UnityEngine;

namespace UnityChan
{
	public class AutoBlink : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
			this.ResetTimer();
			base.StartCoroutine("RandomChange");
		}

		private void ResetTimer()
		{
			this.timeRemining = this.timeBlink;
			this.timerStarted = false;
		}

		private void Update()
		{
			if (!this.timerStarted)
			{
				this.eyeStatus = AutoBlink.Status.Close;
				this.timerStarted = true;
			}
			if (this.timerStarted)
			{
				this.timeRemining -= Time.deltaTime;
				if (this.timeRemining <= 0f)
				{
					this.eyeStatus = AutoBlink.Status.Open;
					this.ResetTimer();
					return;
				}
				if (this.timeRemining <= this.timeBlink * 0.3f)
				{
					this.eyeStatus = AutoBlink.Status.HalfClose;
				}
			}
		}

		private void LateUpdate()
		{
			if (this.isActive && this.isBlink)
			{
				switch (this.eyeStatus)
				{
				case AutoBlink.Status.Close:
					this.SetCloseEyes();
					return;
				case AutoBlink.Status.HalfClose:
					this.SetHalfCloseEyes();
					return;
				case AutoBlink.Status.Open:
					this.SetOpenEyes();
					this.isBlink = false;
					break;
				default:
					return;
				}
			}
		}

		private void SetCloseEyes()
		{
			this.ref_SMR_EYE_DEF.SetBlendShapeWeight(6, this.ratio_Close);
			this.ref_SMR_EL_DEF.SetBlendShapeWeight(6, this.ratio_Close);
		}

		private void SetHalfCloseEyes()
		{
			this.ref_SMR_EYE_DEF.SetBlendShapeWeight(6, this.ratio_HalfClose);
			this.ref_SMR_EL_DEF.SetBlendShapeWeight(6, this.ratio_HalfClose);
		}

		private void SetOpenEyes()
		{
			this.ref_SMR_EYE_DEF.SetBlendShapeWeight(6, this.ratio_Open);
			this.ref_SMR_EL_DEF.SetBlendShapeWeight(6, this.ratio_Open);
		}

		private IEnumerator RandomChange()
		{
			for (;;)
			{
				float num = Random.Range(0f, 1f);
				if (!this.isBlink && num > this.threshold)
				{
					this.isBlink = true;
				}
				yield return new WaitForSeconds(this.interval);
			}
			yield break;
		}

		public bool isActive = true;

		public SkinnedMeshRenderer ref_SMR_EYE_DEF;

		public SkinnedMeshRenderer ref_SMR_EL_DEF;

		public float ratio_Close = 85f;

		public float ratio_HalfClose = 20f;

		[HideInInspector]
		public float ratio_Open;

		private bool timerStarted;

		private bool isBlink;

		public float timeBlink = 0.4f;

		private float timeRemining;

		public float threshold = 0.3f;

		public float interval = 3f;

		private AutoBlink.Status eyeStatus;

		private enum Status
		{
			Close,
			HalfClose,
			Open
		}
	}
}

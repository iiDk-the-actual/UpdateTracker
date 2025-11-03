using System;
using emotitron.Compression;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag
{
	public class DrinkableHoldable : TransferrableObject
	{
		internal override void OnEnable()
		{
			base.OnEnable();
			base.enabled = this.containerLiquid != null;
			this.itemState = (TransferrableObject.ItemStates)DrinkableHoldable.PackValues(this.sipSoundCooldown, this.containerLiquid.fillAmount, this.coolingDown);
			this.myByteArray = new byte[32];
		}

		protected override void LateUpdateLocal()
		{
			if (!this.containerLiquid.isActiveAndEnabled || !GorillaParent.hasInstance || !GorillaComputer.hasInstance)
			{
				base.LateUpdateLocal();
				return;
			}
			float num = (float)((GorillaComputer.instance.startupMillis + (long)Time.realtimeSinceStartup * 1000L) % 259200000L) / 1000f;
			if (Mathf.Abs(num - this.lastTimeSipSoundPlayed) > 129600f)
			{
				this.lastTimeSipSoundPlayed = num;
			}
			float num2 = this.sipRadius * this.sipRadius;
			bool flag = (GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.TransformPoint(this.headToMouthOffset) - this.containerLiquid.cupTopWorldPos).sqrMagnitude < num2;
			if (!flag)
			{
				foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
				{
					if (!vrrig.isOfflineVRRig)
					{
						if (flag || vrrig.head == null)
						{
							break;
						}
						if (vrrig.head.rigTarget == null)
						{
							break;
						}
						flag = (vrrig.head.rigTarget.transform.TransformPoint(this.headToMouthOffset) - this.containerLiquid.cupTopWorldPos).sqrMagnitude < num2;
					}
				}
			}
			if (flag)
			{
				this.containerLiquid.fillAmount = Mathf.Clamp01(this.containerLiquid.fillAmount - this.sipRate * Time.deltaTime);
				if (num > this.lastTimeSipSoundPlayed + this.sipSoundCooldown)
				{
					if (!this.wasSipping)
					{
						this.lastTimeSipSoundPlayed = num;
						this.coolingDown = true;
					}
				}
				else
				{
					this.coolingDown = false;
				}
			}
			this.wasSipping = flag;
			this.itemState = (TransferrableObject.ItemStates)DrinkableHoldable.PackValues(this.lastTimeSipSoundPlayed, this.containerLiquid.fillAmount, this.coolingDown);
			base.LateUpdateLocal();
		}

		protected override void LateUpdateReplicated()
		{
			base.LateUpdateReplicated();
			int itemState = (int)this.itemState;
			this.UnpackValuesNonstatic(in itemState, out this.lastTimeSipSoundPlayed, out this.containerLiquid.fillAmount, out this.coolingDown);
		}

		protected override void LateUpdateShared()
		{
			base.LateUpdateShared();
			if (this.coolingDown && !this.wasCoolingDown)
			{
				this.sipSoundBankPlayer.Play();
			}
			this.wasCoolingDown = this.coolingDown;
		}

		private static int PackValues(float cooldownStartTime, float fillAmount, bool coolingDown)
		{
			byte[] array = new byte[32];
			int num = 0;
			array.WriteBool(coolingDown, ref num);
			array.Write((ulong)((double)cooldownStartTime * 100.0), ref num, 25);
			array.Write((ulong)((double)fillAmount * 63.0), ref num, 6);
			return BitConverter.ToInt32(array, 0);
		}

		private void UnpackValuesNonstatic(in int packed, out float cooldownStartTime, out float fillAmount, out bool coolingDown)
		{
			DrinkableHoldable.GetBytes(packed, ref this.myByteArray);
			int num = 0;
			coolingDown = this.myByteArray.ReadBool(ref num);
			cooldownStartTime = (float)(this.myByteArray.Read(ref num, 25) / 100.0);
			fillAmount = this.myByteArray.Read(ref num, 6) / 63f;
		}

		public static void GetBytes(int value, ref byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)((value >> 8 * i) & 255);
			}
		}

		private static void UnpackValuesStatic(in int packed, out float cooldownStartTime, out float fillAmount, out bool coolingDown)
		{
			byte[] bytes = BitConverter.GetBytes(packed);
			int num = 0;
			coolingDown = bytes.ReadBool(ref num);
			cooldownStartTime = (float)(bytes.Read(ref num, 25) / 100.0);
			fillAmount = bytes.Read(ref num, 6) / 63f;
		}

		[AssignInCorePrefab]
		public ContainerLiquid containerLiquid;

		[AssignInCorePrefab]
		[SoundBankInfo]
		public SoundBankPlayer sipSoundBankPlayer;

		[AssignInCorePrefab]
		public float sipRate = 0.1f;

		[AssignInCorePrefab]
		public float sipSoundCooldown = 0.5f;

		[AssignInCorePrefab]
		public Vector3 headToMouthOffset = new Vector3(0f, 0.0208f, 0.171f);

		[AssignInCorePrefab]
		public float sipRadius = 0.15f;

		private float lastTimeSipSoundPlayed;

		private bool wasSipping;

		private bool coolingDown;

		private bool wasCoolingDown;

		private byte[] myByteArray;
	}
}

using System;
using System.Text;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class NetworkedRandomProvider : MonoBehaviour
	{
		private void Awake()
		{
			if (this.parentTransferable == null)
			{
				this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
			}
		}

		private void OnEnable()
		{
			this.EnsureOwner();
		}

		private void OnValidate()
		{
			if (this.windowSeconds < 0.01f)
			{
				this.windowSeconds = 0.01f;
			}
			if (this.floatRange.y < this.floatRange.x)
			{
				ref float ptr = ref this.floatRange.x;
				float y = this.floatRange.y;
				float x = this.floatRange.x;
				ptr = y;
				this.floatRange.y = x;
			}
			if (this.doubleMax < this.doubleMin)
			{
				double num = this.doubleMax;
				double num2 = this.doubleMin;
				this.doubleMin = num;
				this.doubleMax = num2;
			}
		}

		private void Update()
		{
			long num = (long)Math.Floor(this.GetSharedTime() / (double)this.windowSeconds);
			this.debugWindow = num;
		}

		private bool ShowFloatRange()
		{
			return this.outputMode == NetworkedRandomProvider.OutputMode.FloatRange;
		}

		private bool ShowDoubleRange()
		{
			return this.outputMode == NetworkedRandomProvider.OutputMode.DoubleRange;
		}

		private long GetWindowIndex()
		{
			return (long)Math.Floor(this.GetSharedTime() / (double)this.windowSeconds);
		}

		private double GetSharedTime()
		{
			if (PhotonNetwork.InRoom)
			{
				return PhotonNetwork.Time;
			}
			return (double)Time.realtimeSinceStartup;
		}

		private static ulong Mix64(ulong x)
		{
			x += 11400714819323198485UL;
			x = (x ^ (x >> 30)) * 13787848793156543929UL;
			x = (x ^ (x >> 27)) * 10723151780598845931UL;
			x ^= x >> 31;
			return x;
		}

		private static ulong BuildSeed(long windowIndex, int ownerId, int objectSalt, uint roomSalt)
		{
			return (ulong)(windowIndex ^ (long)((long)((ulong)ownerId) << 32) ^ (long)((ulong)objectSalt * 11400714819323198485UL) ^ (long)((ulong)roomSalt * 15183679468541472403UL));
		}

		private static float UnitFloat01(long windowIndex, int ownerId, int objectSalt, uint roomSalt)
		{
			return (uint)(NetworkedRandomProvider.Mix64(NetworkedRandomProvider.BuildSeed(windowIndex, ownerId, objectSalt, roomSalt)) >> 40) * 5.9604645E-08f;
		}

		private static double UnitDouble01(long windowIndex, int ownerId, int objectSalt, uint roomSalt)
		{
			return (NetworkedRandomProvider.Mix64(NetworkedRandomProvider.BuildSeed(windowIndex, ownerId, objectSalt, roomSalt)) >> 11) * 1.1102230246251565E-16;
		}

		public float NextFloat01()
		{
			this.EnsureOwner();
			long windowIndex = this.GetWindowIndex();
			uint num;
			if (!this.includeRoomNameInSeed)
			{
				num = 0U;
			}
			else
			{
				string text;
				if (!PhotonNetwork.InRoom)
				{
					text = "no_room";
				}
				else
				{
					Room currentRoom = PhotonNetwork.CurrentRoom;
					text = ((currentRoom != null) ? currentRoom.Name : null) ?? "no_room";
				}
				num = NetworkedRandomProvider.StableHash(text);
			}
			uint num2 = num;
			float num3 = NetworkedRandomProvider.UnitFloat01(windowIndex, this.OwnerID, this.objectSalt, num2);
			this.debugResult = num3;
			return num3;
		}

		public float NextFloat(float min, float max)
		{
			float num = this.NextFloat01();
			if (max < min)
			{
				float num2 = max;
				float num3 = min;
				min = num2;
				max = num3;
			}
			return Mathf.Lerp(min, max, num);
		}

		public double NextDouble(double min, double max)
		{
			this.EnsureOwner();
			long windowIndex = this.GetWindowIndex();
			uint num;
			if (!this.includeRoomNameInSeed)
			{
				num = 0U;
			}
			else
			{
				string text;
				if (!PhotonNetwork.InRoom)
				{
					text = "no_room";
				}
				else
				{
					Room currentRoom = PhotonNetwork.CurrentRoom;
					text = ((currentRoom != null) ? currentRoom.Name : null) ?? "no_room";
				}
				num = NetworkedRandomProvider.StableHash(text);
			}
			uint num2 = num;
			double num3 = NetworkedRandomProvider.UnitDouble01(windowIndex, this.OwnerID, this.objectSalt, num2);
			if (max < min)
			{
				double num4 = max;
				double num5 = min;
				min = num4;
				max = num5;
			}
			double num6 = min + (max - min) * num3;
			this.debugResult = (float)num6;
			return num6;
		}

		public float GetSelectedAsFloat()
		{
			switch (this.outputMode)
			{
			default:
				return this.NextFloat01();
			case NetworkedRandomProvider.OutputMode.Double01:
				return (float)this.NextDouble(0.0, 1.0);
			case NetworkedRandomProvider.OutputMode.FloatRange:
				return this.NextFloat(this.floatRange.x, this.floatRange.y);
			case NetworkedRandomProvider.OutputMode.DoubleRange:
				return (float)this.NextDouble(this.doubleMin, this.doubleMax);
			}
		}

		public double GetSelectedAsDouble()
		{
			switch (this.outputMode)
			{
			default:
				return (double)this.NextFloat01();
			case NetworkedRandomProvider.OutputMode.Double01:
				return this.NextDouble(0.0, 1.0);
			case NetworkedRandomProvider.OutputMode.FloatRange:
				return (double)this.NextFloat(this.floatRange.x, this.floatRange.y);
			case NetworkedRandomProvider.OutputMode.DoubleRange:
				return this.NextDouble(this.doubleMin, this.doubleMax);
			}
		}

		private static uint StableHash(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return 0U;
			}
			uint num = 2166136261U;
			for (int i = 0; i < s.Length; i++)
			{
				num ^= (uint)s[i];
				num *= 16777619U;
			}
			return num;
		}

		private void EnsureOwner()
		{
			if (this.OwnerID == 0)
			{
				this.TrySetID();
			}
		}

		private void TrySetID()
		{
			if (this.parentTransferable == null)
			{
				string name = base.gameObject.scene.name;
				string text = "/";
				string hierarchyPath = NetworkedRandomProvider.GetHierarchyPath(base.transform);
				Type type = base.GetType();
				string text2 = name + text + hierarchyPath + ((type != null) ? type.ToString() : null);
				this.OwnerID = text2.GetStaticHash();
				return;
			}
			if (this.parentTransferable.IsLocalObject())
			{
				PlayFabAuthenticator instance = PlayFabAuthenticator.instance;
				if (instance != null)
				{
					string playFabPlayerId = instance.GetPlayFabPlayerId();
					Type type2 = base.GetType();
					this.OwnerID = (playFabPlayerId + ((type2 != null) ? type2.ToString() : null)).GetStaticHash();
					return;
				}
			}
			else if (this.parentTransferable.targetRig != null && this.parentTransferable.targetRig.creator != null)
			{
				string userId = this.parentTransferable.targetRig.creator.UserId;
				Type type3 = base.GetType();
				this.OwnerID = (userId + ((type3 != null) ? type3.ToString() : null)).GetStaticHash();
			}
		}

		private static string GetHierarchyPath(Transform t)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (t != null)
			{
				stringBuilder.Insert(0, "/" + t.name + "#" + t.GetSiblingIndex().ToString());
				t = t.parent;
			}
			return stringBuilder.ToString();
		}

		[Header("Time Granularity")]
		[Min(0.01f)]
		[Tooltip("Length of the time bucket (seconds). Within a bucket the pick is fixed; re-rolls next bucket.")]
		[SerializeField]
		private float windowSeconds = 1f;

		[Tooltip("Mix room name into seed so different rooms never collide.")]
		[SerializeField]
		private bool includeRoomNameInSeed = true;

		[Tooltip("Optional - If multiple component live on the same cosmetic, use different salts.")]
		[SerializeField]
		private int objectSalt;

		[Header("Output")]
		[SerializeField]
		private NetworkedRandomProvider.OutputMode outputMode;

		[SerializeField]
		private Vector2 floatRange = new Vector2(0f, 1f);

		[SerializeField]
		private double doubleMin;

		[SerializeField]
		private double doubleMax = 1.0;

		private TransferrableObject parentTransferable;

		private int OwnerID;

		[Header("Debug")]
		[SerializeField]
		private long debugWindow;

		[SerializeField]
		private float debugResult;

		public enum OutputMode
		{
			Float01,
			Double01,
			FloatRange,
			DoubleRange
		}
	}
}

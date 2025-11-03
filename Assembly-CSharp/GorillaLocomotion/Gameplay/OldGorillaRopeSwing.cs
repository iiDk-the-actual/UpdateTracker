using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaLocomotion.Gameplay
{
	public class OldGorillaRopeSwing : MonoBehaviourPun
	{
		public bool isIdle { get; private set; }

		private void Awake()
		{
			this.SetIsIdle(true);
		}

		private void OnDisable()
		{
			if (!this.isIdle)
			{
				this.SetIsIdle(true);
			}
		}

		private void Update()
		{
			if (this.localPlayerOn && this.localGrabbedRigid)
			{
				float magnitude = this.localGrabbedRigid.linearVelocity.magnitude;
				if (magnitude > 2.5f && !this.ropeCreakSFX.isPlaying && Mathf.RoundToInt(Time.time) % 5 == 0)
				{
					this.ropeCreakSFX.GTPlay();
				}
				float num = MathUtils.Linear(magnitude, 0f, 10f, -0.07f, 0.5f);
				if (num > 0f)
				{
					GorillaTagger.Instance.DoVibration(this.localPlayerXRNode, num, Time.deltaTime);
				}
			}
			if (!this.isIdle)
			{
				if (!this.localPlayerOn && this.remotePlayers.Count == 0)
				{
					foreach (Rigidbody rigidbody in this.bones)
					{
						float magnitude2 = rigidbody.linearVelocity.magnitude;
						float num2 = Time.deltaTime * this.settings.frictionWhenNotHeld;
						if (num2 < magnitude2 - 0.1f)
						{
							rigidbody.linearVelocity = Vector3.MoveTowards(rigidbody.linearVelocity, Vector3.zero, num2);
						}
					}
				}
				bool flag = false;
				for (int j = 0; j < this.bones.Length; j++)
				{
					if (this.bones[j].linearVelocity.magnitude > 0.1f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.potentialIdleTimer += Time.deltaTime;
				}
				else
				{
					this.potentialIdleTimer = 0f;
				}
				if (this.potentialIdleTimer >= 2f)
				{
					this.SetIsIdle(true);
					this.potentialIdleTimer = 0f;
				}
			}
		}

		private void SetIsIdle(bool idle)
		{
			this.isIdle = idle;
			this.ToggleIsKinematic(idle);
			if (idle)
			{
				for (int i = 0; i < this.bones.Length; i++)
				{
					this.bones[i].linearVelocity = Vector3.zero;
					this.bones[i].angularVelocity = Vector3.zero;
					this.bones[i].transform.localRotation = Quaternion.identity;
				}
			}
		}

		private void ToggleIsKinematic(bool kinematic)
		{
			for (int i = 0; i < this.bones.Length; i++)
			{
				this.bones[i].isKinematic = kinematic;
				if (kinematic)
				{
					this.bones[i].interpolation = RigidbodyInterpolation.None;
				}
				else
				{
					this.bones[i].interpolation = RigidbodyInterpolation.Interpolate;
				}
			}
		}

		public Rigidbody GetBone(int index)
		{
			if (index >= this.bones.Length)
			{
				return this.bones.Last<Rigidbody>();
			}
			return this.bones[index];
		}

		public int GetBoneIndex(Rigidbody r)
		{
			for (int i = 0; i < this.bones.Length; i++)
			{
				if (this.bones[i] == r)
				{
					return i;
				}
			}
			return this.bones.Length - 1;
		}

		public void AttachLocalPlayer(XRNode xrNode, Rigidbody rigid, Vector3 offset, Vector3 velocity)
		{
			int boneIndex = this.GetBoneIndex(rigid);
			velocity *= this.settings.inheritVelocityMultiplier;
			if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
			{
				GorillaTagger.Instance.offlineVRRig.grabbedRopeIndex = base.photonView.ViewID;
				GorillaTagger.Instance.offlineVRRig.grabbedRopeBoneIndex = boneIndex;
				GorillaTagger.Instance.offlineVRRig.grabbedRopeIsLeft = xrNode == XRNode.LeftHand;
				GorillaTagger.Instance.offlineVRRig.grabbedRopeOffset = offset;
			}
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			if (this.remotePlayers.Count <= 0)
			{
				foreach (Rigidbody rigidbody in this.bones)
				{
					list.Add(rigidbody.transform.localEulerAngles);
					list2.Add(rigidbody.linearVelocity);
				}
			}
			if (Time.time - this.lastGrabTime > 1f && (this.remotePlayers.Count == 0 || velocity.magnitude > 2f))
			{
				this.SetVelocity_RPC(boneIndex, velocity, true, list.ToArray(), list2.ToArray());
			}
			this.lastGrabTime = Time.time;
			this.ropeCreakSFX.transform.parent = this.GetBone(Math.Max(0, boneIndex - 2)).transform;
			this.ropeCreakSFX.transform.localPosition = Vector3.zero;
			this.localPlayerOn = true;
			this.localPlayerXRNode = xrNode;
			this.localGrabbedRigid = rigid;
		}

		public void DetachLocalPlayer()
		{
			if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
			{
				GorillaTagger.Instance.offlineVRRig.grabbedRopeIndex = -1;
			}
			this.localPlayerOn = false;
			this.localGrabbedRigid = null;
		}

		public bool AttachRemotePlayer(int playerId, int boneIndex, Transform offsetTransform, Vector3 offset)
		{
			Rigidbody bone = this.GetBone(boneIndex);
			if (bone == null)
			{
				return false;
			}
			offsetTransform.SetParent(bone.transform);
			offsetTransform.localPosition = offset;
			offsetTransform.localRotation = Quaternion.identity;
			if (this.remotePlayers.ContainsKey(playerId))
			{
				Debug.LogError("already on the list!");
				return false;
			}
			this.remotePlayers.Add(playerId, boneIndex);
			return true;
		}

		public void DetachRemotePlayer(int playerId)
		{
			this.remotePlayers.Remove(playerId);
		}

		public void SetVelocity_RPC(int boneIndex, Vector3 velocity, bool wholeRope = true, Vector3[] ropeRotations = null, Vector3[] ropeVelocities = null)
		{
			if (NetworkSystem.Instance.InRoom)
			{
				base.photonView.RPC("SetVelocity", RpcTarget.All, new object[] { boneIndex, velocity, wholeRope, ropeRotations, ropeVelocities });
				return;
			}
			this.SetVelocity(boneIndex, velocity, wholeRope, ropeRotations, ropeVelocities);
		}

		[PunRPC]
		public void SetVelocity(int boneIndex, Vector3 velocity, bool wholeRope = true, Vector3[] ropeRotations = null, Vector3[] ropeVelocities = null)
		{
			this.SetIsIdle(false);
			if (ropeRotations != null && ropeVelocities != null && ropeRotations.Length != 0)
			{
				this.ToggleIsKinematic(true);
				for (int i = 0; i < ropeRotations.Length; i++)
				{
					if (i != 0)
					{
						this.bones[i].transform.localRotation = Quaternion.Euler(ropeRotations[i]);
						this.bones[i].linearVelocity = ropeVelocities[i];
					}
				}
				this.ToggleIsKinematic(false);
			}
			Rigidbody bone = this.GetBone(boneIndex);
			if (bone)
			{
				if (wholeRope)
				{
					int num = 0;
					float num2 = Mathf.Min(velocity.magnitude, 15f);
					foreach (Rigidbody rigidbody in this.bones)
					{
						Vector3 vector = velocity / (float)boneIndex * (float)num;
						vector = Vector3.ClampMagnitude(vector, num2);
						rigidbody.linearVelocity = vector;
						num++;
					}
					return;
				}
				bone.linearVelocity = velocity;
			}
		}

		public const float kPlayerMass = 0.8f;

		public const float ropeBitGenOffset = 1f;

		public const float MAX_ROPE_SPEED = 15f;

		[SerializeField]
		private GameObject prefabRopeBit;

		public Rigidbody[] bones = Array.Empty<Rigidbody>();

		private Dictionary<int, int> remotePlayers = new Dictionary<int, int>();

		[NonSerialized]
		public float lastGrabTime;

		[SerializeField]
		private AudioSource ropeCreakSFX;

		private bool localPlayerOn;

		private XRNode localPlayerXRNode;

		private Rigidbody localGrabbedRigid;

		private const float MAX_VELOCITY_FOR_IDLE = 0.1f;

		private const float TIME_FOR_IDLE = 2f;

		private float potentialIdleTimer;

		[Header("Config")]
		[SerializeField]
		private int ropeLength = 8;

		[SerializeField]
		private GorillaRopeSwingSettings settings;
	}
}

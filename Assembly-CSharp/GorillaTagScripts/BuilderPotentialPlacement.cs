using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public struct BuilderPotentialPlacement
	{
		public void Reset()
		{
			this.attachPiece = null;
			this.parentPiece = null;
			this.attachIndex = -1;
			this.parentAttachIndex = -1;
			this.localPosition = Vector3.zero;
			this.localRotation = Quaternion.identity;
			this.attachDistance = float.MaxValue;
			this.attachPlaneNormal = Vector3.zero;
			this.score = float.MinValue;
			this.twist = 0;
			this.bumpOffsetX = 0;
			this.bumpOffsetZ = 0;
		}

		public BuilderPiece attachPiece;

		public BuilderPiece parentPiece;

		public int attachIndex;

		public int parentAttachIndex;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public Vector3 attachPlaneNormal;

		public float attachDistance;

		public float score;

		public SnapBounds attachBounds;

		public SnapBounds parentAttachBounds;

		public byte twist;

		public sbyte bumpOffsetX;

		public sbyte bumpOffsetZ;
	}
}

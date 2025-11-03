using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public struct BuilderGridPlaneData
	{
		public BuilderGridPlaneData(BuilderAttachGridPlane gridPlane, int pieceIndex)
		{
			gridPlane.center.transform.GetPositionAndRotation(out this.position, out this.rotation);
			this.localPosition = gridPlane.pieceToGridPosition;
			this.localRotation = gridPlane.pieceToGridRotation;
			this.width = gridPlane.width;
			this.length = gridPlane.length;
			this.male = gridPlane.male;
			this.pieceId = gridPlane.piece.pieceId;
			this.pieceIndex = pieceIndex;
			this.boundingRadius = gridPlane.boundingRadius;
			this.attachIndex = gridPlane.attachIndex;
		}

		public int width;

		public int length;

		public bool male;

		public int pieceId;

		public int pieceIndex;

		public float boundingRadius;

		public int attachIndex;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 localPosition;

		public Quaternion localRotation;
	}
}

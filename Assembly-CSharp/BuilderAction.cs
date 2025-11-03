using System;
using UnityEngine;

public struct BuilderAction
{
	public BuilderActionType type;

	public int pieceId;

	public int parentPieceId;

	public Vector3 localPosition;

	public Quaternion localRotation;

	public byte twist;

	public sbyte bumpOffsetx;

	public sbyte bumpOffsetz;

	public bool isLeftHand;

	public int playerActorNumber;

	public int parentAttachIndex;

	public int attachIndex;

	public SnapBounds attachBounds;

	public SnapBounds parentAttachBounds;

	public Vector3 velocity;

	public Vector3 angVelocity;

	public int localCommandId;

	public int timeStamp;
}

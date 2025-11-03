using System;
using GorillaTag.CosmeticSystem;
using UnityEngine;

[Serializable]
public struct GorillaPosRotConstraint
{
	[Tooltip("Transform that should be moved, rotated, and scaled to match the `source` Transform in world space.")]
	public Transform follower;

	[Tooltip("Bone that `follower` should match. Set to `None` to assign a specific Transform within the same prefab.")]
	public GTHardCodedBones.SturdyEBone sourceGorillaBone;

	[Tooltip("Transform that `follower` should match. This is overridden at runtime if `sourceGorillaBone` is not `None`. If set in inspector, then it should be only set to a child of the the prefab this component belongs to.")]
	public Transform source;

	public string sourceRelativePath;

	[Tooltip("Offset to be applied to the follower's position.")]
	public Vector3 positionOffset;

	[Tooltip("Offset to be applied to the follower's rotation.")]
	public Quaternion rotationOffset;
}

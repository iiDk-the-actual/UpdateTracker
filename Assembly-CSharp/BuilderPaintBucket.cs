using System;
using UnityEngine;

public class BuilderPaintBucket : MonoBehaviour
{
	private void Awake()
	{
		if (string.IsNullOrEmpty(this.materialId))
		{
			return;
		}
		this.materialType = this.materialId.GetHashCode();
		if (this.bucketMaterialOptions != null && this.paintBucketRenderer != null)
		{
			Material material;
			int num;
			this.bucketMaterialOptions.GetMaterialFromType(this.materialType, out material, out num);
			if (material != null)
			{
				this.paintBucketRenderer.material = material;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.materialType == -1)
		{
			return;
		}
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			BuilderPaintBrush component = attachedRigidbody.GetComponent<BuilderPaintBrush>();
			if (component != null)
			{
				component.SetBrushMaterial(this.materialType);
			}
		}
	}

	[SerializeField]
	private BuilderMaterialOptions bucketMaterialOptions;

	[SerializeField]
	private MeshRenderer paintBucketRenderer;

	[SerializeField]
	private string materialId;

	private int materialType = -1;
}

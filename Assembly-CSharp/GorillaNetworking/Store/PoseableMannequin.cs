using System;
using System.Collections;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaNetworking.Store
{
	public class PoseableMannequin : MonoBehaviour
	{
		public void Start()
		{
			this.skinnedMeshRenderer.gameObject.SetActive(false);
			this.staticGorillaMesh.gameObject.SetActive(true);
		}

		private string GetPrefabPathFromCurrentPrefabStage()
		{
			return "";
		}

		private string GetMeshPathFromPrefabPath(string prefabPath)
		{
			return "";
		}

		public void BakeSkinnedMesh()
		{
			this.BakeAndSaveMeshInPath(this.GetMeshPathFromPrefabPath(this.GetPrefabPathFromCurrentPrefabStage()));
		}

		public void BakeAndSaveMeshInPath(string meshPath)
		{
		}

		private void UpdateStaticMeshMannequin()
		{
			this.staticGorillaMesh.sharedMesh = this.BakedColliderMesh;
			this.staticGorillaMeshRenderer.sharedMaterials = this.skinnedMeshRenderer.sharedMaterials;
			this.staticGorillaMeshCollider.sharedMesh = this.BakedColliderMesh;
		}

		private void UpdateSkinnedMeshCollider()
		{
			this.skinnedMeshCollider.sharedMesh = this.BakedColliderMesh;
		}

		public void UpdateGTPosRotConstraints()
		{
			GTPosRotConstraints[] array = this.cosmeticConstraints;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].constraints.ForEach(delegate(GorillaPosRotConstraint c)
				{
					c.follower.rotation = c.source.rotation;
					c.follower.position = c.source.position;
				});
			}
		}

		private void HookupCosmeticConstraints()
		{
			this.cosmeticConstraints = base.GetComponentsInChildren<GTPosRotConstraints>();
			foreach (GTPosRotConstraints gtposRotConstraints in this.cosmeticConstraints)
			{
				for (int j = 0; j < gtposRotConstraints.constraints.Length; j++)
				{
					gtposRotConstraints.constraints[j].source = this.FindBone(gtposRotConstraints.constraints[j].follower.name);
				}
			}
		}

		private Transform FindBone(string boneName)
		{
			foreach (Transform transform in this.skinnedMeshRenderer.bones)
			{
				if (transform.name == boneName)
				{
					return transform;
				}
			}
			return null;
		}

		public void CreasteTestClip()
		{
		}

		public void SerializeVRRig()
		{
			base.StartCoroutine(this.SaveLocalPlayerPose());
		}

		public IEnumerator SaveLocalPlayerPose()
		{
			yield return null;
			yield break;
		}

		public void SerializeOutBonesFromSkinnedMesh(SkinnedMeshRenderer paramSkinnedMeshRenderer)
		{
		}

		public void SetCurvesForBone(SkinnedMeshRenderer paramSkinnedMeshRenderer, AnimationClip clip, Transform bone)
		{
			Keyframe[] array = new Keyframe[]
			{
				new Keyframe(0f, bone.parent.localRotation.x)
			};
			Keyframe[] array2 = new Keyframe[]
			{
				new Keyframe(0f, bone.parent.localRotation.y)
			};
			Keyframe[] array3 = new Keyframe[]
			{
				new Keyframe(0f, bone.parent.localRotation.z)
			};
			Keyframe[] array4 = new Keyframe[]
			{
				new Keyframe(0f, bone.parent.localRotation.w)
			};
			AnimationCurve animationCurve = new AnimationCurve(array);
			AnimationCurve animationCurve2 = new AnimationCurve(array2);
			AnimationCurve animationCurve3 = new AnimationCurve(array3);
			AnimationCurve animationCurve4 = new AnimationCurve(array4);
			string text = "";
			string text2 = bone.name.Replace("_new", "");
			foreach (Transform transform in this.skinnedMeshRenderer.bones)
			{
				if (transform.name == text2)
				{
					text = transform.GetPath(this.skinnedMeshRenderer.transform.parent).TrimStart('/');
					break;
				}
			}
			clip.SetCurve(text, typeof(Transform), "m_LocalRotation.x", animationCurve);
			clip.SetCurve(text, typeof(Transform), "m_LocalRotation.y", animationCurve2);
			clip.SetCurve(text, typeof(Transform), "m_LocalRotation.z", animationCurve3);
			clip.SetCurve(text, typeof(Transform), "m_LocalRotation.w", animationCurve4);
		}

		public void UpdatePrefabWithAnimationClip(string AnimationFileName)
		{
		}

		public void LoadPoseOntoMannequin(AnimationClip clip, float frameTime = 0f)
		{
		}

		public void OnValidate()
		{
		}

		public SkinnedMeshRenderer skinnedMeshRenderer;

		[FormerlySerializedAs("meshCollider")]
		public MeshCollider skinnedMeshCollider;

		public GTPosRotConstraints[] cosmeticConstraints;

		public Mesh BakedColliderMesh;

		[SerializeField]
		[FormerlySerializedAs("liveAssetPath")]
		protected string prefabAssetPath;

		[SerializeField]
		protected string prefabFolderPath;

		[SerializeField]
		protected string prefabAssetName;

		public MeshFilter staticGorillaMesh;

		public MeshCollider staticGorillaMeshCollider;

		public MeshRenderer staticGorillaMeshRenderer;
	}
}

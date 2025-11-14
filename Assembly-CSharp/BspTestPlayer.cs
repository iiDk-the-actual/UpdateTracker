using System;
using TMPro;
using UnityEngine;

public class BspTestPlayer : MonoBehaviour
{
	private void Start()
	{
		if (this.bspSystem == null)
		{
			this.bspSystem = Object.FindObjectOfType<ZoneGraphBSP>();
		}
		if (this.zoneDisplayText == null)
		{
			this.CreateUI();
		}
	}

	private void Update()
	{
		this.HandleMovement();
		this.UpdateZoneInfo();
		this.UpdateUI();
	}

	private void HandleMovement()
	{
		Vector3 vector = Vector3.zero;
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			vector.x -= 1f;
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			vector.x += 1f;
		}
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			if (this.use3DMovement)
			{
				vector.z += 1f;
			}
			else
			{
				vector.y += 1f;
			}
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			if (this.use3DMovement)
			{
				vector.z -= 1f;
			}
			else
			{
				vector.y -= 1f;
			}
		}
		if (this.use3DMovement)
		{
			if (Input.GetKey(KeyCode.Q))
			{
				vector.y += 1f;
			}
			if (Input.GetKey(KeyCode.E))
			{
				vector.y -= 1f;
			}
		}
		if (vector != Vector3.zero)
		{
			vector = vector.normalized * this.moveSpeed * Time.deltaTime;
			base.transform.position += vector;
		}
	}

	private void UpdateZoneInfo()
	{
		if (this.bspSystem == null)
		{
			return;
		}
		ZoneDef zoneDef = this.bspSystem.FindZoneAtPoint(base.transform.position);
		if (zoneDef != this.currentZone)
		{
			this.currentZone = zoneDef;
			this.currentZoneName = ((zoneDef != null) ? zoneDef.gameObject.name : "None");
			if (zoneDef != null)
			{
				Debug.Log("Player entered zone: " + this.currentZoneName);
				return;
			}
			Debug.Log("Player left all zones");
		}
	}

	private void UpdateUI()
	{
		if (this.zoneDisplayText != null)
		{
			this.zoneDisplayText.text = "Current Zone: " + this.currentZoneName;
		}
		if (this.positionDisplayText != null)
		{
			Vector3 position = base.transform.position;
			this.positionDisplayText.text = string.Format("Position: ({0:F1}, {1:F1}, {2:F1})", position.x, position.y, position.z);
		}
	}

	private void CreateUI()
	{
		GameObject gameObject = new GameObject("Zone Display");
		this.zoneDisplayText = gameObject.AddComponent<TextMeshPro>();
		this.zoneDisplayText.text = "Current Zone: None";
		this.zoneDisplayText.fontSize = 24f;
		this.zoneDisplayText.color = Color.white;
		this.zoneDisplayText.autoSizeTextContainer = true;
		gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 5f + Vector3.up * 3f + Vector3.left * 2f;
		gameObject.transform.LookAt(Camera.main.transform);
		gameObject.transform.Rotate(0f, 180f, 0f);
		GameObject gameObject2 = new GameObject("Position Display");
		this.positionDisplayText = gameObject2.AddComponent<TextMeshPro>();
		this.positionDisplayText.text = "Position: (0, 0, 0)";
		this.positionDisplayText.fontSize = 18f;
		this.positionDisplayText.color = Color.yellow;
		this.positionDisplayText.autoSizeTextContainer = true;
		gameObject2.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 5f + Vector3.up * 2.5f + Vector3.left * 2f;
		gameObject2.transform.LookAt(Camera.main.transform);
		gameObject2.transform.Rotate(0f, 180f, 0f);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = ((this.currentZone != null) ? Color.green : Color.red);
		Gizmos.DrawWireSphere(base.transform.position, 0.5f);
		if (this.currentZone != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(base.transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
		}
		if (this.bspSystem != null && this.bspSystem.HasCompiledTree())
		{
			this.DrawBSPSplits();
		}
	}

	private void DrawBSPSplits()
	{
		SerializableBSPTree bsptree = this.bspSystem.GetBSPTree();
		if (((bsptree != null) ? bsptree.nodes : null) == null)
		{
			return;
		}
		BoxCollider[] array = Object.FindObjectsOfType<BoxCollider>();
		if (array.Length == 0)
		{
			return;
		}
		Bounds bounds = new Bounds(array[0].bounds.center, array[0].bounds.size);
		foreach (BoxCollider boxCollider in array)
		{
			bounds.Encapsulate(boxCollider.bounds);
		}
		bounds.Expand(2f);
		this.DrawPlayerPath(bsptree, base.transform.position, bsptree.rootIndex, bounds, 0);
	}

	private void DrawPlayerPath(SerializableBSPTree tree, Vector3 playerPos, int nodeIndex, Bounds bounds, int depth)
	{
		if (nodeIndex >= tree.nodes.Length || depth >= tree.nodes.Length)
		{
			return;
		}
		SerializableBSPNode serializableBSPNode = tree.nodes[nodeIndex];
		Gizmos.color = this.GetAxisColor(serializableBSPNode.axis, depth);
		if (serializableBSPNode.axis == SerializableBSPNode.Axis.Zone)
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Gizmos.DrawWireCube(bounds.center, bounds.size);
			return;
		}
		if (serializableBSPNode.axis == SerializableBSPNode.Axis.MatrixChain || serializableBSPNode.axis == SerializableBSPNode.Axis.MatrixFinal)
		{
			if (serializableBSPNode.matrixIndex >= 0 && serializableBSPNode.matrixIndex < tree.matrices.Length)
			{
				MatrixZonePair matrixZonePair = tree.matrices[serializableBSPNode.matrixIndex];
				Vector3 vector = matrixZonePair.matrix.MultiplyPoint3x4(playerPos);
				bool flag = Mathf.Abs(vector.x) <= 1f && Mathf.Abs(vector.y) <= 1f && Mathf.Abs(vector.z) <= 1f;
				Gizmos.color = (flag ? Color.yellow : Color.red);
				Matrix4x4 matrix = Gizmos.matrix;
				Gizmos.matrix = matrixZonePair.matrix.inverse;
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2f);
				Gizmos.matrix = matrix;
				if (!flag && serializableBSPNode.axis == SerializableBSPNode.Axis.MatrixChain)
				{
					this.DrawPlayerPath(tree, playerPos, serializableBSPNode.outsideChildIndex, bounds, depth + 1);
					return;
				}
			}
			else
			{
				Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
			return;
		}
		this.DrawSplitPlane(serializableBSPNode.axis, serializableBSPNode.splitValue, bounds);
		Bounds bounds2 = bounds;
		Bounds bounds3 = bounds;
		switch (serializableBSPNode.axis)
		{
		case SerializableBSPNode.Axis.X:
			bounds2.SetMinMax(bounds.min, new Vector3(serializableBSPNode.splitValue, bounds.max.y, bounds.max.z));
			bounds3.SetMinMax(new Vector3(serializableBSPNode.splitValue, bounds.min.y, bounds.min.z), bounds.max);
			break;
		case SerializableBSPNode.Axis.Y:
			bounds2.SetMinMax(bounds.min, new Vector3(bounds.max.x, serializableBSPNode.splitValue, bounds.max.z));
			bounds3.SetMinMax(new Vector3(bounds.min.x, serializableBSPNode.splitValue, bounds.min.z), bounds.max);
			break;
		case SerializableBSPNode.Axis.Z:
			bounds2.SetMinMax(bounds.min, new Vector3(bounds.max.x, bounds.max.y, serializableBSPNode.splitValue));
			bounds3.SetMinMax(new Vector3(bounds.min.x, bounds.min.y, serializableBSPNode.splitValue), bounds.max);
			break;
		}
		if (this.GetAxisValue(playerPos, serializableBSPNode.axis) < serializableBSPNode.splitValue)
		{
			this.DrawPlayerPath(tree, playerPos, (int)serializableBSPNode.leftChildIndex, bounds2, depth + 1);
			return;
		}
		this.DrawPlayerPath(tree, playerPos, (int)serializableBSPNode.rightChildIndex, bounds3, depth + 1);
	}

	private float GetAxisValue(Vector3 point, SerializableBSPNode.Axis axis)
	{
		switch (axis)
		{
		case SerializableBSPNode.Axis.X:
			return point.x;
		case SerializableBSPNode.Axis.Y:
			return point.y;
		case SerializableBSPNode.Axis.Z:
			return point.z;
		case SerializableBSPNode.Axis.MatrixChain:
			return 0f;
		case SerializableBSPNode.Axis.MatrixFinal:
			return 0f;
		case SerializableBSPNode.Axis.Zone:
			return 0f;
		default:
			return 0f;
		}
	}

	private void DrawSplitPlane(SerializableBSPNode.Axis axis, float splitValue, Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;
		switch (axis)
		{
		case SerializableBSPNode.Axis.X:
			center.x = splitValue;
			Gizmos.DrawLine(new Vector3(splitValue, bounds.min.y, bounds.min.z), new Vector3(splitValue, bounds.max.y, bounds.max.z));
			Gizmos.DrawLine(new Vector3(splitValue, bounds.max.y, bounds.min.z), new Vector3(splitValue, bounds.min.y, bounds.max.z));
			return;
		case SerializableBSPNode.Axis.Y:
			center.y = splitValue;
			Gizmos.DrawLine(new Vector3(bounds.min.x, splitValue, bounds.min.z), new Vector3(bounds.max.x, splitValue, bounds.max.z));
			Gizmos.DrawLine(new Vector3(bounds.max.x, splitValue, bounds.min.z), new Vector3(bounds.min.x, splitValue, bounds.max.z));
			return;
		case SerializableBSPNode.Axis.Z:
			center.z = splitValue;
			Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.min.y, splitValue), new Vector3(bounds.max.x, bounds.max.y, splitValue));
			Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.min.y, splitValue), new Vector3(bounds.min.x, bounds.max.y, splitValue));
			return;
		default:
			return;
		}
	}

	private Color GetAxisColor(SerializableBSPNode.Axis axis, int depth)
	{
		float num = 1f - (float)depth * 0.15f;
		num = Mathf.Max(num, 0.3f);
		switch (axis)
		{
		case SerializableBSPNode.Axis.X:
			return new Color(1f, 0f, 0f, num);
		case SerializableBSPNode.Axis.Y:
			return new Color(0f, 1f, 0f, num);
		case SerializableBSPNode.Axis.Z:
			return new Color(0f, 0f, 1f, num);
		case SerializableBSPNode.Axis.MatrixChain:
			return new Color(1f, 0f, 1f, num);
		case SerializableBSPNode.Axis.MatrixFinal:
			return new Color(0.5f, 0f, 1f, num);
		case SerializableBSPNode.Axis.Zone:
			return new Color(0f, 1f, 0f, num);
		default:
			return new Color(1f, 1f, 1f, num);
		}
	}

	[Header("Movement")]
	[SerializeField]
	private float moveSpeed = 5f;

	[SerializeField]
	private bool use3DMovement;

	[Header("UI")]
	[SerializeField]
	private TextMeshPro zoneDisplayText;

	[SerializeField]
	private TextMeshPro positionDisplayText;

	[Header("BSP")]
	[SerializeField]
	private ZoneGraphBSP bspSystem;

	private string currentZoneName = "None";

	private ZoneDef currentZone;
}

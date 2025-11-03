using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtils
{
	public static Mesh CreateReadableMeshCopy(this Mesh sourceMesh)
	{
		Mesh mesh = new Mesh();
		mesh.indexFormat = sourceMesh.indexFormat;
		GraphicsBuffer vertexBuffer = sourceMesh.GetVertexBuffer(0);
		int num = vertexBuffer.stride * vertexBuffer.count;
		byte[] array = new byte[num];
		vertexBuffer.GetData(array);
		mesh.SetVertexBufferParams(sourceMesh.vertexCount, sourceMesh.GetVertexAttributes());
		mesh.SetVertexBufferData<byte>(array, 0, 0, num, 0, MeshUpdateFlags.Default);
		vertexBuffer.Release();
		mesh.subMeshCount = sourceMesh.subMeshCount;
		GraphicsBuffer indexBuffer = sourceMesh.GetIndexBuffer();
		int num2 = indexBuffer.stride * indexBuffer.count;
		byte[] array2 = new byte[num2];
		indexBuffer.GetData(array2);
		mesh.SetIndexBufferParams(indexBuffer.count, sourceMesh.indexFormat);
		mesh.SetIndexBufferData<byte>(array2, 0, 0, num2, MeshUpdateFlags.Default);
		indexBuffer.Release();
		uint num3 = 0U;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			uint indexCount = sourceMesh.GetIndexCount(i);
			mesh.SetSubMesh(i, new SubMeshDescriptor((int)num3, (int)indexCount, MeshTopology.Triangles), MeshUpdateFlags.Default);
			num3 += indexCount;
		}
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}
}

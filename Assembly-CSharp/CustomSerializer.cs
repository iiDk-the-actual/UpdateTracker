using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class CustomSerializer
{
	public static byte[] ByteSerialize(this object obj)
	{
		return CustomSerializer.Serialize(obj);
	}

	public static object ByteDeserialize(this byte[] bytes)
	{
		return CustomSerializer.Deserialize(bytes);
	}

	public static byte[] Serialize(object obj)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
			{
				CustomSerializer.SerializeObject(binaryWriter, obj);
				array = memoryStream.ToArray();
			}
		}
		return array;
	}

	public static object Deserialize(byte[] data)
	{
		object obj;
		using (MemoryStream memoryStream = new MemoryStream(data))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
			{
				obj = CustomSerializer.DeserializeObject(binaryReader);
			}
		}
		return obj;
	}

	private static void SerializeObject(BinaryWriter writer, object obj)
	{
		string text = obj as string;
		if (text != null)
		{
			writer.Write(1);
			writer.Write(text);
			return;
		}
		if (obj is bool)
		{
			bool flag = (bool)obj;
			writer.Write(2);
			writer.Write(flag);
			return;
		}
		if (obj is int)
		{
			int num = (int)obj;
			writer.Write(3);
			writer.Write(num);
			return;
		}
		if (obj is float)
		{
			float num2 = (float)obj;
			writer.Write(4);
			writer.Write(num2);
			return;
		}
		if (obj is double)
		{
			double num3 = (double)obj;
			writer.Write(5);
			writer.Write(num3);
			return;
		}
		if (obj is Vector2)
		{
			Vector2 vector = (Vector2)obj;
			writer.Write(6);
			writer.Write(vector.x);
			writer.Write(vector.y);
			return;
		}
		if (obj is Vector3)
		{
			Vector3 vector2 = (Vector3)obj;
			writer.Write(7);
			writer.Write(vector2.x);
			writer.Write(vector2.y);
			writer.Write(vector2.z);
			return;
		}
		object[] array = obj as object[];
		if (array != null)
		{
			writer.Write(8);
			CustomSerializer.SerializeObjectArray(writer, array);
			return;
		}
		if (obj is byte)
		{
			byte b = (byte)obj;
			writer.Write(9);
			writer.Write(b);
			return;
		}
		Enum @enum = obj as Enum;
		if (@enum != null)
		{
			writer.Write(10);
			writer.Write(Convert.ToInt32(@enum));
			writer.Write(@enum.GetType().AssemblyQualifiedName);
			return;
		}
		NetEventOptions netEventOptions = obj as NetEventOptions;
		if (netEventOptions != null)
		{
			writer.Write(11);
			CustomSerializer.SerializeNetEventOptions(writer, netEventOptions);
			return;
		}
		if (obj is Quaternion)
		{
			Quaternion quaternion = (Quaternion)obj;
			writer.Write(12);
			writer.Write(quaternion.x);
			writer.Write(quaternion.y);
			writer.Write(quaternion.z);
			writer.Write(quaternion.w);
			return;
		}
		Debug.LogWarning("<color=blue>type not supported " + obj.GetType().ToString() + "</color>");
	}

	private static void SerializeObjectArray(BinaryWriter writer, object[] objects)
	{
		writer.Write(objects.Length);
		foreach (object obj in objects)
		{
			CustomSerializer.SerializeObject(writer, obj);
		}
	}

	private static void SerializeNetEventOptions(BinaryWriter writer, NetEventOptions options)
	{
		writer.Write((int)options.Reciever);
		if (options.TargetActors == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(options.TargetActors.Length);
			foreach (int num in options.TargetActors)
			{
				writer.Write(num);
			}
		}
		writer.Write(options.Flags.WebhookFlags);
	}

	private static object DeserializeObject(BinaryReader reader)
	{
		switch (reader.ReadByte())
		{
		case 0:
			return null;
		case 1:
			return reader.ReadString();
		case 2:
			return reader.ReadBoolean();
		case 3:
			return reader.ReadInt32();
		case 4:
			return reader.ReadSingle();
		case 5:
			return reader.ReadDouble();
		case 6:
		{
			float num = reader.ReadSingle();
			float num2 = reader.ReadSingle();
			return new Vector2(num, num2);
		}
		case 7:
		{
			float num3 = reader.ReadSingle();
			float num4 = reader.ReadSingle();
			float num5 = reader.ReadSingle();
			return new Vector3(num3, num4, num5);
		}
		case 8:
			return CustomSerializer.DeserializeObjectArray(reader);
		case 9:
			return reader.ReadByte();
		case 10:
		{
			int num6 = reader.ReadInt32();
			return Enum.ToObject(Type.GetType(reader.ReadString()), num6);
		}
		case 11:
			return CustomSerializer.DeserializeNetEventOptions(reader);
		case 12:
		{
			float num7 = reader.ReadSingle();
			float num8 = reader.ReadSingle();
			float num9 = reader.ReadSingle();
			float num10 = reader.ReadSingle();
			return new Quaternion(num7, num8, num9, num10);
		}
		default:
			throw new InvalidOperationException("Unsupported type");
		}
	}

	private static object[] DeserializeObjectArray(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = CustomSerializer.DeserializeObject(reader);
		}
		return array;
	}

	private static NetEventOptions DeserializeNetEventOptions(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		int[] array = null;
		if (num2 > 0)
		{
			array = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = reader.ReadInt32();
			}
		}
		byte b = reader.ReadByte();
		return new NetEventOptions(num, array, b);
	}
}

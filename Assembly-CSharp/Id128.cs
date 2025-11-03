using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct Id128 : IEquatable<Id128>, IComparable<Id128>, IEquatable<Guid>, IEquatable<Hash128>
{
	public Id128(int a, int b, int c, int d)
	{
		this.guid = Guid.Empty;
		this.h128 = default(Hash128);
		this.x = (this.y = 0L);
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}

	public Id128(long x, long y)
	{
		this.a = (this.b = (this.c = (this.d = 0)));
		this.guid = Guid.Empty;
		this.h128 = default(Hash128);
		this.x = x;
		this.y = y;
	}

	public Id128(Hash128 hash)
	{
		this.x = (this.y = 0L);
		this.a = (this.b = (this.c = (this.d = 0)));
		this.guid = Guid.Empty;
		this.h128 = hash;
	}

	public Id128(Guid guid)
	{
		this.a = (this.b = (this.c = (this.d = 0)));
		this.x = (this.y = 0L);
		this.h128 = default(Hash128);
		this.guid = guid;
	}

	public Id128(string guid)
	{
		if (string.IsNullOrWhiteSpace(guid))
		{
			throw new ArgumentNullException("guid");
		}
		this.a = (this.b = (this.c = (this.d = 0)));
		this.x = (this.y = 0L);
		this.h128 = default(Hash128);
		this.guid = Guid.Parse(guid);
	}

	public Id128(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (bytes.Length != 16)
		{
			throw new ArgumentException("Input buffer must be exactly 16 bytes", "bytes");
		}
		this.a = (this.b = (this.c = (this.d = 0)));
		this.x = (this.y = 0L);
		this.h128 = default(Hash128);
		this.guid = new Guid(bytes);
	}

	[return: TupleElementNames(new string[] { "l1", "l2" })]
	public ValueTuple<long, long> ToLongs()
	{
		return new ValueTuple<long, long>(this.x, this.y);
	}

	[return: TupleElementNames(new string[] { "i1", "i2", "i3", "i4" })]
	public ValueTuple<int, int, int, int> ToInts()
	{
		return new ValueTuple<int, int, int, int>(this.a, this.b, this.c, this.d);
	}

	public byte[] ToByteArray()
	{
		return this.guid.ToByteArray();
	}

	public bool Equals(Id128 id)
	{
		return this.x == id.x && this.y == id.y;
	}

	public bool Equals(Guid g)
	{
		return this.guid == g;
	}

	public bool Equals(Hash128 h)
	{
		return this.h128 == h;
	}

	public override bool Equals(object obj)
	{
		if (obj is Id128)
		{
			Id128 id = (Id128)obj;
			return this.Equals(id);
		}
		if (obj is Guid)
		{
			Guid guid = (Guid)obj;
			return this.Equals(guid);
		}
		if (obj is Hash128)
		{
			Hash128 hash = (Hash128)obj;
			return this.Equals(hash);
		}
		return false;
	}

	public override string ToString()
	{
		return this.guid.ToString();
	}

	public override int GetHashCode()
	{
		return StaticHash.Compute(this.a, this.b, this.c, this.d);
	}

	public int CompareTo(Id128 id)
	{
		int num = this.x.CompareTo(id.x);
		if (num == 0)
		{
			num = this.y.CompareTo(id.y);
		}
		return num;
	}

	public int CompareTo(object obj)
	{
		if (obj is Id128)
		{
			Id128 id = (Id128)obj;
			return this.CompareTo(id);
		}
		if (obj is Guid)
		{
			Guid guid = (Guid)obj;
			return this.guid.CompareTo(guid);
		}
		if (obj is Hash128)
		{
			Hash128 hash = (Hash128)obj;
			return this.h128.CompareTo(hash);
		}
		throw new ArgumentException("Object must be of type Id128 or Guid");
	}

	public static Id128 NewId()
	{
		return new Id128(Guid.NewGuid());
	}

	public static Id128 ComputeMD5(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return Id128.Empty;
		}
		Id128 id;
		using (MD5 md = MD5.Create())
		{
			id = new Guid(md.ComputeHash(Encoding.UTF8.GetBytes(s)));
		}
		return id;
	}

	public static Id128 ComputeSHV2(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return Id128.Empty;
		}
		return Hash128.Compute(s);
	}

	public static bool operator ==(Id128 j, Id128 k)
	{
		return j.Equals(k);
	}

	public static bool operator !=(Id128 j, Id128 k)
	{
		return !j.Equals(k);
	}

	public static bool operator ==(Id128 j, Guid k)
	{
		return j.Equals(k);
	}

	public static bool operator !=(Id128 j, Guid k)
	{
		return !j.Equals(k);
	}

	public static bool operator ==(Guid j, Id128 k)
	{
		return j.Equals(k.guid);
	}

	public static bool operator !=(Guid j, Id128 k)
	{
		return !j.Equals(k.guid);
	}

	public static bool operator ==(Id128 j, Hash128 k)
	{
		return j.Equals(k);
	}

	public static bool operator !=(Id128 j, Hash128 k)
	{
		return !j.Equals(k);
	}

	public static bool operator ==(Hash128 j, Id128 k)
	{
		return j.Equals(k.h128);
	}

	public static bool operator !=(Hash128 j, Id128 k)
	{
		return !j.Equals(k.h128);
	}

	public static bool operator <(Id128 j, Id128 k)
	{
		return j.CompareTo(k) < 0;
	}

	public static bool operator >(Id128 j, Id128 k)
	{
		return j.CompareTo(k) > 0;
	}

	public static bool operator <=(Id128 j, Id128 k)
	{
		return j.CompareTo(k) <= 0;
	}

	public static bool operator >=(Id128 j, Id128 k)
	{
		return j.CompareTo(k) >= 0;
	}

	public static implicit operator Guid(Id128 id)
	{
		return id.guid;
	}

	public static implicit operator Id128(Guid guid)
	{
		return new Id128(guid);
	}

	public static implicit operator Id128(Hash128 h)
	{
		return new Id128(h);
	}

	public static implicit operator Hash128(Id128 id)
	{
		return id.h128;
	}

	public static explicit operator Id128(string s)
	{
		return Id128.ComputeMD5(s);
	}

	[SerializeField]
	[FieldOffset(0)]
	public long x;

	[SerializeField]
	[FieldOffset(8)]
	public long y;

	[NonSerialized]
	[FieldOffset(0)]
	public int a;

	[NonSerialized]
	[FieldOffset(4)]
	public int b;

	[NonSerialized]
	[FieldOffset(8)]
	public int c;

	[NonSerialized]
	[FieldOffset(12)]
	public int d;

	[NonSerialized]
	[FieldOffset(0)]
	public Guid guid;

	[NonSerialized]
	[FieldOffset(0)]
	public Hash128 h128;

	public static readonly Id128 Empty;
}

using System.Collections;
using System.Collections.ObjectModel;
using System.IO;

namespace BitTorrent;

public class ValueList : BEncodeValue, IEnumerable, IEnumerator
{
	private const int MaxItems = 100000;

	private Collection<BEncodeValue> values;

	private int Position = -1;

	public object Current => values[Position];

	public Collection<BEncodeValue> Values
	{
		get
		{
			return values;
		}
		set
		{
			values.Clear();
			foreach (BEncodeValue item in value)
			{
				value.Add(item);
			}
		}
	}

	public BEncodeValue this[int index]
	{
		get
		{
			return values[index];
		}
		set
		{
			values[index] = value;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		if (Position < values.Count - 1)
		{
			Position++;
			return true;
		}
		return false;
	}

	public void Reset()
	{
		Position = -1;
	}

	public ValueList()
	{
		values = new Collection<BEncodeValue>();
	}

	public void Parse(Stream s)
	{
		int num = s.ReadByte();
		byte b = (byte)num;
		while (b != 101 && num != -1)
		{
			if (values.Count >= MaxItems)
			{
				throw new TorrentException("Bencoded list exceeded the maximum item count.");
			}
			BEncodeValue item = BEncode.Parse(s, b);
			values.Add(item);
			num = s.ReadByte();
			b = (byte)num;
		}
		if (num == -1)
		{
			throw new IncompleteTorrentData("Unexpected end of bencoded list.");
		}
	}

	public void Add(BEncodeValue value)
	{
		values.Add(value);
	}

	public void Clear()
	{
		foreach (BEncodeValue value in values)
		{
			BEncode.Clear(value);
		}
		values.Clear();
		Position = -1;
	}

	public byte[] Encode()
	{
		Collection<byte> collection = new Collection<byte>();
		collection.Add(108);
		foreach (BEncodeValue value in values)
		{
			byte[] array = value.Encode();
			foreach (byte item in array)
			{
				collection.Add(item);
			}
		}
		collection.Add(101);
		byte[] array2 = new byte[collection.Count];
		for (int j = 0; j < collection.Count; j++)
		{
			array2[j] = collection[j];
		}
		return array2;
	}
}

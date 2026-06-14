using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BitTorrent;

public class ValueDictionary : BEncodeValue
{
	private const int MaxEntries = 100000;

	private Dictionary<string, BEncodeValue> dict = new Dictionary<string, BEncodeValue>();

	public ICollection Values => dict.Values;

	public ICollection Keys => dict.Keys;

	public BEncodeValue this[string key]
	{
		get
		{
			if (!dict.ContainsKey(key))
			{
				dict.Add(key, new ValueString(""));
			}
			return dict[key];
		}
		set
		{
			if (dict.ContainsKey(key))
			{
				dict.Remove(key);
			}
			dict.Add(key, value);
		}
	}

	public void Parse(Stream s)
	{
		int num = s.ReadByte();
		byte b = (byte)num;
		while (b != 101 && num != -1 && char.IsNumber((char)b))
		{
			if (dict.Count >= MaxEntries)
			{
				throw new TorrentException("Bencoded dictionary exceeded the maximum entry count.");
			}
			ValueString valueString = new ValueString();
			valueString.Parse(s, b);
			BEncodeValue value = BEncode.Parse(s);
			if (valueString.String != null)
			{
				if (dict.ContainsKey(valueString.String))
				{
					dict[valueString.String] = value;
				}
				else
				{
					dict.Add(valueString.String, value);
				}
			}
			num = s.ReadByte();
			b = (byte)num;
		}
		if (num == -1)
		{
			throw new IncompleteTorrentData("Unexpected end of bencoded dictionary.");
		}
	}

	public void Add(string key, BEncodeValue value)
	{
		dict.Add(key, value);
	}

	public void SetStringValue(string key, string value)
	{
		if (Contains(key))
		{
			((ValueString)this[key]).String = value;
		}
		else
		{
			this[key] = new ValueString(value);
		}
	}

	public bool Contains(string key)
	{
		return dict.ContainsKey(key);
	}

	public void Remove(string key)
	{
		dict.Remove(key);
	}

	public void Clear()
	{
		foreach (BEncodeValue value in dict.Values)
		{
			BEncode.Clear(value);
		}
		dict.Clear();
	}

	public byte[] Encode()
	{
		Collection<byte> collection = new Collection<byte>();
		collection.Add(100);
		ArrayList arrayList = new ArrayList();
		foreach (string key in dict.Keys)
		{
			arrayList.Add(key);
		}
		foreach (string item3 in arrayList)
		{
			ValueString valueString = new ValueString(item3);
			byte[] array = valueString.Encode();
			foreach (byte item in array)
			{
				collection.Add(item);
			}
			byte[] array2 = dict[item3].Encode();
			foreach (byte item2 in array2)
			{
				collection.Add(item2);
			}
		}
		collection.Add(101);
		byte[] array3 = new byte[collection.Count];
		collection.CopyTo(array3, 0);
		return array3;
	}
}

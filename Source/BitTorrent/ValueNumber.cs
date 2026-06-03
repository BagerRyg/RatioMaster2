using System;
using System.IO;
using System.Text;

namespace BitTorrent;

public class ValueNumber : BEncodeValue
{
	private string v;

	private byte[] data;

	public string String
	{
		get
		{
			return v;
		}
		set
		{
			v = value;
			data = Encoding.GetEncoding(28591).GetBytes(v);
		}
	}

	public long Integer
	{
		get
		{
			return long.Parse(v);
		}
		set
		{
			String = value.ToString();
		}
	}

	public byte[] Encode()
	{
		byte[] array = new byte[data.Length + 2];
		array[0] = 105;
		for (int i = 0; i < data.Length; i++)
		{
			array[i + 1] = data[i];
		}
		array[data.Length + 1] = 101;
		return array;
	}

	public ValueNumber(long number)
	{
		v = number.ToString();
		String = v;
	}

	public ValueNumber()
	{
	}

	public void Parse(Stream s)
	{
		string text = string.Empty;
		int num = s.ReadByte();
		char c = (char)num;
		while (c != 'e' && num != -1)
		{
			text += c;
			num = s.ReadByte();
			c = (char)num;
		}
		try
		{
			String = long.Parse(text).ToString();
		}
		catch (Exception)
		{
			String = "";
		}
	}
}

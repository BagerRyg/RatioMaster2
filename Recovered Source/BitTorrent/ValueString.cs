using System;
using System.IO;
using System.Text;

namespace BitTorrent;

public class ValueString : BEncodeValue
{
	private string v;

	private byte[] data;

	public int Length => v.Length;

	public byte[] Bytes => data;

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

	public byte[] Encode()
	{
		string text = v.Length + ":";
		byte[] bytes = Encoding.GetEncoding(28591).GetBytes(text);
		byte[] array = new byte[text.Length + data.Length];
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = bytes[i];
		}
		for (int j = 0; j < data.Length; j++)
		{
			array[j + text.Length] = data[j];
		}
		return array;
	}

	public ValueString(string StringValue)
	{
		String = StringValue;
	}

	public ValueString()
	{
	}

	public void Parse(Stream s)
	{
		throw new TorrentException("Parse method not supported, the first byte must be passed into the string parse routine.");
	}

	public void Parse(Stream s, byte firstByte)
	{
		char c = (char)firstByte;
		string text = c.ToString();
		if (!char.IsNumber(text[0]))
		{
			return;
		}
		int num = s.ReadByte();
		char c2 = (char)num;
		while (c2 != ':' && num != -1)
		{
			text += c2;
			num = s.ReadByte();
			c2 = (char)num;
		}
		try
		{
			int num2 = int.Parse(text);
			data = new byte[num2];
			s.Read(data, 0, num2);
			v = Encoding.GetEncoding(28591).GetString(data);
		}
		catch (Exception)
		{
			v = null;
		}
	}
}

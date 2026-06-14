using System;
using System.IO;

namespace BitTorrent;

public class BEncode
{
	private const int MaxParseDepth = 128;

	[ThreadStatic]
	private static int parseDepth;

	private BEncode()
	{
	}

	public static BEncodeValue Parse(Stream d)
	{
		int firstByte = d.ReadByte();
		if (firstByte < 0)
		{
			throw new IncompleteTorrentData("Unexpected end of bencoded data.");
		}
		return Parse(d, (byte)firstByte);
	}

	public static string String(BEncodeValue v)
	{
		if (v is ValueString)
		{
			return ((ValueString)v).String;
		}
		if (v is ValueNumber)
		{
			return ((ValueNumber)v).String;
		}
		return null;
	}

	public static BEncodeValue Parse(Stream d, byte firstByte)
	{
		if (parseDepth >= MaxParseDepth)
		{
			throw new TorrentException("Bencoded data exceeded the maximum nesting depth.");
		}
		parseDepth++;
		try
		{
			char c = (char)firstByte;
			BEncodeValue bEncodeValue = c switch
			{
				'd' => new ValueDictionary(),
				'l' => new ValueList(),
				'i' => new ValueNumber(),
				_ => new ValueString(),
			};
			if (bEncodeValue is ValueString)
			{
				((ValueString)bEncodeValue).Parse(d, (byte)c);
			}
			else
			{
				bEncodeValue.Parse(d);
			}
			return bEncodeValue;
		}
		finally
		{
			parseDepth--;
		}
	}

	public static void Clear(BEncodeValue value)
	{
		switch (value)
		{
		case ValueString stringValue:
			stringValue.Clear();
			break;
		case ValueNumber numberValue:
			numberValue.Clear();
			break;
		case ValueDictionary dictionaryValue:
			dictionaryValue.Clear();
			break;
		case ValueList listValue:
			listValue.Clear();
			break;
		}
	}
}

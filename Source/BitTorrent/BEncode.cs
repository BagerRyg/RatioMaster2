using System.IO;

namespace BitTorrent;

public class BEncode
{
	private BEncode()
	{
	}

	public static BEncodeValue Parse(Stream d)
	{
		return Parse(d, (byte)d.ReadByte());
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
}

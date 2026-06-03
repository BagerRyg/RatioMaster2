using System.IO;

namespace BitTorrent;

public interface BEncodeValue
{
	byte[] Encode();

	void Parse(Stream p);
}

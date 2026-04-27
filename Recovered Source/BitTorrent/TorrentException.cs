using System;

namespace BitTorrent;

public class TorrentException : Exception
{
	public TorrentException(string message)
		: base(message)
	{
	}
}

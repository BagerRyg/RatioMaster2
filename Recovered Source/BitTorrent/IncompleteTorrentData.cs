namespace BitTorrent;

public class IncompleteTorrentData : TorrentException
{
	public IncompleteTorrentData(string message)
		: base(message)
	{
	}
}

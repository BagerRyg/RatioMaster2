namespace BitTorrent;

public class TorrentFile
{
	private long _length;

	private string _filepath;

	public long Length => _length;

	public string Path => _filepath;

	public TorrentFile(long len, string apath)
	{
		_length = len;
		_filepath = apath;
	}
}

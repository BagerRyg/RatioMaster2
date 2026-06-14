using System;

namespace RatioMaster;

public struct TorrentInfo
{
	private string _tracker;

	private string _hash;

	private long _uploadRate;

	private long _downloadRate;

	private int _interval;

	private long _uploaded;

	private long _downloaded;

	private long _uploadedLast;

	private long _downloadedLast;

	private long _leftLast;

	private long _left;

	private long _totalsize;

	private string _filename;

	private string _key;

	private string _port;

	private Random random;

	private string _numberOfPeers;

	private string _peerID;

	private Uri _trackerUri;

	private string[] _trackers;

	public string tracker
	{
		get
		{
			return _tracker;
		}
		set
		{
			_tracker = value;
		}
	}

	public string hash
	{
		get
		{
			return _hash;
		}
		set
		{
			_hash = value;
		}
	}

	public long uploadRate
	{
		get
		{
			return _uploadRate;
		}
		set
		{
			_uploadRate = value;
		}
	}

	public long downloadRate
	{
		get
		{
			return _downloadRate;
		}
		set
		{
			_downloadRate = value;
		}
	}

	public int interval
	{
		get
		{
			return _interval;
		}
		set
		{
			_interval = value;
		}
	}

	public long uploaded
	{
		get
		{
			return _uploaded;
		}
		set
		{
			_uploaded = value;
		}
	}

	public long downloaded
	{
		get
		{
			return _downloaded;
		}
		set
		{
			_downloaded = value;
		}
	}

	public long left
	{
		get
		{
			return _left;
		}
		set
		{
			_left = value;
		}
	}

	public long uploadedLast
	{
		get
		{
			return _uploadedLast;
		}
		set
		{
			_uploadedLast = value;
		}
	}

	public long downloadedLast
	{
		get
		{
			return _downloadedLast;
		}
		set
		{
			_downloadedLast = value;
		}
	}

	public long leftLast
	{
		get
		{
			return _leftLast;
		}
		set
		{
			_leftLast = value;
		}
	}

	public long totalsize
	{
		get
		{
			return _totalsize;
		}
		set
		{
			_totalsize = value;
		}
	}

	public string filename
	{
		get
		{
			return _filename;
		}
		set
		{
			_filename = value;
		}
	}

	public string key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	public string port
	{
		get
		{
			return _port;
		}
		set
		{
			_port = value;
		}
	}

	public string numberOfPeers
	{
		get
		{
			return _numberOfPeers;
		}
		set
		{
			_numberOfPeers = value;
		}
	}

	public string peerID
	{
		get
		{
			return _peerID;
		}
		set
		{
			_peerID = value;
		}
	}

	public Uri trackerUri
	{
		get
		{
			return _trackerUri;
		}
		set
		{
			_trackerUri = value;
		}
	}

	public string[] trackers
	{
		get
		{
			return _trackers;
		}
		set
		{
			_trackers = value;
		}
	}

	public TorrentInfo(long uploaded, long downloaded)
	{
		_uploaded = uploaded;
		_downloaded = downloaded;
		_uploadedLast = uploaded;
		_downloadedLast = downloaded;
		_tracker = "";
		_hash = "";
		_left = 10000L;
		_leftLast = _left;
		_totalsize = 10000L;
		_filename = "";
		_uploadRate = 52224L;
		_downloadRate = 10240L;
		_interval = 1800;
		random = new Random();
		_key = random.Next(1000).ToString();
		_port = random.Next(1025, 65535).ToString();
		_numberOfPeers = "100";
		_peerID = "";
		_trackerUri = null;
		_trackers = new string[0];
	}

	public void ClearSensitiveData()
	{
		_tracker = string.Empty;
		_hash = string.Empty;
		_filename = string.Empty;
		_key = string.Empty;
		_peerID = string.Empty;
		_trackerUri = null;
		if (_trackers != null)
		{
			Array.Clear(_trackers, 0, _trackers.Length);
		}
		_trackers = Array.Empty<string>();
	}
}

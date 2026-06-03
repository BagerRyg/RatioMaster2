namespace RatioMaster;

public class TorrentClient
{
	private string _Name;

	private string _Query;

	private string _Headers;

	private string _PeerID;

	private string _PeerIDPrefix;

	private string _key;

	private string _HttpProtocol;

	private bool _HashUpperCase;

	private string _ProcessName = "";

	private string _UrlEncodingExceptions = "";

	private bool _NumwantRandomize;

	private int _NumwantInitialValue = 100;

	private int _NumwantRadius = 10;

	private bool _IsDivider;

	public string Name
	{
		get
		{
			return _Name;
		}
		set
		{
			_Name = value;
		}
	}

	public string Query
	{
		get
		{
			return _Query;
		}
		set
		{
			_Query = value;
		}
	}

	public string Headers
	{
		get
		{
			return _Headers;
		}
		set
		{
			_Headers = value;
		}
	}

	public string PeerID
	{
		get
		{
			return _PeerID;
		}
		set
		{
			_PeerID = value;
		}
	}

	public string PeerIDPrefix
	{
		get
		{
			return _PeerIDPrefix;
		}
		set
		{
			_PeerIDPrefix = value;
		}
	}

	public string Key
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

	public string HttpProtocol
	{
		get
		{
			return _HttpProtocol;
		}
		set
		{
			_HttpProtocol = value;
		}
	}

	public bool HashUpperCase
	{
		get
		{
			return _HashUpperCase;
		}
		set
		{
			_HashUpperCase = value;
		}
	}

	public string ProcessName
	{
		get
		{
			return _ProcessName;
		}
		set
		{
			_ProcessName = value;
		}
	}

	public string UrlEncodingExceptions
	{
		get
		{
			return _UrlEncodingExceptions;
		}
		set
		{
			_UrlEncodingExceptions = value;
		}
	}

	public bool NumwantRandomize
	{
		get
		{
			return _NumwantRandomize;
		}
		set
		{
			_NumwantRandomize = value;
		}
	}

	public int NumwantInitialValue
	{
		get
		{
			return _NumwantInitialValue;
		}
		set
		{
			_NumwantInitialValue = value;
		}
	}

	public int NumwantRadius
	{
		get
		{
			return _NumwantRadius;
		}
		set
		{
			_NumwantRadius = value;
		}
	}

	public bool IsDivider
	{
		get
		{
			return _IsDivider;
		}
		set
		{
			_IsDivider = value;
		}
	}

	public TorrentClient(string Name)
	{
		_Name = Name;
	}
}

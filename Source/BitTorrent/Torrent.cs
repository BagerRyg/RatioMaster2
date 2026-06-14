using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;

namespace BitTorrent;

public class Torrent
{
	private Collection<TorrentFile> torrentFiles;

	private ValueDictionary data;

	private string localTorrentFile;

	private long pieceLength;

	private Piece[] pieceArray;

	private byte[] infohash;

	private int pieces;

	private long _totalLength;

	public Collection<TorrentFile> PhysicalFiles => torrentFiles;

	public long totalLength => _totalLength;

	public bool SingleFile => ((ValueDictionary)data["info"]).Contains("length");

	public ValueDictionary Data => data;

	public ValueDictionary Info => (ValueDictionary)data["info"];

	public byte[] InfoHash
	{
		get
		{
			using SHA1 sHA = SHA1.Create();
			return sHA.ComputeHash(data["info"].Encode());
		}
	}

	public string Name
	{
		get
		{
			return BEncode.String(((ValueDictionary)data["info"])["name"]);
		}
		set
		{
			if (!data.Contains("info"))
			{
				data.Add("info", new ValueDictionary());
			}
			((ValueDictionary)data["info"]).SetStringValue("name", value);
		}
	}

	public string Comment
	{
		get
		{
			return BEncode.String(data["comment"]);
		}
		set
		{
			data.SetStringValue("comment", value);
		}
	}

	public string Announce
	{
		get
		{
			return BEncode.String(data["announce"]);
		}
		set
		{
			data.SetStringValue("announce", value);
		}
	}

	public string CreatedBy
	{
		get
		{
			return BEncode.String(data["created by"]);
		}
		set
		{
			data.SetStringValue("created by", value);
		}
	}

	public string Encoding
	{
		get
		{
			return BEncode.String(data["encoding"]);
		}
		set
		{
			data.SetStringValue("encoding", value);
		}
	}

	public int Pieces => pieceArray.Length;

	public Torrent()
	{
		data = new ValueDictionary();
		localTorrentFile = string.Empty;
		torrentFiles = new Collection<TorrentFile>();
	}

	public Torrent(string localFilename)
	{
		torrentFiles = new Collection<TorrentFile>();
		OpenTorrent(localFilename);
	}

	public bool OpenTorrent(string localFilename)
	{
		data = null;
		bool result = false;
		localTorrentFile = localFilename;
		data = new ValueDictionary();
		FileStream fileStream = null;
		BinaryReader binaryReader = null;
		try
		{
			fileStream = File.OpenRead(localFilename);
			if (fileStream.Length <= 0 || fileStream.Length > 64L * 1024L * 1024L)
			{
				throw new InvalidDataException("Torrent metadata file size is invalid.");
			}
			binaryReader = new BinaryReader(fileStream);
			data = (ValueDictionary)BEncode.Parse(binaryReader.BaseStream);
			if (fileStream.Position != fileStream.Length)
			{
				throw new InvalidDataException("Torrent metadata contains trailing data.");
			}
			LoadTorrent();
			result = true;
			binaryReader.Close();
			fileStream.Close();
		}
		catch (IOException)
		{
			result = false;
		}
		finally
		{
			binaryReader?.Close();
			fileStream?.Close();
		}
		return result;
	}

	private void ParsePieceHashes(byte[] hashdata)
	{
		int num = hashdata.Length / 20;
		pieces = 0;
		pieceArray = null;
		for (pieceArray = new Piece[num]; pieces < num; pieces++)
		{
			Piece piece = new Piece(this, pieces);
			pieceArray[pieces] = piece;
		}
	}

	private void LoadTorrent()
	{
		if (!data.Contains("announce"))
		{
			if (!data.Contains("announce-list"))
			{
				throw new IncompleteTorrentData("No tracker URL");
			}
			ValueList valueList = (ValueList)data["announce-list"];
			string value = BEncode.String(((ValueList)valueList.Values[0]).Values[0]);
			data.SetStringValue("announce", value);
		}
		if (!data.Contains("info"))
		{
			throw new IncompleteTorrentData("No internal torrent information");
		}
		ValueDictionary valueDictionary = (ValueDictionary)data["info"];
		pieceLength = ((ValueNumber)valueDictionary["piece length"]).Integer;
		if (pieceLength <= 0)
		{
			throw new IncompleteTorrentData("Piece length must be positive.");
		}
		if (!valueDictionary.Contains("pieces"))
		{
			throw new IncompleteTorrentData("No piece hash data");
		}
		ValueString valueString = (ValueString)valueDictionary["pieces"];
		if (valueString.Length % 20 != 0)
		{
			throw new IncompleteTorrentData("Missing or damaged piece hash codes");
		}
		ParsePieceHashes(valueString.Bytes);
		if (SingleFile)
		{
			ParseSingleFile();
		}
		else
		{
			ParseMultipleFiles();
		}
		infohash = InfoHash;
	}

	private void ParseSingleFile()
	{
		ValueDictionary valueDictionary = (ValueDictionary)data["info"];
		long length = ((ValueNumber)valueDictionary["length"]).Integer;
		if (length < 0)
		{
			throw new IncompleteTorrentData("Torrent file length cannot be negative.");
		}
		_totalLength = length;
		TorrentFile item = new TorrentFile(length, ((ValueString)valueDictionary["name"]).String);
		torrentFiles.Add(item);
	}

	private void ParseMultipleFiles()
	{
		ValueDictionary valueDictionary = (ValueDictionary)data["info"];
		ValueList valueList = (ValueList)valueDictionary["files"];
		torrentFiles = null;
		torrentFiles = new Collection<TorrentFile>();
		foreach (ValueDictionary item2 in valueList)
		{
			ValueList valueList2 = (ValueList)item2["path"];
			bool flag = true;
			string text = "";
			foreach (ValueString item3 in valueList2)
			{
				if (!flag)
				{
					text += "/";
				}
				flag = false;
				text += item3.String;
			}
			long length = ((ValueNumber)item2["length"]).Integer;
			if (length < 0)
			{
				throw new IncompleteTorrentData("Torrent file length cannot be negative.");
			}
			try
			{
				_totalLength = checked(_totalLength + length);
			}
			catch (OverflowException)
			{
				throw new IncompleteTorrentData("Torrent total size is too large.");
			}
			TorrentFile item = new TorrentFile(length, text);
			torrentFiles.Add(item);
		}
	}

	public bool OpenTorrent()
	{
		return OpenTorrent(localTorrentFile);
	}

	public void ClearSensitiveData()
	{
		if (infohash != null)
		{
			Array.Clear(infohash, 0, infohash.Length);
		}
		infohash = null;
		localTorrentFile = string.Empty;
		BEncode.Clear(data);
		data = null;
		pieceArray = null;
		torrentFiles?.Clear();
		_totalLength = 0L;
	}
}

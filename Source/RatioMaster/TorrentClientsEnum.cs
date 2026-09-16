using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ProcessMemoryReaderLib;

namespace RatioMaster;

internal class TorrentClientsEnum
{
	private TorrentClient[] _torrentClients;

	private TorrentClient[] externalClients;

	private TorrentClient[] internalClients;

	private int _clientsNum = 5;

	private Random random = new Random();

	private RandomStringGenerator stringGenerator = new RandomStringGenerator();

	private MainForm _mainForm;

	private static readonly string[] pinnedClientFiles =
	{
		"uTorrent_3.6.0_build_47254.client",
		"BitTorrent_7.11.0_build_47255.client",
		"qBittorrent_5.2.3.client",
		"Vuze_5.7.7.0.client",
		"Deluge_2.2.0.client",
		"rTorrent_ruTorrent_0.16.22_5.3.14.client",
		"Transmission_4.1.3.client",
		"Halite_0.4.0.4.client",
		"BitTyrant_1.1.1.client"
	};

	// Build 74 saved dropdown positions, including its divider. Keep this order for migration.
	private static readonly string[] legacyClientIds =
	{
		"uTorrent_3.6.0_build_47196.client",
		"BitTorrent_7.11.0_build_47235.client",
		"qBittorrent_5.2.3.client",
		"Vuze_5.7.7.0.client",
		"Deluge_2.2.0.client",
		"rTorrent_ruTorrent_0.16.12_5.3.1.client",
		"",
		"Azureus_3050.client",
		"BitComet0107.client",
		"BitComet0113.client",
		"bitlord_1.1.client",
		"BitSpirit_v3.5.0.275.client",
		"BitTorrent 6.0.3 (8642).client",
		"BitTyrant_1.1.client",
		"burst_310b.client",
		"Deluge_1.1.7.client",
		"Deluge_1.1.9.client",
		"Halite 0.3.1.1.client",
		"qBittorrent_5.2.1.client",
		"Transmission_1.06_Build_5136.client",
		"utorrent_1.7.7_build_(8179).client",
		"utorrent_1.8.1_(build_12616).client",
		"utorrent_1.8.1_(build_12639).client",
		"utorrent_1.8.2_(build_15227).client",
		"utorrent_1.8.2_(build_15296).client",
		"utorrent_1.8.2_(build_15357).client",
		"utorrent_1.8.2_build(14153).client",
		"utorrent_1.8.2_build(14458).client",
		"utorrent_1.8.2_build_15167.client",
		"uTorrent_1.8.3_(build_15772).client",
		"uTorrent_1.8.3_(build_16010).client",
		"utorrent_1.8.3_build_15728.client",
		"uTorrent_1.8.4_(build_16286).client",
		"uTorrent_1.8.4_(build_16301).client",
		"utorrent_1.8.4_(build_16667).client",
		"utorrent_1.8.4_(build_16688).client",
		"utorrent_1.8.4_build_(16150).client",
		"utorrent_1.8.5_(build_17091).client",
		"utorrent_1.8.5_(build_17414).client",
		"utorrent_1.8_(build_11813).client",
		"utorrent_2.0.4_(build_21586).client",
		"utorrent_2.0.4_build_21431.client",
		"utorrent_2.0.4_build_21515.client",
		"Vuze_4202.client",
		"Vuze_4204.client",
		"Vuze_4208.client",
		"Vuze_4306.client",
		"Vuze_4404-Fixed.client",
		"Vuze_4500-Fixed.client",
		"internal:Azureus 2.4.0.2",
		"internal:uTorrent 1.6.1  (build 490)",
		"internal:BitComet 0.70"
	};

	public TorrentClient[] TorrentClients => _torrentClients;

	public TorrentClientsEnum(MainForm MainForm)
	{
		_mainForm = MainForm;
		internalClients = EnumerateInternalClients();
		externalClients = EnumerateExternalClients();
		List<TorrentClient> clients = new List<TorrentClient>();
		bool dividerAdded = false;
		for (int i = 0; i < externalClients.Length; i++)
		{
			if (!dividerAdded && GetPinnedClientRank(externalClients[i].ProfileId) == pinnedClientFiles.Length)
			{
				clients.Add(new TorrentClient("----- Legacy -----")
				{
					IsDivider = true
				});
				dividerAdded = true;
			}
			clients.Add(externalClients[i]);
		}
		if (!dividerAdded && internalClients.Length > 0)
		{
			clients.Add(new TorrentClient("----- Legacy -----") { IsDivider = true });
		}
		for (int i = 0; i < internalClients.Length; i++)
		{
			internalClients[i].ProfileId = "internal:" + internalClients[i].Name;
			clients.Add(internalClients[i]);
		}
		_torrentClients = clients.ToArray();
	}

	internal static int ResolveClientIndex(TorrentClient[] clients, string profileId, int legacyIndex)
	{
		if (string.IsNullOrEmpty(profileId) && legacyIndex >= 0 && legacyIndex < legacyClientIds.Length)
		{
			profileId = legacyClientIds[legacyIndex];
		}
		int fallback = -1;
		for (int i = 0; i < clients.Length; i++)
		{
			if (clients[i] == null || clients[i].IsDivider)
			{
				continue;
			}
			if (fallback < 0 || clients[i].ProfileId == "qBittorrent_5.2.3.client")
			{
				fallback = i;
			}
			if (!string.IsNullOrEmpty(profileId) && string.Equals(clients[i].ProfileId, profileId, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return fallback;
	}

	private TorrentClient[] EnumerateInternalClients()
	{
		internalClients = new TorrentClient[3];
		internalClients[0] = new TorrentClient("Azureus 2.4.0.2");
		internalClients[0].Query = "info_hash={infohash}&peer_id={peerid}&port={port}&uploaded={uploaded}&downloaded={downloaded}&left={left}{event}&numwant={numwant}&no_peer_id=1&compact=1&key={key}";
		internalClients[0].Headers = "User-Agent: Azureus 2.4.0.2;Windows XP;Java 1.5.0_04\r\nConnection: close\r\nAccept-Encoding: gzip\r\nHost: {host}\r\nAccept: text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2\r\nContent-type: application/x-www-form-urlencoded\r\n";
		internalClients[0].PeerIDPrefix = "-AZ2402-";
		internalClients[0].PeerID = internalClients[0].PeerIDPrefix + GenerateIdString("alphanumeric", 12, urlencoding: false);
		internalClients[0].Key = GenerateIdString("alphanumeric", 8, urlencoding: false);
		internalClients[0].HttpProtocol = "HTTP/1.1";
		internalClients[0].HashUpperCase = true;
		internalClients[0].ProcessName = "azureus";
		internalClients[1] = new TorrentClient("uTorrent 1.6.1  (build 490)");
		internalClients[1].Query = "info_hash={infohash}&peer_id={peerid}&port={port}&uploaded={uploaded}&downloaded={downloaded}&left={left}&key={key}{event}&numwant={numwant}&compact=1&no_peer_id=1";
		internalClients[1].Headers = "Host: {host}\r\nUser-Agent: uTorrent/1610\r\nAccept-Encoding: gzip\r\n";
		internalClients[1].PeerIDPrefix = "-UT1610-%ea%81";
		internalClients[1].PeerID = internalClients[1].PeerIDPrefix + GenerateIdString("random", 10, urlencoding: true);
		internalClients[1].Key = GenerateIdString("hex", 8, urlencoding: false).ToUpper();
		internalClients[1].HttpProtocol = "HTTP/1.1";
		internalClients[1].HashUpperCase = false;
		internalClients[1].ProcessName = "utorrent";
		internalClients[2] = new TorrentClient("BitComet 0.70");
		internalClients[2].Query = "info_hash={infohash}&peer_id={peerid}&port={port}&uploaded={uploaded}&downloaded={downloaded}&left={left}&numwant=200&compact=1&no_peer_id=1&key={key}{event}";
		internalClients[2].Headers = "Connection: close\r\nHost: {host}\r\nUser-Agent: BitTorrent/3.4.2\r\nAccept-Encoding: gzip, deflate\r\nCache-Control: no-cache\r\n";
		internalClients[2].PeerIDPrefix = "%2DBC0070%2D";
		internalClients[2].PeerID = internalClients[2].PeerIDPrefix + GenerateIdString("random", 12, urlencoding: true, upperCase: true);
		internalClients[2].Key = GenerateIdString("numeric", 5, urlencoding: false);
		internalClients[2].HttpProtocol = "HTTP/1.0";
		internalClients[2].HashUpperCase = true;
		internalClients[2].ProcessName = "bitcomet";
		return internalClients;
	}

	private void ReadIDsFromMemory(TorrentClient[] internalClients)
	{
		_mainForm.AddLogLine("Looking for uTorrent 1.6  (build 474) process...");
		Process[] processesByName = Process.GetProcessesByName("utorrent");
		if (processesByName.Length == 0)
		{
			_mainForm.AddLogLine("No uTorrent process found :(");
			return;
		}
		using ProcessMemoryReader processMemoryReader = new ProcessMemoryReader();
		processMemoryReader.ReadProcess = processesByName[0];
		_mainForm.AddLogLine("uTorrent process found! Checking version and data offsets :)");
		processMemoryReader.OpenProcess();
		string text = "";
		string prefix = "-UT1600-";
		text = ReadMemoryIdString(processMemoryReader, "utorrent_peerid", prefix, 4491832);
		if (text == "")
		{
			_mainForm.AddLogLine("Incompatible uTorrent version or wrong data offsets :(");
			return;
		}
		internalClients[1].PeerID = "-UT1600-" + stringGenerator.urlEncode(text, "_", upperCase: false, lowerCase: true);
		ReadMemoryIdString(processMemoryReader, "utorrent_key", "", 4491828);
		internalClients[1].Key = ReadMemoryIdString(processMemoryReader, "utorrent_key", "", 4491828);
		_mainForm.AddLogLine("Success!!!");
		_mainForm.AddLogLine("Updated " + internalClients[1].Name);
		_mainForm.AddLogLine("Peer ID and key were read without logging their values.");
	}

	private TorrentClient[] EnumerateExternalClients()
	{
		string startupPath = Application.StartupPath;
		string path = Path.Combine(startupPath, "clients");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path, "*.client");
		Array.Sort(files, CompareClientFiles);
		_clientsNum = files.GetUpperBound(0);
		externalClients = new TorrentClient[_clientsNum + 1];
		int num = 0;
		string[] array = files;
		foreach (string text in array)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(text);
			}
			catch (Exception)
			{
				throw new InvalidDataException("Corrupted client file: " + Path.GetFileName(text));
			}
			XmlNode xmlNode = xmlDocument.SelectSingleNode("//client");
			externalClients[num] = new TorrentClient(getAttribute(xmlNode, "name") + " (author " + getAttribute(xmlNode, "author") + ", ver. " + getAttribute(xmlNode, "version") + ")");
			externalClients[num].ProfileId = Path.GetFileName(text);
			externalClients[num].ProcessName = getAttribute(xmlNode, "processname");
			XmlNode xmlNode2 = xmlNode.SelectSingleNode("query");
			externalClients[num].Query = xmlNode2.InnerText;
			XmlNode xmlNode3 = xmlNode.SelectSingleNode("headers");
			externalClients[num].Headers = xmlNode3.InnerText.Replace("_nl_", "\r\n");
			XmlNode xmlNode4 = xmlNode.SelectSingleNode("urlencoding");
			externalClients[num].UrlEncodingExceptions = getAttribute(xmlNode4, "exceptions", "_");
			XmlNode xmlNode5 = xmlNode.SelectSingleNode("peer_id");
			externalClients[num].PeerIDPrefix = getAttribute(xmlNode5, "prefix");
			externalClients[num].PeerID = parseValueFromNode(xmlNode5, externalClients[num].UrlEncodingExceptions);
			XmlNode xmlNode6 = xmlNode.SelectSingleNode("key");
			externalClients[num].Key = parseValueFromNode(xmlNode6, externalClients[num].UrlEncodingExceptions);
			XmlNode xmlNode7 = xmlNode.SelectSingleNode("protocol");
			externalClients[num].HttpProtocol = getAttribute(xmlNode7, "value", "HTTP/1.1");
			XmlNode xmlNode8 = xmlNode.SelectSingleNode("hash");
			string attribute = getAttribute(xmlNode8, "upperCase", "false");
			externalClients[num].HashUpperCase = attribute == "true";
			XmlNode xmlNode9 = xmlNode.SelectSingleNode("numwant");
			externalClients[num].NumwantInitialValue = int.Parse(getAttribute(xmlNode9, "value", "100"));
			externalClients[num].NumwantRadius = int.Parse(getAttribute(xmlNode9, "radius", "10"));
			externalClients[num].NumwantRandomize = getAttribute(xmlNode9, "randomize", "false") == "true";
			num++;
		}
		return externalClients;
	}

	private static int CompareClientFiles(string left, string right)
	{
		int leftRank = GetPinnedClientRank(Path.GetFileName(left));
		int rightRank = GetPinnedClientRank(Path.GetFileName(right));
		if (leftRank != rightRank)
		{
			return leftRank.CompareTo(rightRank);
		}
		return string.Compare(Path.GetFileName(left), Path.GetFileName(right), StringComparison.OrdinalIgnoreCase);
	}

	private static int GetPinnedClientRank(string fileName)
	{
		for (int i = 0; i < pinnedClientFiles.Length; i++)
		{
			if (string.Equals(fileName, pinnedClientFiles[i], StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return pinnedClientFiles.Length;
	}

	private string parseValueFromNode(XmlNode xmlNode, string urlEncodingExceptions)
	{
		string text = getAttribute(xmlNode, "value");
		if (text == "")
		{
			string attribute = getAttribute(xmlNode, "prefix");
			string attribute2 = getAttribute(xmlNode, "suffix");
			string attribute3 = getAttribute(xmlNode, "type", "alphanumeric");
			int keyLength = int.Parse(getAttribute(xmlNode, "length", "12"));
			bool urlencoding = getAttribute(xmlNode, "urlencoding", "true") == "true";
			bool upperCase = getAttribute(xmlNode, "upperCase", "false") == "true";
			bool lowerCase = getAttribute(xmlNode, "lowerCase", "false") == "true";
			text = attribute + GenerateIdString(attribute3, keyLength, urlencoding, upperCase, urlEncodingExceptions, lowerCase) + attribute2;
		}
		return text;
	}

	private string getAttribute(XmlNode xmlNode, string attribName)
	{
		return getAttribute(xmlNode, attribName, "");
	}

	private string getAttribute(XmlNode xmlNode, string attribName, string defValue)
	{
		if (xmlNode == null)
		{
			return defValue;
		}
		XmlNode namedItem = xmlNode.Attributes.GetNamedItem(attribName);
		if (namedItem == null)
		{
			return defValue;
		}
		return namedItem.Value.ToString();
	}

	private string ReadMemoryIdString(ProcessMemoryReader pReader, string stringType, string prefix, int memOffset)
	{
		string text = "";
		string text2 = "";
		try
		{
			int bytesReaded;
			switch (stringType.ToLower())
			{
			case "utorrent_key":
			{
				byte[] bytes = pReader.ReadProcessMemory((IntPtr)memOffset, 4u, out bytesReaded);
				if (bytesReaded != 4)
				{
					Array.Clear(bytes, 0, bytes.Length);
					return "";
				}
				text = $"{bytes[3]:X}{bytes[2]:X}{bytes[1]:X}{bytes[0]:X}";
				Array.Clear(bytes, 0, bytes.Length);
				break;
			}
			case "utorrent_peerid":
			{
				byte[] bytes = pReader.ReadProcessMemory((IntPtr)memOffset, 20u, out bytesReaded);
				if (bytesReaded != 20)
				{
					Array.Clear(bytes, 0, bytes.Length);
					return "";
				}
				text = Encoding.GetEncoding(28591).GetString(bytes);
				Array.Clear(bytes, 0, bytes.Length);
				text2 = text.Replace(prefix, "");
				text = ((text2.Length >= text.Length) ? "" : text2);
				break;
			}
			default:
				text = "";
				break;
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("ClientsEnum : " + ex.Message);
		}
		return text;
	}

	private string GenerateIdString(string keyType, int keyLength, bool urlencoding, bool upperCase, string urlEncodingExceptions, bool lowerCase)
	{
		string text = "";
		text = keyType switch
		{
			"printable" => stringGenerator.Generate(keyLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_.~".ToCharArray()), 
			"alphabetic" => stringGenerator.Generate(keyLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()), 
			"alphanumeric" => stringGenerator.Generate(keyLength), 
			"numeric" => stringGenerator.Generate(keyLength, "0123456789".ToCharArray()), 
			"qbittorrent" or "libtorrent" => stringGenerator.Generate(keyLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~()!.*-".ToCharArray()),
			"transmission" => GenerateTransmissionSuffix(keyLength),
			"random" => stringGenerator.Generate(keyLength, randomness: true), 
			"hex" => stringGenerator.Generate(keyLength, "0123456789abcdef".ToCharArray()), 
			"hexVariable" => GenerateVariableHex(keyLength),
			"hexNoLeadingZero" => stringGenerator.Generate(1, "123456789abcdef".ToCharArray()) + stringGenerator.Generate(keyLength - 1, "0123456789abcdef".ToCharArray()), 
			_ => stringGenerator.Generate(keyLength), 
		};
		if (upperCase && keyType != "random")
		{
			text = text.ToUpper();
		}
		else if (lowerCase && keyType != "random")
		{
			text = text.ToLower();
		}
		if (urlencoding)
		{
			text = stringGenerator.urlEncode(text, urlEncodingExceptions, upperCase, lowerCase);
		}
		return text;
	}

	private string GenerateTransmissionSuffix(int length)
	{
		if (length != 12)
		{
			throw new InvalidDataException("Transmission peer IDs require a 12-character suffix.");
		}
		const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
		char[] suffix = new char[length];
		int sum = 0;
		for (int i = 0; i < length - 1; i++)
		{
			int value = random.Next(256) % alphabet.Length;
			suffix[i] = alphabet[value];
			sum += value;
		}
		suffix[length - 1] = alphabet[(alphabet.Length - sum % alphabet.Length) % alphabet.Length];
		return new string(suffix);
	}

	private string GenerateVariableHex(int length)
	{
		string value = stringGenerator.Generate(length, "0123456789abcdef".ToCharArray()).TrimStart('0');
		return value.Length == 0 ? "0" : value;
	}

	private string GenerateIdString(string keyType, int keyLength, bool urlencoding, bool upperCase, string urlEncodingExceptions)
	{
		return GenerateIdString(keyType, keyLength, urlencoding, upperCase, urlEncodingExceptions, lowerCase: false);
	}

	private string GenerateIdString(string keyType, int keyLength, bool urlencoding, bool upperCase)
	{
		return GenerateIdString(keyType, keyLength, urlencoding, upperCase, "_");
	}

	private string GenerateIdString(string keyType, int keyLength, bool urlencoding)
	{
		return GenerateIdString(keyType, keyLength, urlencoding, upperCase: false);
	}
}

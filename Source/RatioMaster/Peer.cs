using System;
using System.Net;

namespace RatioMaster;

public class Peer
{
	public IPAddress IpAddress;

	public ushort Port;

	public string Peer_ID = "";

	public Peer(byte[] ip, short port)
	{
		IpAddress = new IPAddress(ip);
		Port = (ushort)IPAddress.NetworkToHostOrder(port);
		Peer_ID = "";
	}

	public Peer(string ip, string port, string peer_id)
	{
		try
		{
			IpAddress = IPAddress.Parse(ip);
			Port = ushort.Parse(port);
			Peer_ID = peer_id ?? string.Empty;
		}
		catch (Exception)
		{
		}
	}

	public override string ToString()
	{
		return string.Concat(IpAddress, ":", Port);
	}

	public void ClearSensitiveData()
	{
		Peer_ID = string.Empty;
	}
}

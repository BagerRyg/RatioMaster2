using System.Net;

namespace NatTraversal;

public class PortMappingInfo
{
	private bool enabled;

	private string description;

	private string internalHostName;

	private int internalPort;

	private IPAddress externalIPAddress;

	private int externalPort;

	private string protocol;

	public string InternalHostName => internalHostName;

	public int InternalPort => internalPort;

	public IPAddress ExternalIPAddress => externalIPAddress;

	public int ExternalPort => externalPort;

	public string Protocol => protocol;

	public bool Enabled => enabled;

	public string Description => description;

	public PortMappingInfo(string description, string protocol, string internalHostName, int internalPort, IPAddress externalIPAddress, int externalPort, bool enabled)
	{
		this.enabled = enabled;
		this.description = description;
		this.internalHostName = internalHostName;
		this.internalPort = internalPort;
		this.externalIPAddress = externalIPAddress;
		this.externalPort = externalPort;
		this.protocol = protocol;
	}
}

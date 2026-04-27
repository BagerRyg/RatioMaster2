using System;
using System.Collections;
using System.Net;
using NatTraversal.Interop;
using RatioMaster;

namespace NatTraversal;

public class UPnPNat
{
	private UPnPNAT uPnpNat;

	private Form1 _mainForm;

	public bool Enabled;

	public ArrayList PortMappings
	{
		get
		{
			if (uPnpNat == null)
			{
				return null;
			}
			ArrayList arrayList = new ArrayList();
			int count = uPnpNat.StaticPortMappingCollection.Count;
			IEnumerator enumerator = uPnpNat.StaticPortMappingCollection.GetEnumerator();
			enumerator.Reset();
			for (int i = 0; i <= count; i++)
			{
				IStaticPortMapping staticPortMapping = null;
				try
				{
					if (enumerator.MoveNext())
					{
						staticPortMapping = (IStaticPortMapping)enumerator.Current;
					}
				}
				catch
				{
				}
				if (staticPortMapping != null)
				{
					arrayList.Add(new PortMappingInfo(staticPortMapping.Description, staticPortMapping.Protocol.ToUpper(), staticPortMapping.InternalClient, staticPortMapping.InternalPort, IPAddress.Parse(staticPortMapping.ExternalIPAddress), staticPortMapping.ExternalPort, staticPortMapping.Enabled));
				}
			}
			return arrayList;
		}
	}

	public UPnPNat(Form1 mainForm)
	{
		_mainForm = mainForm;
		try
		{
			UPnPNAT uPnPNAT = (UPnPNAT)new UPnPNATCreator();
			if (uPnPNAT.NATEventManager != null && uPnPNAT.StaticPortMappingCollection != null)
			{
				uPnpNat = uPnPNAT;
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("Error initializing UPnP Nat : " + ex.Message);
		}
		if (uPnpNat == null)
		{
			_mainForm.AddLogLine("No configurable UPnP Nat is available");
		}
		else
		{
			Enabled = true;
		}
	}

	public void AddPortMapping(PortMappingInfo portMapping)
	{
		if (uPnpNat == null || portMapping == null)
		{
			return;
		}
		try
		{
			uPnpNat.StaticPortMappingCollection.Add(portMapping.ExternalPort, portMapping.Protocol, portMapping.InternalPort, portMapping.InternalHostName, portMapping.Enabled, portMapping.Description);
		}
		catch
		{
		}
	}

	public void RemovePortMapping(PortMappingInfo portMapping)
	{
		if (uPnpNat == null || portMapping == null)
		{
			return;
		}
		try
		{
			uPnpNat.StaticPortMappingCollection.Remove(portMapping.ExternalPort, portMapping.Protocol);
		}
		catch
		{
		}
	}
}

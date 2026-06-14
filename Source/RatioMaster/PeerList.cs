using System.Collections.Generic;

namespace RatioMaster;

public class PeerList : List<Peer>
{
	public int maxPeersToShow = 5;

	public int peerCounter;

	public override string ToString()
	{
		return "(" + Count + " peers)";
	}

	public void ClearSensitiveData()
	{
		foreach (Peer peer in this)
		{
			peer.ClearSensitiveData();
		}
		Clear();
	}
}

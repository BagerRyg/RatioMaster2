using System.Collections.Generic;

namespace RatioMaster;

public class PeerList : List<Peer>
{
	public int maxPeersToShow = 5;

	public int peerCounter;

	public override string ToString()
	{
		string text = "";
		text = "(" + base.Count + ") ";
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Peer current = enumerator.Current;
			if (peerCounter < maxPeersToShow)
			{
				text = string.Concat(text, current, ";");
			}
			peerCounter++;
		}
		return text;
	}
}

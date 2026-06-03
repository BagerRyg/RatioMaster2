using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RatioMaster;

internal class PeerListUtils
{
	public static void SavePeerList(MainForm mainForm, PeerList peerList, string torrentName)
	{
		try
		{
			if (peerList.Count == 0)
			{
				return;
			}
			string text = Application.StartupPath + "\\PeerLists";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = text + "\\peerlist_" + Path.GetFileNameWithoutExtension(torrentName) + ".txt";
			using StreamWriter streamWriter = new StreamWriter(text2, append: false, Encoding.UTF8);
			streamWriter.WriteLine("Time: " + DateTime.Now);
			streamWriter.WriteLine("Torrent Path: " + torrentName);
			streamWriter.WriteLine();
			foreach (Peer peer in peerList)
			{
				streamWriter.WriteLine(peer.ToString());
			}
			mainForm.AddLogLine("Saved Peerlist to: " + text2);
		}
		catch (Exception ex)
		{
			mainForm.AddLogLine("Error writing peer list: " + ex.Message);
		}
	}
}

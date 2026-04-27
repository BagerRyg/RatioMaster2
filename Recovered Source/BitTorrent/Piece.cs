using System;
using System.IO;

namespace BitTorrent;

public class Piece
{
	private Torrent torrent;

	private byte[] hash;

	private int pieceNumber;

	public byte[] Bytes
	{
		get
		{
			using FileStream fileStream = new FileStream(torrent.PhysicalFiles[0].Path, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			return binaryReader.ReadBytes((int)fileStream.Length);
		}
	}

	public Torrent Torrent => torrent;

	public byte[] Hash => hash;

	public int PieceNumber => pieceNumber;

	public Piece(Torrent parent, int pieceNumber)
	{
		hash = new byte[20];
		this.pieceNumber = pieceNumber;
		torrent = parent;
		Buffer.BlockCopy(((ValueString)torrent.Info["pieces"]).Bytes, pieceNumber * 20, hash, 0, 20);
	}
}

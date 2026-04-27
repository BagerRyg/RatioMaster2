using System.Collections;
using System.IO;

namespace RatioMaster;

public class FilesDateComparer : IComparer
{
	public int Compare(object x, object y)
	{
		FileInfo fileInfo = new FileInfo((string)x);
		FileInfo fileInfo2 = new FileInfo((string)y);
		if (fileInfo.LastWriteTime == fileInfo2.LastWriteTime)
		{
			return 0;
		}
		if (fileInfo.LastWriteTime > fileInfo2.LastWriteTime)
		{
			return -1;
		}
		return 1;
	}
}

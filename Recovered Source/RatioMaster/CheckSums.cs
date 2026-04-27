using System;
using System.IO;
using System.Security.Cryptography;

namespace RatioMaster;

public class CheckSums
{
	public static string Md5Hash(string sFilePath)
	{
		try
		{
			using MD5 mD5CryptoServiceProvider = MD5.Create();
			using FileStream fileStream = new FileStream(sFilePath, FileMode.Open, FileAccess.Read);
			byte[] array = mD5CryptoServiceProvider.ComputeHash(fileStream);
			return BitConverter.ToString(array).Replace("-", string.Empty);
		}
		catch (Exception)
		{
			return "";
		}
	}
}

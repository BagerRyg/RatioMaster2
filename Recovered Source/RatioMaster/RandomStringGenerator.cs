using System;
using System.Text;

namespace RatioMaster;

public class RandomStringGenerator
{
	private char[] characterArray;

	private Random randNum = new Random();

	public RandomStringGenerator()
	{
		characterArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
	}

	private char GetRandomCharacter()
	{
		return characterArray[(int)((double)(characterArray.GetUpperBound(0) + 1) * randNum.NextDouble())];
	}

	public string Generate(int stringLength)
	{
		return Generate(stringLength, randomness: false);
	}

	public string Generate(int stringLength, bool randomness)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = stringLength;
		for (int i = 0; i <= stringLength - 1; i++)
		{
			if (randomness)
			{
				stringBuilder.Append((char)randNum.Next(255));
			}
			else
			{
				stringBuilder.Append(GetRandomCharacter());
			}
		}
		return stringBuilder.ToString();
	}

	public string Generate(int stringLength, char[] characterArray)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Capacity = stringLength;
		for (int i = 0; i <= stringLength - 1; i++)
		{
			stringBuilder.Append(characterArray[(int)((double)(characterArray.GetUpperBound(0) + 1) * randNum.NextDouble())]);
		}
		return stringBuilder.ToString();
	}

	public string urlEncode(string str, string urlEncodingExceptions, bool upperCase, bool lowerCase)
	{
		string text = "";
		string text2 = "";
		for (int i = 0; i < str.Length; i++)
		{
			if ((char.IsLetterOrDigit(str[i]) && str[i] <= '\u007f') || urlEncodingExceptions.IndexOf(str[i]) != -1)
			{
				text += str[i];
				continue;
			}
			text += "%";
			text2 = Convert.ToString(str[i], 16);
			if (upperCase)
			{
				text2 = text2.ToUpper();
			}
			else if (lowerCase)
			{
				text2 = text2.ToLower();
			}
			text = ((text2.Length != 1) ? (text + text2) : (text + "0" + text2));
		}
		return text;
	}
}

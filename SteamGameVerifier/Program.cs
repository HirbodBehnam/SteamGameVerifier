using System.Security.Cryptography;
using System.Text.RegularExpressions;

class Program
{
	public static void Main(string[] args)
	{
		// Check arguments
		if (args.Length != 2)
		{
			Console.WriteLine("Please pass the manifest file as first argument and game path as second argument.");
			Environment.Exit(1);
		}
		// Open the manifest
		using StreamReader input = new(args[0]);
		using StreamWriter missingFilesOutput = new(args[0] + "-missing.txt");
		// Read until we reach the files section
		string? line;
		while (true)
		{
			line = input.ReadLine();
			if (line == null)
			{
				Console.WriteLine("Reached end of file without reading the headers!");
				Environment.Exit(1);
			}
			if (line == "          Size Chunks File SHA                                 Flags Name")
				break;
		}
		// Now read files
		int totalFiles = 0, sizeMismatchFiles = 0, badFiles = 0, missingFiles = 0;
		while ((line = input.ReadLine()) != null)
		{
			// Do nothing if line is empty
			if (string.IsNullOrWhiteSpace(line))
				continue;
			// Parse the line
			(long size, byte[] sha1, int flags, string name) = ParseLine(line);
			if (flags == 64) // skip folders
				continue;
			totalFiles++;
			// Check the file
			try
			{
				// Check size before checking the hash
				string path = Path.Combine(args[1], name);
				long realFileSize = new FileInfo(path).Length;
				if (realFileSize != size)
				{
					PrintLog($"Size mismatch on {name}. Expected {size} got {realFileSize}.", ConsoleColor.Red);
					missingFilesOutput.WriteLine(name);
					sizeMismatchFiles++;
					continue;
				}
				// Check hash
				if (!CheckChecksum(path, sha1))
				{
					PrintLog($"SHA1 mismatch on {name}.", ConsoleColor.Red);
					missingFilesOutput.WriteLine(name);
					badFiles++;
					continue;
				}
			}
			catch (Exception ex)
			{
				PrintLog("Cannot get the file: " + ex.Message, ConsoleColor.Red);
				missingFilesOutput.WriteLine(name);
				missingFiles++;
			}
		}
		// Done
		PrintLog($"Analyzed {totalFiles} files. Got {missingFiles} missing file and {sizeMismatchFiles} size mismatched files and {badFiles} bad files.", ConsoleColor.Green);
	}

	/// <summary>
	/// This function will parse one line of manifest
	/// </summary>
	/// <param name="line">The line to parse</param>
	/// <returns>Data which the line holds</returns>
	/// <exception cref="InvalidDataException">If the line is invalid</exception>
	private static (long size, byte[] sha1, int flags, string name) ParseLine(string line)
	{
		// Split the data
		var splitLine = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ', 5);
		if (splitLine.Length != 5)
			throw new InvalidDataException("split size is not 5");
		// Parse inputs
		long size = long.Parse(splitLine[0]);
		byte[] sha1 = Convert.FromHexString(splitLine[2]);
		int flags = int.Parse(splitLine[3]);
		return (size, sha1, flags, splitLine[4]);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="expectedSha1"></param>
	/// <returns></returns>
	private static bool CheckChecksum(string filename, byte[] expectedSha1)
	{
		using FileStream fs = new(filename, FileMode.Open);
		using SHA1 sha = SHA1.Create();
		byte[] checksum = sha.ComputeHash(fs);
		return checksum.SequenceEqual(expectedSha1);
	}

	/// <summary>
	/// Logs a message to console with given color as text
	/// </summary>
	/// <param name="message">The message to print</param>
	/// <param name="color">Text color</param>
	private static void PrintLog(string message, ConsoleColor color)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(message);
		Console.ResetColor();
	}
}
using System;
using System.IO;
using System.Text;

class Inputs
{
	private static readonly Random RANDOM = new Random();
	public static readonly string EULER13_INPUT_PATH = Path.Combine("examples", "euler13.txt");

	public static string GetUniformInputFromArgs(string[] args)
	{
		string numberListInput = null;
		string cliInput;

		//If no input given, use default euler13 example
		cliInput = args.Length == 0 ? EULER13_INPUT_PATH : args[0];

		bool isExistingFileInput = !string.IsNullOrEmpty(cliInput) &&
			cliInput.IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
			File.Exists(Path.Combine(Environment.CurrentDirectory, cliInput));

		if (isExistingFileInput)
		{
			numberListInput = System.IO.File.ReadAllText(Path.Combine(Environment.CurrentDirectory, cliInput));
		}
		else if (int.TryParse(cliInput, out int numLinesToGenerate))
		{
			numberListInput = ArbitraryNumLines(numLinesToGenerate);
			// Console.WriteLine($"*** Dumping Generated Input ({numLines} lines of {Consts.NUM_STRING_LEN}-digit numbers) ***");
			// Console.WriteLine(input.Trim());
			// Console.WriteLine($"*** Finished Dumping Generated Input ({numLines} lines of {Consts.NUM_STRING_LEN}-digit numbers) ***");
		}

		if (numberListInput != null)
		{
			numberListInput = numberListInput.Trim() + Environment.NewLine; //All lines should end in a newline, so they're uniform
		}
		return numberListInput;
	}

	private static string ArbitraryNumLines(int numLines)
	{
		//Do not use for cryptography, pls.
		Console.Write($"Generating Random Input ({numLines} lines of {Consts.NUM_STRING_LEN}-digit numbers)... ");
		StringBuilder retStringBuilder = new StringBuilder(numLines * Consts.LINE_LEN);
		using (var progress = new ProgressBar()) {
			for (int lineIdx = 0; lineIdx < numLines; lineIdx++)
			{
				for (int charIdx = 0; charIdx < Consts.NUM_STRING_LEN; charIdx++)
				{
					retStringBuilder.Append(RANDOM.Next(0, 10).ToString());
				}
				progress.Report((double) (lineIdx + 1) / numLines);
				retStringBuilder.Append(Environment.NewLine);
			}
		}
		Console.WriteLine("Done.");
		return retStringBuilder.ToString();
	}
}
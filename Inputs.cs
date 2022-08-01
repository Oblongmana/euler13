using System;
using System.Text;

class Inputs
{
	private static readonly Random RANDOM = new Random();

	public static string GetUniformInputFromArgs(string[] args)
	{
		string input;
		if (args.Length == 0)
		{
			input = Consts.EULER13_INPUT;
		}
		else
		{
			int numLines;
			input = int.TryParse(args[0], out numLines) ? ArbitraryNumLines(numLines) : Consts.EULER13_INPUT;
			// Console.WriteLine($"*** Dumping Generated Input ({numLines} lines of {Consts.NUM_STRING_LEN}-digit numbers) ***");
			// Console.WriteLine(input.Trim());
			// Console.WriteLine($"*** Finished Dumping Generated Input ({numLines} lines of {Consts.NUM_STRING_LEN}-digit numbers) ***");
		}
		return input.Trim() + Environment.NewLine; //All lines should end in a newline, so they're uniform
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
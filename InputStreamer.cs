using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class InputStreamer
{
	public const string HELP_OUTPUT_IDENTIFIER = "Euler13++ Help";

	private static readonly string EULER13_INPUT_PATH = Path.Combine("examples", "euler13.txt");
	private static readonly int COMPILE_TIME_RANDOM_SEED = new Random().Next();

	private readonly ProgressReporter progressReporter;


	public InputStreamer(ProgressReporter progressReporter) => this.progressReporter = progressReporter;


	public IEnumerable<string> GetStreamingInputFromArgs(string[] args)
	{
		IEnumerable<string> numberListInput = null;
		string cliInput;
		bool confirmedNonFileInput = false;

		//If no input given, use default euler13 example
		cliInput = args.Length == 0 ? EULER13_INPUT_PATH : args[0];

		//Check if help needed
		if (cliInput == "help")
		{
			List<string> help = new List<string>();
			help.Add(HELP_OUTPUT_IDENTIFIER);
			help.Add("    Calculate the sum of chonky numbers.");
			help.Add("");
			help.Add("Limitations:");
			help.Add("      Input numbers must contain fewer than ~2^30 characters (usually -33 or -56 depending on system), all positive natural numbers.");
			help.Add("      The total sum also has this length limit.");
			help.Add("");
			help.Add("Input Options ");
			help.Add("    dotnet run");
			help.Add("        Calculate the sum of the Euler13 data set.");
			help.Add("    dotnet run [FILE_PATH]");
			help.Add("        Calculate the sum of the positive natural numbers in that file. Newline separated, no empty lines.");
			help.Add("    dotnet run [NUMBER_OF_LINES] [OPTIONAL_WIDTH_OF_NUMBER]");
			help.Add("        Sum [NUMBER_OF_LINES] random numbers. The number will have [OPTIONAL_WIDTH_OF_NUMBER] digits, or 50 if not supplied.");
			help.Add("    dotnet run [NAME_OF_BUILT_IN_GENERATOR] [OPTIONAL_PARAMETER]");
			help.Add("        Various poorly named other generators are available, such as Pow10Generator, or ArbitraryLength9sPlus1. Examine the code for these.");
			help.Add("    dotnet run help");
			help.Add("        Shows this help screen! Congrats, you did it!");
			return help;
		}

		//If a special generator is indicated by user, use that
		void HandleSpecialGenerator(Func<int,IEnumerable<string>> parameterisedGenerator, int defaultVal)
		{
			confirmedNonFileInput = true;

			int param = 0;
			bool parsedParam = args.Length > 1 ? int.TryParse(args[1], out param) : false;
			numberListInput = parsedParam ? parameterisedGenerator(param) : parameterisedGenerator(defaultVal);
		}

		if (cliInput == "IntLimit9sGenerator")
		{
			HandleSpecialGenerator(IntLimit9sGenerator, 1);
			progressReporter.ReportArbitraryGenerationStart("IntLimit9sGenerator", null, null);
		}
		else if (cliInput == "Pow10Generator")
		{
			HandleSpecialGenerator(Pow10Generator, 15);
			progressReporter.ReportArbitraryGenerationStart("Pow10Generator", null, null);
		}
		else if (cliInput == "ArbitraryLength8sPlus1")
		{
			HandleSpecialGenerator(ArbitraryLength8sPlus1, 1);
			progressReporter.ReportArbitraryGenerationStart("ArbitraryLength8sPlus1", 2, null);
		}
		else if (cliInput == "ArbitraryLength9sPlus1")
		{
			HandleSpecialGenerator(ArbitraryLength9sPlus1, 1);
			progressReporter.ReportArbitraryGenerationStart("ArbitraryLength9sPlus1", 2, null);
		}
		else if (cliInput == "ArbitraryLength9sTwice")
		{
			HandleSpecialGenerator(ArbitraryLength9sTwice, 1);
			progressReporter.ReportArbitraryGenerationStart("ArbitraryLength9sTwice", 2, null);
		}

		bool isExistingFileInput = !confirmedNonFileInput &&
			!string.IsNullOrEmpty(cliInput) &&
			cliInput.IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
			File.Exists(Path.Combine(Environment.CurrentDirectory, cliInput));

		if (isExistingFileInput)
		{
			numberListInput = File.ReadLines(Path.Combine(Environment.CurrentDirectory, cliInput));
		}
		else if (int.TryParse(cliInput, out int numLinesToGenerate))
		{
			int digitsPerLine = 0;
			bool parsedDigitsPerLine = args.Length > 1 ? int.TryParse(args[1], out digitsPerLine) : false;
			digitsPerLine = parsedDigitsPerLine ? digitsPerLine : 50; //Backup is the standard euler13 width = 50
			numberListInput = ArbitraryNumLines(numLinesToGenerate, digitsPerLine);
			progressReporter.ReportArbitraryGenerationStart("ArbitraryNumLines", numLinesToGenerate, digitsPerLine);
		}

		return numberListInput;
	}

	private IEnumerable<string> ArbitraryNumLines(int numLines, int digitsPerLine)
	{
		//Do not use for cryptography, pls.
		StringBuilder lineBuilder = new StringBuilder(digitsPerLine);
		Random random = new Random(COMPILE_TIME_RANDOM_SEED);

		for (int lineIdx = 0; lineIdx < numLines; lineIdx++)
		{
			for (int charIdx = 0; charIdx < digitsPerLine; charIdx++)
			{
				//NB the "random" generation seed is fixed at compile time, and repeatable so it returns the
				//  same results if the IEnumerable gets hit multiple times. We don't want to actually store the generated
				//  values because we might generate 10s of millions of them
				lineBuilder.Append(random.Next(0, 10).ToString());
			}
			yield return lineBuilder.ToString();
			lineBuilder.Clear();
		}
	}

	private IEnumerable<string> IntLimit9sGenerator(int modification)
	{
		for (int i = 0; i < int.MaxValue + (modification < 0 ? modification : 0); i++)
		{
			yield return "9";
		}

		//If we're testing adding qtys of numbers past the int maxValue, do another loop!
		if (modification > 0)
		{
			for (int i = 0; i < modification; i++)
			{
				yield return "9";
			}
		}
	}

	private IEnumerable<string> Pow10Generator(int maxStrLenIs2ToPowerOf)
	{
		int maxPowerOf10 = (int)(Math.Pow(2, maxStrLenIs2ToPowerOf) - 1); //- 1 as e.g. 10^(2^4) produces a number with 2^4 zeroes, so total len 2^4 + 1
		string currPow10String = "1";
		for (int currPowerOf10 = 0; currPowerOf10 < maxPowerOf10; currPowerOf10++)
		{
			currPow10String += "0";
			yield return currPow10String;
		}
	}

	private IEnumerable<string> ArbitraryLength8sPlus1(int length)
	{
		yield return new string('8', length);
		yield return "1";
	}

	private IEnumerable<string> ArbitraryLength9sPlus1(int length)
	{
		yield return new string('9', length);
		yield return "1";
	}

	private IEnumerable<string> ArbitraryLength9sTwice(int length)
	{
		yield return new string('9', length);
		yield return new string('9', length);
	}
}
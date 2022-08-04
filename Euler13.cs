#pragma warning disable CS0162 //Ignore unreachable code warnings - this is just complaing about debugs that can't be reached depending on reporting mode
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GnarlyStrings;

class Euler13
{
	//Self-imposed limitations? Features? ¯\_(ツ)_/¯
	// - store things in strings except when doing trivial sums (int only used for loop indexes, and summing single digits in two lines (number1Digit + number2Digit + remainderFromPreviousSum))
	//   - no existing arbitrary-precision data structures or libraries or anyuthing like that, that's cheating!
	// - input should only be positive natural numbers - there's a limit to my masochism. I may extend this later though :)
	// - don't assume anything about the total quantity of input numbers
	// - don't assume anything about the the significant digit count of individual input numbers
	// - streaming input (and random generation if using that option instead), so we can have truly huge inputs
	//
	// - Input methods:
	//    - if an int is supplied, uses a stream of that many 50-digit random numbers for input
	//    - can accept a file path for streaming input.
	//    - uses the euler13 list as input if no args supplied.
	// - So our only limit is therefore that Total Sum (and any individual input number) must have <= ~2^30 significant figures in it
	//    - For all limit notes, cf. https://stackoverflow.com/questions/140468, https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/gcallowverylargeobjects-element?redirectedfrom=MSDN
	//    - this is the limit on any object being larger than the 2gb limit, with c# strings being UTF-16=2 bytes per char = 2^30 char limit
	//    - Theoretically, with gcAllowVeryLargeObjects=true, we might be able to do 2^31? but I'm not testing it. A string with 2,147,483,648 chars in it, wowee.
	//    - This is probably trivially obvious as well, but that limit applies to individual input lines too, given that they must be positive natural numbers
	//    - Experimentally (see detailed notes in README.md), the practical limit for CLR reasons is a sum with digit count of 2^30 - 33, 2^30 - 56, depending on something, probably PC architecture related.
	//
	// - Have externally verified the validity of various results: 1 million x 50 digit numbers, 2 x 1 million digit numbers, Millionth triangle number, etc. Some examples can be plugged in out of the `examples` directory

	//TODO: the ultimate optimisation would probably be to write `unsafe` code to mutate the sum string. Dispensing with GnarlyStrings would be nice. Could be a fun aside at some point
	//TODO: Maybe generalise the random generation to also allow GENERATING arbitrary width as well? We can now handle
	//TODO CLI help
	//TODO allow reporting mode setting from args

	static void Main(string[] args)
	{
		// Begin reporting
		const ProgressReporter.ReportingMode REPORTING_MODE = ProgressReporter.ReportingMode.NO_INTERMEDIATE_OUTPUT;
		ProgressReporter progressReporter = new ProgressReporter(REPORTING_MODE);

		//Begin parsing/generating input
		InputStreamer inputStreamer = new InputStreamer(progressReporter);
		IEnumerable<string> streamingInput = inputStreamer.GetStreamingInputFromArgs(args);

		if (streamingInput == null)
		{
			Console.WriteLine("Error: null or invalid input");
			return;
		}
		else if (streamingInput.Any() && streamingInput.First() == InputStreamer.HELP_OUTPUT_IDENTIFIER)
		{
			foreach (string helpLine in streamingInput)
			{
				Console.WriteLine(helpLine);
			}
			return;
		}

		//Some functions to improve readability for conversion and slicing significant digits
		int StrToInt(string numString) => numString.Length == 0 ? 0 : int.Parse(numString); //int.Parse, but "" is 0 instead of an exception
		int CharToInt(char numChar) => (int)Char.GetNumericValue(numChar); //just lazy/readable writing really :)
		char LeastSignificantDigit(string numString) => numString.Length == 0 ? '0' : numString[numString.Length - 1]; //e.g. "12345" -> "5","" -> "0"
		string DropLeastSignificantDigit(string numString) => numString.Remove(numString.Length - 1); //e.g. "12345" -> "1234", "" -> exception


		//We're going to do classroom column-summing, carrying remainders over and all. This lets us sum huge numbers that
		//  well exceed C# numeric types' ability to hold the actual nubmers. We're limited only by string length maximums.

		//Set initial state to be the first line. We're not going to validate inputs. Simply provide valid inputs :)
		//NB: not using StringBuilder here - performance impact of massively repeated index-based access on a StringBuilder is probably worse than using our
		//    own simple data structure backed by a mutable list of chars. Have not tested that theory - but the docs VERY much warn against index-access on string builders
		MutableReverseIndexedPrependOptimisedString fullSum = new MutableReverseIndexedPrependOptimisedString(streamingInput.Take(1).FirstOrDefault());
		progressReporter.ReportProcessingInputNumber(fullSum);

		//Add each line to the full sum one at a time - streaming avoids memory issues from holding the entire series of numbers at once
		foreach (ReverseIndexedString inputLine in streamingInput.Skip(1).Select(str => new ReverseIndexedString(str))) //skip first line as we already used it as our start value
		{
			progressReporter.ReportProcessingInputNumber(inputLine);

			string carryString = "0";

			int maxStrLength = Math.Max(fullSum.Length, inputLine.Length);
			int minStrLength = Math.Min(fullSum.Length, inputLine.Length);

			//Run through each column of chars, starting at least significant (nb: we're using special datastructures so [0] is the last position in the string)
			for (int charIdx = 0; charIdx < maxStrLength; charIdx++)
			{
				//Optimisation: shortcut column loop if fullSum or inputLine is longer than the other (and we're done carrying). If inputLine is longer, we can just copy remaining digits
				//  See OptimisationAndLimitPushingNotes.md for slightly more detailed notes if needed
				if (charIdx >= minStrLength && carryString == "0")
				{
					if (inputLine.Length >= fullSum.Length)
					{
						fullSum.Prepend(inputLine, inputLine.Length - charIdx);
					}
					break;
				}

				//Deal with carries
				int columnSum = CharToInt(LeastSignificantDigit(carryString)); // start with least-significant-digit of carry value

				//Can be a mismatch between widths e.g.
				//   1000   (fullSum - say as a result of adding 500+500 previously)
				// +  400   (inputLine)
				//   ^      (inputLine doesn't have value in thousands column)
				bool fullSumHasColumn = charIdx < fullSum.Length;
				bool inputHasColumn = charIdx < inputLine.Length;

				//Add the value from the inputLine and the existing sum
				columnSum += fullSumHasColumn ? CharToInt(fullSum[charIdx]) : 0;
				columnSum += inputHasColumn ? CharToInt(inputLine[charIdx]) : 0;

				//Add a new column to the fullSum string, or set the appropriate index in the fullSum string
				string colSumString = columnSum.ToString();
				if (!fullSumHasColumn)
				{
					fullSum.Prepend(LeastSignificantDigit(colSumString));
				}
				else
				{
					fullSum[charIdx] = LeastSignificantDigit(colSumString);
				}

				//Update our carry number string
				carryString = (StrToInt(DropLeastSignificantDigit(carryString)) + StrToInt(DropLeastSignificantDigit(colSumString))).ToString();
			}

			//Each time we finish summing columns, if we have a carry string, prepend it
			if (carryString.Length > 0 && carryString != "0")
			{
				fullSum.Prepend(carryString);
			}
		}

		progressReporter.ReportFinalOutcome(fullSum);
	}
}

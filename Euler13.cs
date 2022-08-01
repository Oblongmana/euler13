using System;
using System.Linq;

class Euler13
{
	//Self-imposed limitations? Features? ¯\_(ツ)_/¯
	// - store things in strings except when doing trivial sums (int only used for loop indexes, and summing a "column" of single-digits)
	// 	  - no arbitrary-precision data structures, that's cheating!
	// - don't assume the quantity of input numbers - this should (?) work with an arbitrary* quantity of 50-digit numbers
	//    - *Upper limit on input is: the number of input characters must be less than/eq to 2^31 (max String length), so (?)
	//        -> (InputNumbers * (NumberLength[i.e. 50] + NewlineCharLen[assume Windows CRLF, so 2])) <= 2^31
	//        -> InputNumbers * (50 + 2) <= 2^31  ->   InputNumbers <= 2^31 / (50 + 2)
	//        -> InputNumbers <= ~41,297,762 (maybe)
	//        -> Though actually apparently there's a 2gb limit, so more like:
	//           -> InputNumbers <= ~20,648,881 ~= 1,073,741,823 / 52 (cf. https://stackoverflow.com/questions/140468/)
	//    - I've tested it with 20,000,000 lines of 50-digit chars. It works great - but generating the inputs takes forever (actually, way better w stringbuilder. Dumping output takes forever though!).
	//        - I will not test it with 41 million numbers. Probably
	//        - I probably will actually, maybe using Stringbuilder in main algo though? Might not be useful actually
	//TODO: given we don't modify the input, we can probs unsafe access it for more speeeed https://stackoverflow.com/questions/13179554/is-it-faster-to-access-char-in-string-via-operator-or-faster-to-access-char-i
	// - uses the euler13 list as input if no args (or invalid args) supplied; or if an int is supplied, generates that many 50-digit numbers and uses that as input
	// - in addition to dumping the euler13 expected output (first 10 digits of sum), also dumps the full sum, and the randomly generated input (if used) so you can verify the solution externally.
	//TODO: stringbuilders. Added to generator so far. May not bring that much efficiency elsewhere
	//TODO: generalise to arbitrary width too?????
	static void Main(string[] args)
	{
		string uniformInput = Inputs.GetUniformInputFromArgs(args);
		string fullSumString = "";
		string carryString = "";

		int StrToInt(string carryableString) => carryableString == "" ? 0 : int.Parse(carryableString); //int.Parse, but "" is 0 instead of an exception

		//run through each column of chars backwards. We're going to do classroom column-summing. This lets us sum huge numbers that well-exceed C# numeric type bounds. We're limited only by string length maximums (hopefully)
		for (int charIdx = Consts.NUM_STRING_LEN - 1; charIdx >= 0 ; charIdx--)
		{
			int columnSum = carryString.Length > 0 ? (int)Char.GetNumericValue(carryString.Last()) : 0; //seed curr colSum with the least significant digit
			carryString = carryString.Length > 0 ? carryString.Remove(carryString.Length - 1) : carryString; // drop least significant digit from running remainder/carry string

			//Sum curr col values, line by line
			for (int lineIdx = 0; lineIdx < (uniformInput.Length / Consts.LINE_LEN); lineIdx++)
			{
				columnSum += (int)Char.GetNumericValue(uniformInput[(lineIdx * Consts.LINE_LEN) + charIdx]);
			}

			string colSumString = columnSum.ToString();
			fullSumString = colSumString[colSumString.Length - 1] + fullSumString;
			carryString = (StrToInt(carryString) + StrToInt(colSumString.Substring(0, colSumString.Length - 1))).ToString(); //wow so readable. Adds the existing carry/remainder with the carry/remainder for this col
		}

		fullSumString = carryString + fullSumString;
		Console.WriteLine();
		Console.WriteLine($"First 10 digits of Sum: {fullSumString.Substring(0,10)}");
		Console.Write($"Full Sum: ");
		Console.WriteLine(fullSumString);
	}
}
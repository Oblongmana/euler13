using System;
using System.Collections.Generic;
using System.Linq;

namespace GnarlyStrings
{
	/// <summary>
	/// <para>
	/// Fairly simple List≪char≫ wrapper, optimised for "prepending" values, and indexed such that 0 is the right-most char in the string
	/// </para>
	/// <para>
	/// Prepend and [index] access will be space and time efficient. ToString() will probably cause
	/// triple allocation as the internal List≪char≫ turned into a char array, then a new string - do this infrequently.
	/// Initial construction also causes double-allocation (converting to a List) - don't hang onto the original input.
	/// </para>
	/// <para>
	/// Internally, we use a reversed List≪char≫, so the "Prepend" operation simply adds to the List: O(1), no temporary allocations
	/// The [indexer] just naively indexes that, meaning [0] is the right-most char in the string this represents.
	/// ToString() gives the string back in the normal order.
	/// </para>
	///
	/// TODO there may be yet more efficient ways to do this, this just hurts brain less to think about
	/// </summary>
	class MutableReverseIndexedPrependOptimisedString //lmao
	{
		public int Length { get => stringChars.Count; }
		private readonly List<char> stringChars;


		public MutableReverseIndexedPrependOptimisedString(string theString) => stringChars = theString.AsEnumerable().Reverse().ToList();


		public void Prepend(char theChar) => stringChars.Add(theChar);
		public void Prepend(string theString) => stringChars.AddRange(theString.AsEnumerable().Reverse());
		public void Prepend(ReverseIndexedString reverseIndexedString, int leadingCharsToCopy)
		{
			stringChars.AddRange(
				Enumerable.Range(reverseIndexedString.Length - leadingCharsToCopy, leadingCharsToCopy)
					.Select(charIdx => reverseIndexedString[charIdx])
			);
		}
		/// <summary>Take care when using this - bear in mind that a string can be 2^30 characters long, maybe even 2^31 in special modes</summary>
		public override string ToString() => new string(stringChars.AsEnumerable().Reverse().ToArray());
		/// <summary>Take care when using this - bear in mind that a string can be 2^30 characters long, maybe even 2^31 in special modes</summary>
		public string UpToFirstNDigits(int numDigits) => new string(stringChars.AsEnumerable().Reverse().Take(Math.Min(numDigits, stringChars.Count)).ToArray());

		public char this[int index]
		{
			get => stringChars[index];
			set => stringChars[index] = value;
		}
	}

	/// <summary>
	/// Convenience class for readably reverse-indexing a string: [0] is the last char, [length] is the first char
	/// </summary>
	class ReverseIndexedString
	{
		public int Length { get => theString.Length; }
		private readonly string theString;

		public ReverseIndexedString(string theString) => this.theString = theString;


		public override string ToString() => theString;

		public char this[int index]
		{
			get => theString[theString.Length - index - 1];
		}
	}
}
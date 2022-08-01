using System;

class Consts
{
	#region Ugly Consts and Pseudo-consts Yuck
	public static readonly int NEWLINE_CHAR_LEN = Environment.NewLine.Length;
	public const int NUM_STRING_LEN = 50;
	public static readonly int LINE_LEN = NEWLINE_CHAR_LEN + NUM_STRING_LEN;
	#endregion
}
using System;
using System.Diagnostics;

class ProgressReporter
{
	/// <summary>
	///!!!!!<br/>
	/// NOTE: Using any mode other than NO_INTERMEDIATE_OUTPUT has significant performance impact.<br/>
	///  - 1 million random lines in NO_INTERMEDIATE_OUTPUT took 00h:00m:08s.44ms<br/>
	///  - 1 million random lines in REPORT_PROGRESS took 00h:00m:38s.40ms:42s.94ms<br/>
	///  - 1 million random lines in DUMP_VALUES took 00h:00m:11s.15ms (when redirecting output to file, I don't dare try letting it go direct to terminal - that will def. perform absymally)<br/>
	///!!!!!
	/// </summary>
	public enum ReportingMode { DUMP_VALUES, REPORT_PROGRESS, NO_INTERMEDIATE_OUTPUT }

	private readonly ReportingMode reportingMode = ReportingMode.NO_INTERMEDIATE_OUTPUT;
	private readonly Stopwatch stopWatch;
	private int? knownExpectedLines;
	private int? knownFixedWidthNumbers;
	private string approachDescription;
	private int processedCount = 0;

	/// <inheritdoc cref="ReportingMode"/>
	public ProgressReporter(ReportingMode reportingMode = ReportingMode.NO_INTERMEDIATE_OUTPUT)
	{
		this.reportingMode = reportingMode;
		stopWatch = new Stopwatch();
		stopWatch.Start();

		if (reportingMode == ReportingMode.REPORT_PROGRESS)
		{
			Console.WriteLine("Reporting in REPORT_PROGRESS mode. Be aware this operates ~5 times slower than NO_INTERMEDIATE_OUTPUT due to constant updating of step info");
		}
		else if (reportingMode == ReportingMode.DUMP_VALUES)
		{
			Console.WriteLine("Reporting in DUMP_VALUES mode. HIGHLY RECOMMEND AGAINST OUTPUTTING TO TERMINAL IN THIS MODE. If redirecting output to a file, this has been observed to operate ~1.35 times slower than NO_INTERMEDIATE_OUTPUT. Results may vary depending on system.");
		}
	}

	public void ReportArbitraryGenerationStart(string approachDescription, int? lines, int? width)
	{
		knownFixedWidthNumbers = width;
		knownExpectedLines = lines;
		this.approachDescription = approachDescription;
		if (reportingMode != ReportingMode.NO_INTERMEDIATE_OUTPUT)
		{
			Console.WriteLine($"Generating {(knownExpectedLines != null ? $"{knownExpectedLines} lines " : "")}{(knownFixedWidthNumbers != null ? $"with {knownFixedWidthNumbers} digits each " : "")}using {approachDescription}");
		}
	}

	public void ReportProcessingInputNumber(object inputNumber)
	{
		processedCount++;

		if (reportingMode == ReportingMode.DUMP_VALUES)
		{
			Console.WriteLine(inputNumber);
		}
		else if (reportingMode == ReportingMode.REPORT_PROGRESS)
		{
			TimeSpan elapsed = stopWatch.Elapsed;
			string elapsedString = String.Format("{0:00}h:{1:00}m:{2:00}s.{3:00}ms", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10);
			Console.Write($"\rProcessed {(knownExpectedLines != null ? $"{Math.Round((processedCount / (double)knownExpectedLines) * 100d)} % of lines" : $"{processedCount} lines")} in {elapsedString}");
		}
	}

	public void ReportFinalOutcome(GnarlyStrings.MutableReverseIndexedPrependOptimisedString fullSum)
	{
		if (reportingMode != ReportingMode.REPORT_PROGRESS)
		{
			TimeSpan elapsed = stopWatch.Elapsed;
			string elapsedString = String.Format("{0:00}h:{1:00}m:{2:00}s.{3:00}ms", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10);
			Console.WriteLine($"\rProcessed {processedCount} lines {(knownFixedWidthNumbers != null ? $"with {knownFixedWidthNumbers} digits each " : "")}{(approachDescription != null ? $"using {approachDescription} " : "")}in {elapsedString}");
		}
		else
		{
			Console.WriteLine("\r");
		}
		Console.WriteLine($"Up to first 10 digits of Sum         : {fullSum.UpToFirstNDigits(10)}");
		Console.WriteLine($"Total Digits in Sum                  : {fullSum.Length}");
		Console.WriteLine($"Up to first 10 Million Digits of Sum : {fullSum.UpToFirstNDigits(10000000)}");
	}

}
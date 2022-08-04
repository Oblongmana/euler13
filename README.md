# Euler 13 ++

Calculate the sum of extremely chonky numbers

- [Euler 13 ++](#euler-13-)
- [Help Contents](#help-contents)
- [Externally Verifying Sums, viewing inputs, etc](#externally-verifying-sums-viewing-inputs-etc)
- [Optimisation, testing etc scratchpad](#optimisation-testing-etc-scratchpad)
  - [Testing](#testing)
  - [Optimization notes on frequently used helper/readability functions](#optimization-notes-on-frequently-used-helperreadability-functions)
  - [Limit Pushing/Testing](#limit-pushingtesting)
    - [Testing the Sum limit?](#testing-the-sum-limit)
      - [Powers of 10? Approach 1](#powers-of-10-approach-1)
      - [Arbitrary length, small qty of digits](#arbitrary-length-small-qty-of-digits)
    - [Sniffing out any other string storage limits](#sniffing-out-any-other-string-storage-limits)

# Help Contents
```
Euler13 Help
    Calculate the sum of chonky numbers.

Limitations:
      Input numbers must contain fewer than ~2^30 characters (usually -33 or -56 depending on system), all positive natural numbers.
      The total sum also has this length limit.

Input Options
    dotnet run
        Calculate the sum of the Euler13 data set.
    dotnet run [FILE_PATH]
        Calculate the sum of the positive natural numbers in that file. Newline separated, no empty lines.
    dotnet run [NUMBER_OF_LINES] [OPTIONAL_WIDTH_OF_NUMBER]
        Sum [NUMBER_OF_LINES] random numbers. The number will have [OPTIONAL_WIDTH_OF_NUMBER] digits, or 50 if not supplied.
    dotnet run [NAME_OF_BUILT_IN_GENERATOR] [OPTIONAL_PARAMETER]
        Various poorly named other generators are available, such as Pow10Generator, or ArbitraryLength9sPlus1. Examine the code for these.
    dotnet run help
        Shows this help screen! Congrats, you did it!
```

# Externally Verifying Sums, viewing inputs, etc
You can't do this at the command line without modifying the code yet.

The good news is, line one of `Main` in `Euler13.cs` has a reporting mode
you can set, to `DUMP_VALUES`. You probably want to redirect this to a file instead of dumping to terminal, your terminal will probably
explode. e.g. `dotnet run 100000 100000 > dump.txt` will dump 100000 x 100000 character numbers and their summary info to a file

Note that for giant numbers (> 10 Million chars in the total sum), you'll need to uncomment the line in `ProgressReporter.cs` to also
output the full sum.

There's also a mode called `REPORT_PROGRESS` if you want to see a progress percentage and counter, but this SIGNIFICANTLY impacts performance.

# Optimisation, testing etc scratchpad

Far from exhaustive, just a few things noted while trying to cut time down and examine the limits of what this could do once things were a bit more stable/organised.

## Testing
General testing done against a 10 mil x 50 digit test, in NO_INTERMEDIATE_OUTPUT mode.
 https://dotnetfiddle.net/KW7EW4 can do convenient benching too

## Optimization notes on frequently used helper/readability functions
 - LeastSignificantDigit: use [len-1] instead of Linq's .Last(). Went from ~12s to ~8.7s in std benchmark!
 - DropLeastSignificantDigit: removed Length safety check. Went from ~9.5s to 8.75s in std bench
 - The char adding loop - Shortcut on differing sizes. Went from ~8.88s to ~8.37s in std bench. ~0.5s, and vaguely looks like the range of total times is a bit tighter now
   - shortcut column loop if the charIdx is greater than/eq to the length either of the two string/numbers we're adding, and the carry is zero.
     If the fullSum is the longer, nothing further to do. If the inputLine is longer, prepend the rest of it to the fullSum. In either case, break the loop.
     This saves a little computation any time the fullSum is longer than the inputLine (most of the time when adding same-length digits!), and saves a lot
     when adding mixed-length source digits

## Limit Pushing/Testing

### Testing the Sum limit?
As noted in Euler13.cs, our main theoretical limit is on storage for the Sum. A string can be at most 2gb, or 4gb with gcAllowVeryLargeObjects=true, which I probs won't do.

The practical limit therefore is a Sum string that is 2^30 digits long - produced by adding positive natural numbers.

#### Powers of 10? Approach 1
Riffing on the simplified approach to streaming input generation taken in the below section, we could generate a 2^30 long Sum by providing stepping 10^x, where x=0..2^30-1, i.e. 1,10,100,1000...

Add a "Pow10Generator(int raiseTenUpToMaxPowerOfTwo)" that produces strings from 10^0,10^1...10^(2^raiseTenUpToMaxPowerOfTwo{default=15}).

Testing the waters:
  - 10^0...10^(2^5 = 32) `dotnet run Pow10Generator 5 > dump.txt`. Looks good
    - ```
      Processed 32 lines in 00h:00m:00s.00ms
        Up to first 10 digits of Sum         : 1111111111
        Up to first 10 Million Digits of Sum : 11111111111111111111111111111111
      ```
  - 10^0...10^(2^10 = 1024) `dotnet run Pow10Generator 10 > dump.txt`. Looks good. Also added a dump of total digits in sum to make sure we're on track
    - ```
      Processed 1024 lines in 00h:00m:00s.06ms
        Up to first 10 digits of Sum         : 1111111111
        Total Digits in Sum                  : 1024
        Up to first 10 Million Digits of Sum : 111111111111111111111{SNIPPED}
      ```
  - 10^0...10^(2^15 = 32,768) `dotnet run Pow10Generator 15 > dump.txt`. Modified to simply add an extra "0" each iteration (prev regenerated ALL 0s each time). Time is getting chunky now, unsurprisingly! 14 took ~13s. This takes ~53s (corrected to ~54s, see below, after tweaking generation - see notes on 2^16) - 4x as much
    - ```
      Processed 32767 lines in 00h:00m:54s.15ms
        Up to first 10 digits of Sum         : 1111111111
        Total Digits in Sum                  : 32768
        Up to first 10 Million Digits of Sum : {snip}
      ```
  - 10^0...10^(2^16 = 65,536) `dotnet run Pow10Generator 16 > dump.txt`. Maybe 4x as much again? 4 * ~53s ~= 212s ~= 3.5 mins? Yeah! Looks like this formula is correct. Each power up takes 4x longer than last.
    - Note that this is now producing Sums with (10^(2^x)) + 1. Will correct for next run, renamed param to maxStrLenIs2ToPowerOf, num of iterations is -1, so generates number 1{((2^x)-1) zeroes},
        so len = 2^x. Might later need to add an offset param for boundary checking. I have a bad feeling about the time here though
    - ```
      Processed 65536 lines in 00h:03m:38s.81ms
        Up to first 10 digits of Sum         : 1111111111
        Total Digits in Sum                  : 65537
        Up to first 10 Million Digits of Sum : {SNIP}
      ```
  - 10^0...10^(2^20 = 1,048,576) `dotnet run Pow10Generator 20 > dump.txt`. So this would be adding a number with 1,048,576 digits to one with 1,048,575 digits... to one with 1 digit.
    - Predicting exec time of 4^(20-15) * 54s {i.e. the baseline time for 2^15} ~= 1024 * 54s {oh no} ~= 55,296s {aaaaaaaaaaaaaaaaaaaaaaaaaaaahhhhh} ~= 921 mins {aaaaaaaaaaaaaaaaaahhhhhh} ~= 15 hours {help}
  - 10^0...10^(2^17 = 1,048,576) might be more sane?
    - Predicting exec time of 4^(17-15) * 54s ~= 16 * 54s ~= 864s ~= 14 mins
    - That's more sane, let's run it and see how it goes.
    - Meanwhile, I'll calculate expected exec time for summing up to 2^30 length by pow 10 steps:
    - Result - prediction was bang on:
      - ```
        Processed 131071 lines in 00h:14m:24s.42ms
        Up to first 10 digits of Sum         : 1111111111
        Total Digits in Sum                  : 131072
        Up to first 10 Million Digits of Sum :
        ```
  - 10^0...10^(2^30 = 1,073,741,824)
    - Predicting exec time of 4^(30-15) * 54s ~= 1,073,741,824 {oh no} * 54s ~= 57,982,058,496s {well} ~= 966,367,641.6 mins {rip} ~= 16,106,127.36 hours {this technique is sunk} ~= 671,088.64 days {hah} ~= 1,838 years {woah} ~= 1.8 millenia {honestly a very cool length of time}. Fairly confident in this formula given above results so uhhhhhhhhh, parking this.

#### Arbitrary length, small qty of digits

Adding some new special generators:
  - ArbitraryLength8sPlus1
  - ArbitraryLength9sPlus1
  - ArbitraryLength9sTwice

`dotnet run ArbitraryLength9sPlus1 $((2 ** 5))`
  - ```
    Processed 2 lines in 00h:00m:00s.00ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 33
    Up to first 10 Million Digits of Sum : 100000000000000000000000000000000
    ```

`dotnet run ArbitraryLength8sPlus1 $((2 ** 30))` (ditto for 9s)
  - ```
    OOM
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 23)) > dump.txt`
  - ```
    Processed 2 lines in 00h:00m:01s.37ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 8388609
    Up to first 10 Million Digits of Sum : {snip}
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 24)) > dump.txt`
  - ```
    Processed 2 lines in 00h:00m:02s.71ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 16777217
    Up to first 10 Million Digits of Sum : {snip}
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 25)) > dump.txt`
  - ```
    Processed 2 lines in 00h:00m:05s.41ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 33554433
    Up to first 10 Million Digits of Sum : {snip}
    ```
  - Looks like each pow2 increase is doubling time taken? Nice

`dotnet run ArbitraryLength9sPlus1 $((2 ** 27)) > dump.txt`
  - So ~22s if pattern holds?
  - ```
    Processed 2 lines in 00h:00m:21s.71ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 134217729
    Up to first 10 Million Digits of Sum :
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 28)) > dump.txt`
  - So ~44s if pattern holds?
  - ```
    Processed 2 lines in 00h:00m:44s.14ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 268435457
    Up to first 10 Million Digits of Sum :
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 29)) > dump.txt`
  - So ~88s if pattern holds?
  - ```
    Processed 2 lines in 00h:01m:26s.67ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 536870913
    Up to first 10 Million Digits of Sum :
    ```

`dotnet run ArbitraryLength9sPlus1 $((2 ** 30)) > dump.txt`
  - `OOM`

`dotnet run ArbitraryLength9sPlus1 $(((2 ** 30)-1)) > dump.txt`
  - `OOM`

`dotnet run ArbitraryLength9sPlus1 $(((2 ** 30)-10)) > dump.txt`
  - `OOM`

`dotnet run ArbitraryLength9sPlus1 $(((2 ** 30)-100)) > dump.txt`
  - Not immediately OOM!
  - If pattern holds, exec time ~166s ~= 3m46s?
  - ```
    Processed 2 lines in 00h:02m:57s.55ms
    Up to first 10 digits of Sum         : 1000000000
    Total Digits in Sum                  : 1073741725
    Up to first 10 Million Digits of Sum :
    ```

Here we can basically conclude that the implementation is good, and is only limited by total string length/CLR limits.

Some further quick googling indicates slightly variable exact char limits depending on machine and runtime (e.g. 64 bit .Net CLR vs Mono)
 - https://stackoverflow.com/questions/6616739/why-is-the-max-size-of-byte-2-gb-57-b
 - https://stackoverflow.com/a/31803489

Common exact limits seem to be 1073741791 = 2^30 - 33, or 1,073,741,768 = 2^30 - 56.

Some quick testing (`dotnet run ArbitraryLength9sPlus1 $(((2 ** 30)-32)) > dump.txt` vs `dotnet run ArbitraryLength9sPlus1 $(((2 ** 33)-33)) > dump.txt`) shows my system's limit is 2^30 - 33. Mildly interesting giving there's maths in one of those posts showing (though I haven't read super close, that the 56 byte overhead is "correct")

So ~<= 2^30


### Sniffing out any other string storage limits
**NB: this was written before above notes, if some of this seems a little incongruous.**

Based on current benchmark (see ProgressReporter.cs notes): 1 million random lines of 50 digits in NO_INTERMEDIATE_OUTPUT takes ~00h:00m:08s.44ms.

The good news is that it scales linearly, so well done me, I didn't cause any exponential time problems.

To sniff out potential string storage limits where I've missed something, it could be worth summing 2^30 random numbers, 2^30 + 1, etc.

To test whether we can reach our boundary, that means we'd need (2^30 / 1mil) * 8.44s ~= 9062s ~= 151 mins ~= 2.5hrs.

Of course, we probably don't need 50-digit numbers to do that, just a bunch of 9s? Added a "IntLimit9sGenerator" option that generates 2^31 9s, and can take an optional +- modifier to increase/reduce that qty.

Using that to generate 1 million lines of 9s is SUPER fast - 00h:00m:00s.35ms. Extrapolating out to the 2^30 boundary (2^30 / 1mil) * .35s ~= 375s ~= 6 mins. Now that's what I call achievable

Just need to invoke with `dotnet run IntLimit9sGenerator -1073741824 > dump.txt` (i.e. it will generate (int.max = 2^31 -1) - 2^30 = 2^30-1 "9"s).
 - 2^30 - 1 "9"s: That worked fine: Processed 1073741823 lines in 00h:06m:13s.47ms
 - 2^30 + 1 "9"s: `dotnet run IntLimit9sGenerator -1073741822 > dump.txt`. Seems fine: Processed 1073741825 lines in 00h:06m:13s.11ms
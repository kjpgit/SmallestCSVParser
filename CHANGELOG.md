## 1.1

* Performance: Removed `yield return`, now just call `_sb.Append()` directly.  Speedup 6.5ms -> 5.0ms

* Performance/Readability: Instead of stripping quotes afterward in a 2nd pass based on
  `removeEnclosingQuotes`, instead don't write them in the first place.

* Performance: Added microbenchmark in ./SmallestCSVParserBenchmark/

## 1.0

Initial Release

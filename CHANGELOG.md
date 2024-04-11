## 1.1

* Performance: Removed `yield return`, now just call `_sb.Append()` directly.  Speedup 427 -> 284 us (33% speedup).

* Performance/Readability: Instead of stripping quotes afterward in a 2nd pass based on
  `removeEnclosingQuotes`, instead don't write them in the first place.

* Performance: Added microbenchmark in ./SmallestCSVParserBenchmark/

## 1.0

Initial Release

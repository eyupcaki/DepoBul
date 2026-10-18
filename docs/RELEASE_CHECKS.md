# Distribution checks — 2026-10-18

Base: user-supplied `DepoBul_V6_1_TLS_Duzeltmesi (1).zip`.

## Changes

- Added English/Turkish README, Git ignore/attribute files, data-source and rights notices, upstream package metadata, and a Windows launcher for existing C# checks.
- Confirmed that `App.config` is already listed in the project; the original project file and TLS settings were retained unchanged.
- All original `.cs` files, DLLs, NuGet package, pilot inputs and archived experiment files are byte-for-byte unchanged. No algorithm, seed list or scientific result was modified.
- No blanket open-source license was selected on behalf of the rights holders. Existing source/resource rights remain unchanged; OSM data rights and third-party conditions are documented separately.

## Checks completed here

- Parsed project XML; verified every explicit Compile/EmbeddedResource/None item and both GMap.NET HintPath targets exist.
- Independently recomputed **123 archived GA runs** against their saved directed distance matrices. Checked exact store coverage, one visit per store, depot departure/return, capacity <= 20, vehicle counts, distance and F = D + 100*(K-1).
- Selected depot: 30 runs; best distance 1855.3742000 km; mean distance 1936.578553333 km; sample standard deviation 44.430551610 km.
- Reviewed textual source/configuration for obvious embedded credentials and absolute personal-machine paths; none were identified. This is not a full security audit.
- Preserved the existing test sources and provided `Testler/RunChecks.ps1`.

## Checks not performed here

No C# compiler, .NET build tools or Windows GUI runtime are installed in this execution environment. **The Visual Studio build, Windows UI smoke test and C# synthetic test executables were not run for this distribution.** Run them on Windows; do not interpret this document as a successful Windows build report. Historical verification statements in `OKU.md` refer to the prior package, not a new execution here.

No new OSRM network search was performed. Recorded timing values are not substituted for timings from another computer.

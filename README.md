# DepoBul

Regional depot-location exploration and capacity-constrained vehicle routing using road-network distances.

[Turkce kullanim kilavuzu](README_TR.md) · [Detailed method notes (Turkish)](OKU.md) · [Data](DATA_SOURCES.md) · [Rights and dependencies](RIGHTS_AND_NOTICES.md)

## Open and run in Visual Studio

1. Extract **the complete ZIP** to a local folder. Keep `packages`, `Pilot_Veriler` and `Ornek_Otomatik_Deney` beside `DepoBul.sln`.
2. Open **DepoBul.sln** in Visual Studio on Windows.
3. The project targets **.NET Framework 4.5.2**, not modern .NET. The corresponding targeting/reference assemblies and the **.NET desktop development** tools are required. A newer installed runtime alone does not supply older targeting assemblies. Do not retarget if reproducing the archived experiment.
4. Set `DepoBul` as the startup project and press **F5**. The bundled GMap.NET 1.7.5 assemblies are referenced from `packages/`.
5. Select **Otomatik Depo Ara**. The input is `Pilot_Veriler/magazalar.csv`. Then select **Depo konumunu arastir** (Turkish UI spelling may include accents).
6. Once complete, select **Onerilen depoyu haritada goster**. Use **Karayolu Rotalari** on the main window to request road geometries. Fresh searches and road drawing require internet access.

The project retains a legacy interface and old algorithm code. **The research method described here is the automatic-depot-search workflow**, implemented by `AutoDepotSearch.cs` and `RoutingEngine.cs`; the old form's legacy algorithm is not the reported method.

## Inspect the archived result without a new search

In the automatic search window, select **Kayitli otomatik sonucu ac**, then open:

`Ornek_Otomatik_Deney/secilen_depo_kosumlari.csv`

This loads and validates the archived selected-depot solutions. It does not rerun the complete search. Requesting map tiles or road geometries can still require network access.

## Method and defaults

- Input: 28 OSM-derived store records, seven from each of four Turkish provinces. Demands are synthetic scenario values, not actual retailer orders.
- Demand: **91 pallets**; vehicle capacity: **20 pallets**; maximum vehicles: **28**.
- Objective: `F = D + 100 * (K - 1)`, where `D` is total directed road distance in km and `K` is the route/vehicle count.
- Outer search: demand-weighted latitude/longitude center; center + north/south/east/west at **40, 20, 10 km**, one center update per level. Candidate road snapping threshold: **2,000 m**; table endpoint threshold: **100 m**.
- Screening: population **100**, generations **100**, seeds **1,2,3**.
- Final evaluation: the three best screening locations; population **300**, generations **200**, seeds **1001–1030** at each location.
- Inner GA: customer permutation, greedy capacity split, binary tournaments, prefix-preserving ordered crossover, per-child swap probability **0.20**, one copied elite. Depot coordinates are not chromosome genes.
- Every route starts and ends at its evaluated depot. Store-to-store directed distances remain fixed during a search.

## Archived pilot result

| Measure | Value |
| --- | ---: |
| Distinct valid locations | 11 |
| Selected depot | AUTO001 |
| Latitude, longitude | 37.467991, 37.930525 |
| Selected-depot mean distance (30 runs) | 1,936.578553 km |
| Best-run distance | 1,855.3742 km |
| Vehicles in all selected-depot runs | 5 |
| Capacity-aware nearest-neighbor distance | 2,275.5799 km |
| GA distance improvement, mean / best | 14.89736% / 18.46587% |

There are 15 search visits: two rejected points, two repeated center visits and 11 distinct valid locations. Screening has 33 runs, final evaluation has 90 runs: **123 archived GA runs** in total. The selected depot is the road-snapped initial center; the local search did not find a better location. This is not a global optimality or land-use suitability claim.

**Important CSV distinction:** `en_yakin_komsu.csv` stores `ga_mean_improvement_percent` calculated from **F**, not distance (about 12.67%). The 14.90% and 18.47% figures above use **distance**. Archived files have not been rewritten. The UI's nearest-neighbor **F** is 2,675.5799 = 2,275.5799 km + 400 penalty.

## Repository layout

- `DepoBul/`: C# Windows Forms project, calculation engine and OSRM integration.
- `Pilot_Veriler/`: pilot input, selection trace and source records.
- `Ornek_Otomatik_Deney/`: archived matrices, routes, seeds, convergence and raw OSRM requests/responses.
- `Testler/`: existing synthetic C# checks; `RunChecks.ps1` compiles and executes them on Windows.
- `packages/`: pinned legacy GMap.NET package and assemblies, retained for the existing project references.
- `docs/RELEASE_CHECKS.md`: checks performed for this distribution.

## Reproducibility and limits

Use the saved matrices to audit the reported costs. New OSRM requests can change results because the public server's road data may change. The archive's timing values describe that archive's execution environment and are not a timing guarantee for another machine. One seed starts a complete GA run; a repeated identical run is not an additional independent experiment.

The public OSRM server is an external service with no availability guarantee. Avoid repeated bulk experiments against it; for larger use configure your own appropriate service. Road distances use the `driving` profile; no heavy-truck routing, delivery time windows, service durations or maximum working-day constraints are modeled. Road snapping does not verify parcel ownership, zoning or building suitability. Store activity was not field-verified. Google map/imagery providers are accessed by the existing GMap.NET interface; map imagery is not a bundled research dataset.

## Publication and reuse

Prepared as research software for INFTEC 2026. No final publication DOI or repository URL has been assigned in this distribution. Do not invent either in citations. After publication, cite the final paper and the specific repository tag/commit used.

Original-code reuse terms have **not** been changed by this packaging step. This is a source-available research release, not an assertion that all contents share an open-source license. See `RIGHTS_AND_NOTICES.md` for the separate status of original code, map-derived data and dependencies.

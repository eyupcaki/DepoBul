# Data provenance

## Pilot input

`Pilot_Veriler/magazalar.csv` contains 28 store records, seven from each of Gaziantep, Kahramanmaras, Adiyaman and Sanliurfa. The input/source records retain their original identifiers. Publication figures may show `M-` instead of `BIM-` with the same numeric suffix. This distribution does not rewrite identifiers or imply retailer endorsement.

Locations originate from OpenStreetMap records retrieved on 2026-10-15. Attribution: **© OpenStreetMap contributors**. The OSM-derived geographical database is made available under **ODbL 1.0**: https://opendatacommons.org/licenses/odbl/1-0/ . Attribution and license details: https://www.openstreetmap.org/copyright . Retain the source identifiers, links and attribution when redistributing the OSM-derived data. A code license does not replace the database license.

The source pool was filtered using geoBoundaries gbOpen Turkish first-level administrative boundaries. Boundary polygons are not bundled here. Reference: Runfola et al. (2020), geoBoundaries: A global database of political administrative boundaries. https://doi.org/10.1371/journal.pone.0231866 . `candidate_records.json` and `magaza_secimi.csv` preserve the available selection/source trace. No field survey confirming current store operation was conducted.

Selection used SHA-256 of UTF-8 `20261015|<osm_type>/<osm_id>`, sorting per province and selecting the first seven eligible records. Synthetic demand used `2 + (int(SHA256("demand|20261015|<osm_type>/<osm_id>"), 16) % 4)`. Demand is in [2,5] pallets and totals 91 pallets. Capacity 20 pallets and penalty 100 km-equivalent are research scenario assumptions, not actual retailer operational data.

## Road data

Archived OSRM requests/responses are in `Ornek_Otomatik_Deney/osrm_kayitlari/`; directional distance matrices are under `konum_degerlendirmeleri/`. The endpoint used the public `router.project-osrm.org` service with the `driving` profile. Individual request timestamps and query parameters remain in the archive. The OSRM software build and underlying OSM database epoch were not established; retrieval dates must not be substituted for those versions. OSRM-derived geographical data retain the underlying OSM attribution.

Software reference: Luxen and Vetter (2011), Real-time routing with OpenStreetMap data. https://doi.org/10.1145/2093973.2094062 . Project: https://project-osrm.org/ .

Preserve raw matrices for result verification. Map tiles or satellite imagery are not included. Saved CSVs retain original numerical precision and provenance, including failed/repeated search visits.

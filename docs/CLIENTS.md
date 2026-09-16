# Client Database Audit

Checked 2026-09-16. Stable desktop releases only; beta/nightly releases are not promoted. Historical profiles remain available under Legacy. These are tracker announce profiles, not bundled torrent clients.

| Client family | Release check | Database action |
| --- | --- | --- |
| qBittorrent | [5.2.3 stable](https://www.qbittorrent.org/download); 5.3.0beta1 is a prerelease | Retained 5.2.3 as the default. Corrected 5.2.1/5.2.3 keys to allow all eight hexadecimal digits, including leading zeroes. |
| uTorrent Classic | [Official Windows installer](https://www.utorrent.com/downloads/complete/track/stable/os/win/): 3.6.0.47254 | Added 47254; updated both build fields in User-Agent and the little-endian build bytes in the peer ID. Request layout inherited from the existing 3.6.0 profile; not packet-captured. |
| BitTorrent Classic | [Official Windows installer](https://www.bittorrent.com/downloads/complete/classic/): 7.11.0.47255 | Added 47255; updated User-Agent and peer-ID build bytes. Request layout inherited from the existing 7.11.0 profile; not packet-captured. |
| Deluge | [2.2.0 stable tag](https://github.com/deluge-torrent/deluge/tags); 2.2.1.dev0 is development | Retained 2.2.0. Fixed the peer suffix to use libtorrent's full URL-safe alphabet, including digits. The profile explicitly models libtorrent 2.0.11.0, not every possible Deluge package. |
| rTorrent | [v0.16.22 tag](https://github.com/rakshasa/rtorrent/releases/tag/v0.16.22) and configure.ac; the release title incorrectly says 0.16.21 | Added 0.16.22 with libtorrent's `-lt1016-` prefix, corrected query ordering and headers. Default `trackers.numwant=-1` means the parameter is omitted. |
| ruTorrent | [5.3.14](https://github.com/Novik/ruTorrent/releases/tag/v5.3.14) | Updated the paired label. ruTorrent is a Web UI; its version is not an announce identity. |
| Transmission | [4.1.3](https://github.com/transmission/transmission/releases/tag/4.1.3) | Added current query, User-Agent, `-TR4130-`, checksummed base36 suffix, eight-digit uppercase key and default numwant 80. |
| Halite | [0.4.0.4](https://github.com/Eoinocal/Halite/releases/tag/Release-0.4.0.4); release title omits the leading zero | Added the final published release with libtorrent 1.0.7 request layout, `Halite v 0.4.0.4`, `-HL0404-` and variable-width hexadecimal keys. |
| BitTyrant | [1.1.1](https://bittyrant.cs.washington.edu/), released 2007-09-07 | Added final published release. Official JAR retains `AZ2500BT` and `AzureusBitTyrant 2.5.0.0BitTyrant`; removed the old template's duplicate port parameter. |
| Vuze / Azureus | [Official archive](https://sourceforge.net/projects/azureus/files/vuze/) exposes 5.7.6.0; live site returned 403 | No newer release verified. Existing 5.7.7.0 profile retained, but its release/protocol provenance could not be confirmed. |
| BitComet | [2.21](https://www.bitcomet.com/en/downloads), released 2026-06-23; official ZIP inspected | New version confirmed, but complete current announce/peer/key format remains unverified. Existing 1.13/1.07 profiles remain Legacy. A local capture from 2.21 is needed before adding an accurate profile. |
| BitLord | [Official download page](https://www.bitlord.com/download) returned no usable release data; direct fetch failed DNS resolution | Current version and protocol unverified. Existing 1.1 profile remains Legacy; its BitComet-derived request must not be relabeled as a modern libtorrent client. |
| BitSpirit | [Official site](http://www.bitspirit.cc/en/) unavailable | Current version and protocol unverified from primary sources. Existing 3.5.0.275 profile remains Legacy. |
| burst! | [Official archive](https://sourceforge.net/projects/burst/files/) still lists 3.1.0b | No update found; retained. |

## Request Verification

- qBittorrent: [release-5.2.3 session settings](https://github.com/qbittorrent/qBittorrent/blob/release-5.2.3/src/base/bittorrent/sessionimpl.cpp), libtorrent [tracker query](https://github.com/arvidn/libtorrent/blob/v2.0.13/src/http_tracker_connection.cpp), [HTTP headers](https://github.com/arvidn/libtorrent/blob/v2.0.13/src/http_connection.cpp), [peer suffix alphabet](https://github.com/arvidn/libtorrent/blob/v2.0.13/src/string_util.cpp). Current 1.2 and 2.0 branches both use padded eight-digit keys.
- Deluge: [2.2.0 core](https://github.com/deluge-torrent/deluge/blob/deluge-2.2.0/deluge/core/core.py) generates `-DE220s-` and includes the linked libtorrent version in User-Agent.
- rTorrent: [libtorrent identity](https://github.com/rakshasa/libtorrent/blob/v0.16.22/configure.ac), [announce URL](https://github.com/rakshasa/libtorrent/blob/v0.16.22/src/tracker/tracker_http.cc), [binary peer suffix](https://github.com/rakshasa/libtorrent/blob/v0.16.22/src/torrent/torrent.cc), [default numwant](https://github.com/rakshasa/rtorrent/blob/v0.16.22/src/command_tracker.cc).
- Transmission: [version/prefix](https://github.com/transmission/transmission/blob/4.1.3/CMakeLists.txt), [peer checksum and User-Agent](https://github.com/transmission/transmission/blob/4.1.3/libtransmission/session.cc), [announce URL](https://github.com/transmission/transmission/blob/4.1.3/libtransmission/announcer-http.cc), [numwant and stop behavior](https://github.com/transmission/transmission/blob/4.1.3/libtransmission/announcer.cc).
- Halite: [version constants](https://github.com/Eoinocal/Halite/blob/Release-0.4.0.4/src/halTorrentDefines.hpp), [session settings](https://github.com/Eoinocal/Halite/blob/Release-0.4.0.4/src/halSession.cpp), [libtorrent 1.0.7 announce URL](https://github.com/arvidn/libtorrent/blob/libtorrent-1_0_7/src/http_tracker_connection.cpp).
- BitTyrant: [official changelog](https://bittyrant.cs.washington.edu/changelog.txt) and [official release archive](https://bittyrant.cs.washington.edu/dist_090607/BitTyrant-Linux32.tar.bz2), inspected with `javap` without launching the client.

Source inspection verifies the modeled defaults, not every possible installation. libcurl compression headers depend on build options; rTorrent and Transmission profiles advertise the gzip/deflate codecs RM supports. Optional externally discovered IPv4/IPv6 addresses and tracker-returned IDs are not modeled by these static templates. No claim of byte-for-byte equivalence to all upstream builds is made, and no live tracker traffic was used for verification.

## Configuration And Packaging

The `.client` files are copied automatically by `Source/RM.csproj` into build/publish output. The shipped default now explicitly selects qBittorrent 5.2.3. New global and per-torrent settings save a stable profile filename, so adding or reordering clients does not change the selection. Index-only settings migrate against the previous Build 74 list; missing profiles fall back to qBittorrent or the first available client. Older/custom index-only lists cannot be reconstructed reliably without their original profile directory.

Offline tests check the existing HTTP header normalization for every profile. Binary peer-ID generation now includes byte 255 and preserves binary bytes when uppercase percent encoding is requested.

Existing Build 72/73/74 release folders are not modified by this audit. A fresh build or publish is required to use the new database and generators together; copying new profiles into an old EXE is insufficient for Transmission and Halite.

Run the offline checks:

```powershell
dotnet run --project Tests/ClientProfiles/ClientProfiles.csproj -c Release
```

## Inspected Downloads

SHA-256 of the official packages inspected on the audit date:

```text
uTorrent 3.6.0.47254 installer
E2837E13D80E4602B0C2D711C6D6C17217F4A80C8FF0B33DD90DF219F025F3B7
BitTorrent 7.11.0.47255 installer
E26E4495D2B67B68B5E40FA923345D37E20F346B3DD19996AF2FA166F9883C5A
BitComet 2.21 ZIP
D305A824FEE8DB010BB9F4FDFB272DE830124889261C1044346C2FFF17F01090
```

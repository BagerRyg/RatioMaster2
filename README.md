<a id="readme-top"></a>

[![.NET 10][dotnet-shield]][dotnet-url]
[![Windows App][windows-shield]][windows-url]
[![Issues][issues-shield]][issues-url]
[![License: GPL 3.0][license-shield]][license-url]

<br />
<div align="center">
  <img src="RM.ico" alt="RatioMaster 2.0 icon" width="80" height="80">

  <h1 align="center">RatioMaster 2.0</h1>

  <p align="center">
    A modernized RatioMaster release for spoofing torrent tracker upload/download announce data.
    <br />
    <a href="https://github.com/BagerRyg/RatioMaster2/issues">Report Bug</a>
    &middot;
    <a href="https://github.com/BagerRyg/RatioMaster2/issues">Request Feature</a>
  </p>
</div>

## Introduction

RatioMaster 2.0 is an improved and modified version of RatioMaster v1.9.1. The application was modernized by decompiling the original RatioMaster v1.9.1 source by Ratiomaster_06/Moofdev, then updating and extending it.

All original credits go to Ratiomaster_06/Moofdev.

## About The Project

RatioMaster 2.0 is a Windows application used to spoof upload and download statistics to a torrent tracker. It does not upload real torrent data to peers. The only data being sent is tracker announce data, such as reported uploaded/downloaded amounts, selected client identity, peer ID, key, port, and related tracker parameters.

Use it carefully. Tracker behavior and rules vary.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Screenshots

![RatioMaster 2.0 app preview](App_preview.png)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Features

RatioMaster 2.0 introduces a new set of features, improvements, and changes:

* Improved UI, with the addition of Dark Mode.
* Updated framework from old .NET 8 -> .NET 10
* Improved stability and security.
* Added new, up-to-date torrent clients, such as qBittorrent 5.2.1, uTorrent 3.6.0, BitTorrent 7.11.0, Vuze 5.7.7.0, Deluge 2.2.0, and rTorrent 0.16.12 / ruTorrent 5.3.1.
* Improved tracker/announce response.
* Torrent files containing multiple announce URLs are now supported correctly and working.
* Fixed an issue where the client could stall for up to 15 minutes before updating the tracker.
* Improved logging tab.
* Removed the auto-update function, as it was hitting a dead endpoint anyway.
* Many other minor tweaks and improvements.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Built With

* C#
* Windows Forms
* .NET 10

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

Download the latest build from the repository releases or build it from source with the .NET SDK.

### Prerequisites

* Windows
* .NET 10 Runtime for published framework-dependent builds
* .NET 10 SDK if building from source

### Build From Source

```powershell
dotnet publish ".\Source\RM.csproj" -c Release -r win-x64 --self-contained false -o ".\Build X"
```

Replace `Build X` with the next numbered build folder.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

1. Open RatioMaster 2.0.
2. Load a `.torrent` file.
3. Select the torrent client profile you want to emulate.
4. Configure upload/download values and tracker settings.
5. Start the announce session.

The app reports spoofed announce data to the tracker. It does not transfer actual torrent payload data to peers.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Bugs And Feedback

Please report bugs, feedback, or feature requests via GitHub issues:

[https://github.com/BagerRyg/RatioMaster2/issues](https://github.com/BagerRyg/RatioMaster2/issues)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## License

Distributed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Disclaimer

I take absolutely no responsibility. Use the program at your own risk.

I have run and tested the program against different trackers for an extended period without any warnings or detections, but use it conservatively and with reasonable use.

The program comes with ZERO warranty.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Acknowledgments

* Original RatioMaster v1.9.1 by Ratiomaster_06/Moofdev.
* Original credits belong to the original authors.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

[dotnet-shield]: https://img.shields.io/badge/.NET-10-blue?style=for-the-badge
[dotnet-url]: https://dotnet.microsoft.com/
[windows-shield]: https://img.shields.io/badge/Windows-app-lightblue?style=for-the-badge
[windows-url]: https://www.microsoft.com/windows
[issues-shield]: https://img.shields.io/github/issues/BagerRyg/RatioMaster2.svg?style=for-the-badge&color=red
[issues-url]: https://github.com/BagerRyg/RatioMaster2/issues
[license-shield]: https://img.shields.io/badge/License-GPL%203.0-green?style=for-the-badge
[license-url]: LICENSE

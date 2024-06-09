# TvTv2XmlTv Converter

This project is a C# console application that extracts TV guide data from a public api and produces an "XmlTV" data file. The original PHP script was created by Jaime Idolpx (<jaime@idolpx.com>) and can be found [here](https://gist.github.com/idolpx/c82747bb740c303f56ad8a1e8f17d575).

## Table of Contents

- [TvTv2XmlTv Converter](#tvtv2xmltv-converter)
  - [Table of Contents](#table-of-contents)
  - [Background](#background)
  - [Features](#features)
  - [Requirements](#requirements)
  - [Usage](#usage)
  - [Command Line Arguments](#command-line-arguments)
    - [Examples](#examples)
  - [Building the Project](#building-the-project)
  - [Acknowledgements](#acknowledgements)
  - [License](#license)

## Background

This project is a C# implementation based on the original PHP script by Jaime Idolpx. It extracts TV guide data from a public api and outputs it in the XMLTV format, which is widely used by various TV guide software.

## Features

- Extracts TV guide data from a public api.
- Supports up to 8 days of guide data.
- Outputs data in XMLTV format.
- Configurable via command-line arguments.

## Requirements

- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Internet connection to access a public api API.

## Usage

1. Clone the repository:

   ```sh
   git clone https://github.com/yourusername/tvtv2xmltv.git
   cd tvtv2xmltv
   ```

2. Build the project:

   ```sh
   dotnet build
   ```

3. Run the application:

   ```sh
   dotnet run -- [options]
   ```

## Command Line Arguments

The application supports the following command-line arguments:

- `--timezone` (default: "America/Chicago"): Specify the timezone.
- `--lineUpID` (default: "USA-OTA60611"): Specify the line up ID.
- `--days` (default: 8): Specify the number of days (up to 8).
- `--fileName` (default: "xmltv.xml"): Specify the output file name.

### Examples

Run with default values:

```sh
dotnet run
```

Run with custom values:

```sh
dotnet run -- --timezone="Europe/London" --lineUpID="UK-OTA12345" --days=5 --fileName="custom_output.xml"
```

## Building the Project

To build the project, follow these steps:

1. Ensure you have .NET 8.0 SDK installed.
2. Navigate to the project directory.
3. Run the build command:

   ```sh
   dotnet build
   ```

## Acknowledgements

- **Jaime Idolpx** for the original PHP script. The original script can be found [here](https://gist.github.com/idolpx/c82747bb740c303f56ad8a1e8f17d575).
- **.NET Core Team** for providing a powerful framework for building cross-platform applications.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

# Windows Data Recovery Tool

A comprehensive desktop application for recovering deleted files from Windows-formatted disks. This tool is designed to help recover data from drives affected by Windows updates, accidental deletion, or disk formatting.

## Features

- **Disk Scanning**: Deep scan for deleted files on NTFS and FAT32 file systems
- **File Preview**: Preview recovered files before recovery
- **Selective Recovery**: Choose specific files or folders to recover
- **Multiple Format Support**: NTFS, FAT32, exFAT
- **Progress Tracking**: Real-time scanning and recovery progress
- **Filter Options**: Search by file type, date, size
- **Recovery to External Drive**: Safe recovery to external storage
- **Detailed Logging**: Complete operation logs for troubleshooting

## System Requirements

- Windows 7 or later (x64)
- .NET 6.0 or higher
- Administrator privileges
- Minimum 4GB RAM
- Minimum 500MB free disk space

## Installation

```bash
# Clone the repository
git clone https://github.com/ndajakson-max/windows-data-recovery.git

# Navigate to the project
cd windows-data-recovery

# Build the project
dotnet build

# Run the application
dotnet run
```

## Usage

1. Launch the application with administrator privileges
2. Select the drive to scan
3. Choose scan depth (Quick or Deep)
4. Review found files and select ones to recover
5. Specify recovery destination
6. Start the recovery process

## Architecture

- **Core**: Data recovery engine and file system scanning
- **UI**: WPF-based Windows desktop application
- **Utilities**: File system interaction and disk operations
- **Logger**: Application logging and error tracking

## License

MIT License - See LICENSE file for details

## Disclaimer

This tool is provided for legitimate data recovery purposes only. Users are responsible for complying with applicable laws and regulations. Always ensure you have proper authorization before attempting data recovery operations.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bug reports and feature requests.

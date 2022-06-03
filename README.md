# Steam Game Verifier
Small application to verify steam files and list the invalid files based on the output manifest of 
[DepotDownloader](https://github.com/SteamRE/DepotDownloader).

## Usage

Run the program with the first argument as the output manifest of DepotDownloader and second argument as
the root of the game you want to check. For example:

```bash
SteamGameVerifier.exe manifest.txt 'D:\Program Files (x86)\My Game'
```

The program will report invalid files and create a file ending in `-bad.txt` with a list of all bad
files. You can later feed this file to DepotDownloader in order to download these files only.
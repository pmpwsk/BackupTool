# backto
Cross-platform program written in C# that creates and updates backups.

Website: https://uwap.org/projects/backto

## Main features
- Creating backups
- Updating existing backups

## Installation
This program is portable, simply download a binary for your system from the releases on GitHub (SC means self-contained, FD means framework-dependent so it needs dotnet installed) or build the code yourself using the dotnet command or an IDE of your choosing, then execute the file <code>backto</code> on Linux and macOS or <code>backto.exe</code> on Windows.

## Usage
Run <code>backto [source] [target]</code> to back up the source folder to the target folder.

The program shows some stats about how many files were checked, created, deleted or changed. Additionally, it counts failures and outputs the list of failed paths once the backup has completed.

Here are some things that are neat to know:
- The program doesn't check for changes in the target folder, so don't modify files there!
- Backing up for the first time takes quite long since all of the files need to be copied. Backups after that are quicker because the program only copies files that were changed since the last backup.
- The program creates a file called <code>BackupState.bin</code> in the target directory to keep track of backed up files. Since it's in the target folder, you can have multiple targets for a given source folder.
- The target folder looks exactly like the source folder, so you can simply move it over to restore from a backup (delete <code>BackupState.bin</code>, though).
- Symbolic links are ignored entirely.
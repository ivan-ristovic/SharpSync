# SharpSync
Simple cross-platform directory synchronization CLI tool written in C#.

SharpSync was created because I needed something to quickly backup a list of directories to an external drive. 
Since I could not find any programs that fit my requirements, I decided to create my own (and frankly it took me less
time to code it than to search and try out all those other sync programs).

SharpSync can perform file/directory filtering as well as strict backup by deleting extra files/directories from destination folder. This behavior can also be filtered to exclude certain files/directories.

## How to use

SharpSync is a CLI tool. Backups are formulated through a series of **sync rules**. SharpSync uses `.json` files to store the sync rules.
This means that you can keep multiple sync rule files or transfer them between machines.

Current program options are:
- `add`: Adds a new sync rule to the database with a variety of options:
	- `zip`: Compress the directory
	- `top`: Top directory only, no recursive
	- `include-hidden`: Include hidden files
	- `delete-extra-files`: Strict backup, delete extra files from destination
	- `delete-extra-dirs`: Strict backup, delete extra directories from destination
	- `exclude-dirs`: Directories to exclude (regex list)
	- `exclude-files`: Files to exclude (regex list)
	- `include-dirs`: Directories to include (regex list)
	- `include-files`: Files to include (regex list)
	- `del-exclude-dirs`: Directories to exclude from strict backup (regex list)
	- `del-exclude-files`: Files to exclude from strict backup (regex list)
- `remove`: Removes a sync rule with options:
	- `all`: Removes all sync rules
- `list`: Lists all sync rules.
- `sync`: Initiates folder sync.
- `help`: Prints command manual.

Starting the program without arguments prints out a help screen with all possible commands. You can also use `help`
command on other commands, for example `help sync`.

## Next steps?
- [ ] More options: include/exclude lists (via regexes), rule search, rule activation/deactivation
- [ ] Export/Import sync rules
- [ ] Multiple sync destinations

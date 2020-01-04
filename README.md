# SharpSync
Simple directory synchronization CLI tool written in C# using [BlinkSyncLib](https://github.com/dvoituron/BlinkSyncLib).

SharpSync was created because I needed something to quickly backup a list of directories to an external drive. 
Since I could not find any programs that fit my requirements, I decided to create my own (and frankly it took me less
time to code it than to search and try out all those other sync programs).

This project in its current state can perform basic syncronization. I will add more features as time goes on (this 
means: when I feel the need for them). I have made the repository public in case anyone wants to contribute either by
implementing something or sharing ideas/feedback.

The release will be added soon to the releases page. I plan to release the binary packed with .NET Core 3.1 runtime. 
There will also be a standard packed executable in case anyone prefers that.

## How to use

SharpSync is completely CLI tool. It requires no external dependencies. Backups are formulated through series of **sync
rules**. SharpSync uses SQLite to save the sync rules into a local database file - this is not encrypted (I might add 
an option to encrypt the database later on). This means that you can save your `.db` files to keep current rule set.

Current program options are:
- `add`: Adds a new sync rule to the database.
- `remove`: Removes a sync rule from the database.
- `list`: Lists all sync rules.
- `sync`: Initiate sync.
- `help`: Prints manual.

Starting the program without arguments prints out a help screen with all possible commands. You can also use `help`
command on other commands, for example `help sync`.

Examples:
- args: `list`
```
[00:44:23 INF] Registered sync rules:
+----------------------+
|   ID | Source        |
| Zip? | Destination   |
+----------------------+
+----------------------+
|    1 | ...\test\src  |
|   No | ...\test\dst  |
+----------------------+
+----------------------+
|    2 | ...\test1\src |
|  Yes | ...\test1\dst |
+----------------------+
```
- args: `add test2/src test2/dst` 
```
[00:47:32 INF] Adding sync rule test2/src -> test2/dst
[00:47:33 INF] Rule successfully added
```
```
[00:47:40 INF] Registered sync rules:
+----------------------+
|   ID | Source        |
| Zip? | Destination   |
+----------------------+
+----------------------+
|    1 | ...\test\src  |
|   No | ...\test\dst  |
+----------------------+
+----------------------+
|    2 | ...\test1\src |
|  Yes | ...\test1\dst |
+----------------------+
+----------------------+
|    3 | ...\test2\src |
|  Yes | ...\test2\dst |
+----------------------+
```
- args: `remove 2 3` 
```
[00:49:25 INF] Removing sync rule(s): [2, 3]
[00:49:25 INF] Rule 2 successfully removed
[00:49:25 INF] Rule 3 successfully removed
```
```
[00:49:30 INF] Registered sync rules:
+----------------------+
|   ID | Source        |
| Zip? | Destination   |
+----------------------+
+----------------------+
|    1 | ...\test\src  |
|   No | ...\test\dst  |
+----------------------+
```
- args: `sync`


## Next steps?
- More options: include/exclude lists (via regexes), rule search, rule activation/deactivation
- Export/Import rules
- Database configuration, other DB providers (SQL Server, PostgreSQL)
- Multiple sync destinations
- and more, these are only ones I remember at this time...
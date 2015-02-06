## Project Description
Yet another migrations platform right? This is purely sql based (tsql as it is only for Sql Server at this time). This tool is meant to help manage versioning a database schema including scripting out objects/test data, managing change scripts, and backups/restores etc.

## Feature List

* Script out individual objects into their own folders (views/procedures/tables etc)
	* Baseline an existing database that is not under version control
* Create update scripts in an Updates folder
* Create a brand new instance of a scripted database by simply providing a new database name!
	* The script out creates an entire schema in order of dependencies to create new instances on the fly.
* Script out test data sets by passing a list of table names, the dependencies will be walked between tables and drop statements will be created along with the inserts.
	* This is great to setup integration test environments
* Apply update scripts automatically
	* Command line tool applies update scripts by checking the databases current version.
	* These are applied with transactions so if a failure occurs it will rollback the update.
	* Scripts are numbered Major.Minor.Point.Whatever.sql and allow for text after the point number
	* Updates are timestamped when applied and kept in a table called SchemaChanges (this will be configurable in the future)
* Backup / restore local databases to disk
	* This is great for quickly dumping your instance to disk and giving it to another developer to use (for testing/debugging or just to get them started)
* Run arbitrary scripts on a database (lookup or other test data sets you may have)
* Fully automated via CLI
	* Great way to use the tool is to create a set of batch files for typical scenarios (like the ones listed above), and version them in your repository so other users can quickly run commands like"
	* Update Northwind
	* Create New NorthwindIntegartionTests
	* Backup Northwind
	* Backup then Update Northwind
	* Restore Northwind
	* Script out Northwind lookup data
	* Script out Northwind

More features to come in the future, including possible integration with [Open DBDiff](https://opendbiff.codeplex.com/) or the spin off [Sql-DBDiff](https://code.google.com/p/sql-dbdiff/).

## History

This was migrated from http://tsqlmigrations.codeplex.com/

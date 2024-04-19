# Microsoft SQL Server Tool Box Utility

This utility tool provides a set of functionalities to manage and perform various tasks on Microsoft SQL Server databases.

## Features

- **List Online Databases**: Display the list of online databases on the specified SQL Server.
- **List Offline Databases**: Display the list of offline databases on the specified SQL Server.
- **List Recovery Models**: Retrieve and display the recovery models of databases.
- **Change Recovery Model**: Allow changing the recovery model of a database.
- **List Index Fragmentations**: List index fragmentations for databases, specific tables, or all tables within a database, allowing you to identify areas for optimization.
- **Rebuild Indexes**: Rebuilds indexes for all tables in all online databases, a specific database, or a specific table, with the option to set a fragmentation limit.
- **Reorganize Indexes**: Reorganize indexes for all tables in all online databases, a specific database, or a specific table, with the option to set a fragmentation limit.
- **Update Index Statistics**: Updates index statistics for all tables in all online databases, a specific database, or a specific table.
- **Index Optimization**: Optimizes indexes for all tables in all online databases, a specific database, or a specific table, with the option to set a fragmentation limit.
- **Top 10 Queries by Avg. CPU Time**: Display the top 10 queries based on average CPU time.
- **Top 10 Queries by Avg. Elapsed Time**: Display the top 10 queries based on average elapsed time.
- **Top 10 Active Queries by CPU Time**: Display the top 10 active queries based on CPU time.
- **Exit**: Exit the application.

## Getting Started

To use this tool, you need to provide the SQL Server credentials including the server name, username, and password. The tool connects to the SQL Server using the provided credentials and allows you to perform various database operations.

### Prerequisites

- .NET SDK
- Microsoft.Data.SqlClient package (version 5.2.0 or higher)

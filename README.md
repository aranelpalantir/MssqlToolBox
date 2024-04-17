# Microsoft SQL Server Tool Box Utility

This utility tool provides a set of functionalities to manage and perform various tasks on Microsoft SQL Server databases.

## Features

- **List Online Databases**: Displays the list of online databases on the specified SQL Server.
- **List Offline Databases**: Displays the list of offline databases on the specified SQL Server.
- **List Recovery Models**: Retrieves and displays the recovery models of databases.
- **Change Recovery Model**: Allows changing the recovery model of a database.
- **List Index Fragmentations**: Lists index fragmentations for databases, allowing you to identify areas for optimization.
- **Rebuild Indexes**: Rebuilds indexes of all online databases, with an option to specify a fragmentation limit.

## Getting Started

To use this tool, you need to provide the SQL Server credentials including the server name, username, and password. The tool connects to the SQL Server using the provided credentials and allows you to perform various database operations.

### Prerequisites

- .NET SDK
- Microsoft.Data.SqlClient package (version 5.2.0 or higher)

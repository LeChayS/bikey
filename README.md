
# BIKEY - bike for rent

A project about Bike rent by [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet)

**DRIFT** is a web-based vehicle rental management system that allows users to browse available vehicles, make rental bookings, and manage rental information online.

The system supports role-based access control with three main roles: customers, staff, and administrators, each having a dedicated interface and set of permissions.

Administrators can manage vehicles, vehicle categories, rental orders, and user accounts. Staff members can assist in managing rental operations and order statuses, while customers can view available vehicles and place rental requests.

The application is built using ASP.NET Core and SQL Server, focusing on clear architecture, practical business logic, and suitability for academic purposes in a software engineering course.

## Table of Content

- [Feature](https://github.com/LeChayS/bikey/tree/master?tab=readme-ov-file#features)
- [Getting Started](https://github.com/LeChayS/bikey/tree/master?tab=readme-ov-file#getting-started)
- [Environment Variables](https://github.com/LeChayS/bikey/tree/master?tab=readme-ov-file#environment-variables)
- [Deployment](https://github.com/LeChayS/bikey/tree/master?tab=readme-ov-file#deployment)
- [Merge, Branch and Commit Rules](https://github.com/LeChayS/bikey/tree/master?tab=readme-ov-file#merge-branch-and-commit-rules)

## Features

- User authentication with role-based access control
- Item catalog management with categories and availability tracking
- Online rental booking and order management
- Admin dashboard for managing users, items, and rental records
- Search and basic filtering for available items

## Getting Started

Before installing my project, you need to install these following things

1. Checking your .NET version by use the following commands:

```sh
dotnet --version
```

If it is not **10.0.102** then click [HERE](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) to go to the download place. Make sure to find the right version

![dotnetv10](dotnetv10.png)

1. Install [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/ssms/install/install) and [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

Be sure to choose the *SQL Server 2025 Express* when download the SQL server.

![sqlserver](sqlserver.png)

You will have to install the SQL Server and setup it first, then you can install the SSMS

![sseissms](sseissms.png)

1. Install [Visual Studio](https://visualstudio.microsoft.com/)
Make sure when installing you will check **ASP.NET and web development**

![checkbox](vscheckbox.png)

## Environment Variables

To run this project, you will need to do something first

1. Clone the project

```bash
  git clone https://github.com/LeChayS/bikey.git
```

1. Open SSMS and copy the **Server name**

![servername](servername.png)

2. Open the project using VS

3. Find the file name **appsettings.json**

4. Replace the **Server** in the line

```bash
"BikeConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BikeyDB;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=120;Max Pool Size=200;Min Pool Size=5;"
```

by **your server name**

1. On the taskboard choose **Tools > NuGet Package Manager > Package Manager Console**

![nuget](nuget.png)

2. Install NuGet Package

```bash
Add-Migration InitialCreate
```

1. Create the database

```bash
update-database
```

## Deployment

Run by press **F5** on keyboard or **Run button** on taskboard

## Merge, Branch and Commit Rules

### 1. Create branch

New branch always created from ***main*** (or ***develop***)

1. Use `git checkout master` to checkout *main*

2. To update the newest things of *main* to local, use `git pull`

3. `git checkout -b <branch-name>` to create a new branch from *main*

\**Note*: before push the new branch up to repo, **ALWAYS** checkout ***main*** version on repo and local is the same (or **up-to-date**)

- `git pull origin master` to update all the newest things on *main* to local

- up the branch to repo by `git push`

### 2. Naming branch

Following this rule

```bash
<type>/#<issue_number>-<issue_title_in_english>
```

In which

*<type>* are:
- feature: new feature
- bugfix: bug fixing when found out a bug
- hotfix: serious bug that need to be fix emidialy on *main*
- revert: revert back to the previous commit
- update: change feature or logic available

*<issue_number>* is the number of the issue on Github Issue

*<issue_title_in_english>* is the name of the issue but in english

### 3. Commit name

```bash
<type>/<change content>
```

In which *<type>* are:

- feat: A new feature
- fix: A bug fix
- docs: documentation only changes
- style: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- refactor: A code change that neither fixes a bug nor adds a feature
- perf: A code change that improves performance
- test: Adding missing tests or correcting existing tests
- build: Changes that affect the build system or external dependencies (example scopes: gulp, broccoli, npm)
- ci: Changes to our CI configuration files and scripts
- chore: Other changes that don't modify src or test files
- revert: Reverts a previous comit
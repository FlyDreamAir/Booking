# FlyDreamAir

This repository contains the source code for the new booking system for FlyDreamAir.

## Building

You will need the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to build
the code.

To build the app:

```sh
dotnet build FlyDreamAir/FlyDreamAir.csproj
```

## Running

Before your first run, you will need to prepare the application's runtime environment.

### PostgreSQL Database

This application uses the PostgreSQL 16 DBMS.

Running this app requires a dedicated user and database, which may be set up by running this in
a query window:

```sql
CREATE USER <UserName> WITH PASSWORD '<UserPassword>';
CREATE DATABASE <DatabaseName>;
GRANT ALL ON DATABASE <DatabaseName> TO <UserName>;
ALTER DATABASE <DatabaseName> OWNER TO <UserName>;
```

Replace `<UserName>`, `<UserPassword>`, and `<DatabaseName>` with your chosen values.

Then, we should register the connection string as a .NET
[app secret](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```sh
dotnet user-secrets set ConnectionStrings:DefaultConnection \
    "Host=<HostName>; Database=<DatabaseName>; Username=<UserName>; Password=<UserPassword>" \
    --project FlyDreamAir/FlyDreamAir.csproj
```

### API Keys

#### Postmark

We use [Postmark](https://postmarkapp.com/) for sending emails.
For this to work, we need a
[Server Token](https://postmarkapp.com/developer/api/overview#authentication) from Postmark.

After getting one, we should register this as a .NET app secret:

```sh
dotnet user-secrets set ApiKeys:Postmark "<Postmark Server Token>" \
    --project FlyDreamAir/FlyDreamAir.csproj
```

#### Google/Microsoft OAuth

For external authentication providers, we need to acquire client IDs and client secrets from the
corresponding portal.

You only need to get the client ID and secret then store it using `dotnet user-secrets set`. The
other steps in the tutorials below are for app developers and are not necessary for configuration.

For Google, please follow [these instructions](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-8.0#create-the-google-oauth-20-client-id-and-secret).

For Microsoft, please follow [these instructions](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-8.0#create-the-app-in-microsoft-developer-portal).

### Executing

After the database and API keys are ready, you can run the app by using:

```sh
dotnet run --project FlyDreamAir/FlyDreamAir.csproj
```

## Legal

The source code of the FlyDreamAir booking system is released under the MIT License.
See [LICENSE.md](LICENSE.md) file more details.

### UOW Students

Please note that this repository contains assessed work for CSIT214 which has been made public for
the purpose of evaluation by academic staff.

Be sure to consider the
[Academic Integrity Policy](https://www.uow.edu.au/about/governance/academic-integrity/) before
accessing and using this repository.

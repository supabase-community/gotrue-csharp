# Getting Started

The Supabase C# library is a wrapper around the various REST APIs provided by Supabase and
the various server components (e.g. GoTrue, Realtime, etc.).

## Getting Oriented

At the most basic level, Supabase provides services based on the Postgres database and
the supporting ecosystem. You can use the online, hosted version of Supabase, run it locally
via CLI and Docker images, or some other combination (e.g. hosted yourself on another cloud service).

One option for .NET developers, of course, is to just treat it as any other RDBMS package. You can
create a project, grab the .NET connection string, and use it as you would any other database.

However, Supabase also provides a number of other services that are useful for building applications
and services. These include:

- Authentication (GoTrue)
- PostgREST (REST API)
- Realtime
- Storage
- Edge Functions

Authentication is provided by GoTrue, which is a JWT-based authentication service. It provides
a number of features, including email/password authentication, OAuth, password resets, email 
verification, and more. In addition, you can use it to handle the native Sign in With Apple and 
Sign in With Google authentication systems.

PostgREST is a REST API that is automatically generated from your database schema. It provides
a number of features, including filtering, sorting, and more.

Realtime is a service that provides realtime updates to your database. It is based on Postgres
LISTEN/NOTIFY and WebSockets.

Storage is a service that provides a simple interface for storing files. It is based on Postgres
and provides a number of features, including file versioning, metadata, and more.

Edge Functions is a service that provides a way to run serverless functions on the edge. It is
based on Cloudflare Workers and provides a number of features, including authentication, caching,
and more.

The Supabase C# library provides a wrapper around the various REST APIs provided by Supabase and
the various server components (e.g. GoTrue, Realtime, etc.). It also provides a number of
convenience methods for working with the various services.

## Basic Concepts

There are two main ways to access your Supabase instance - either via an "untrusted" client 
(e.g. Unity or some other desktop client) or a "trusted" client (e.g. a server-side application).

The untrusted clients have two main aspects - first, you'll likely want to manage the user
state (e.g. login, logout, etc.) and second, you'll be using the anonomous/public API key to 
access those services.

Trusted, server-side code is usually best managed as a stateless system, where each request
is managed independently. In this scenario, you will often want to use the library in conjunction
with the private API key.

**Remember - the public key is designed to be used in untrusted clients, while the private key
is designed to be used in trusted clients ONLY.**

## Next Steps

Given that the configuration is pretty different depending on the scenario, we'll cover each
of the scenarios separately.

- [Unity](Unity.md)
- [Desktop Clients](DesktopClients.md)
- [Server-Side Applications](ServerSideApplications.md)

To use this library on the Supabase Hosted service but separately from the `supabase-csharp`, you'll need to specify
your url and public key like so:

```c#
var auth = new Supabase.Gotrue.Client(new ClientOptions<Session>
{
    Url = "https://PROJECT_ID.supabase.co/auth/v1",
    Headers = new Dictionary<string, string>
    {
        { "apikey", SUPABASE_PUBLIC_KEY }
    }
})
```

Otherwise, using it this library with a local instance:

```c#
var options = new ClientOptions { Url = "https://example.com/api" };
var client = new Client(options);
var user = await client.SignUp("new-user@example.com");

// Alternatively, you can use a StatelessClient and do API interactions that way
var options = new StatelessClientOptions { Url = "https://example.com/api" }
await new StatelessClient().SignUp("new-user@example.com", options);
```


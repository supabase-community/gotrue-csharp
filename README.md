<p align="center">
<img width="300" src=".github/supabase-gotrue.png"/>
</p>

<p align="center">
  <img src="https://github.com/supabase/gotrue-csharp/workflows/Build%20And%20Test/badge.svg"/>
  <a href="https://www.nuget.org/packages/gotrue-csharp/">
    <img src="https://img.shields.io/badge/dynamic/json?color=green&label=Nuget%20Release&query=data[0].version&url=https%3A%2F%2Fazuresearch-usnc.nuget.org%2Fquery%3Fq%3Dpackageid%3Agotrue-csharp"/>
  </a>
</p>

---

## BREAKING CHANGES v3.1 → v3.x

- Exceptions have been simplified to a single GotrueException. A Reason field has been added
to GotrueException to help sort out what happened. This should also be easier to manage as the Gotrue
server API & messages evolve.
- The delegates for save/load/destroy persistence have been simplified to no longer require async.
- Console logging in a few places (most notable the background refresh thread) has been removed 
in favor of a notification method. See Client.AddDebugListener() and the test cases for examples.
This will allow you to implement your own logging strategy (write to temp file, console, user visible 
err console, etc).
- The client now more reliably emits AuthState changes.
- There is now a single source of truth for headers in the stateful Client - the Options headers.

Implementation notes:

- Test cases have been added to help ensure reliability of auth state change notifications
  and persistence.
- Persistence is now managed via the same notifications as auth state change

## BREAKING CHANGES v3.0 → 3.1

- We've implemented the PKCE auth flow. SignIn using a provider now returns an instance of `ProviderAuthState` rather than a `string`.
- The provider sign in signature has moved `scopes` into `SignInOptions`

In Short:
```c#
# What was:
var url = await client.SignIn(Provider.Github, "scopes and things");

# Becomes:
var state = await client.SignIn(Provider.Github, new SignInOptions { "scopes and things" });
// Url is now at `state.Uri`
```

---

## Getting Started

To use this library on the Supabase Hosted service but separately from the `supabase-csharp`, you'll need to specify your url and public key like so:
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

## Persisting, Retrieving, and Destroying Sessions.

This Gotrue client is written to be agnostic when it comes to session persistence, retrieval, and destruction. `ClientOptions` exposes
properties that allow these to be specified.

In the event these are specified and the `AutoRefreshToken` option is set, as the `Client` Initializes, it will also attempt to
retrieve, set, and refresh an existing session.

For example, using `Xamarin.Essentials` in `Xamarin.Forms`, this might look like:

```c#

var cacheFileName = ".gotrue.cache";

async void Initialize() {
    var options = new ClientOptions
    {
        Url = GOTRUE_URL,
        PersistSession = true, 
        SessionPersistor = SaveSession, // PeristenceListener public delegate bool SaveSession(Session session);
        SessionRetriever = LoadSession, // PeristenceListener public delegate Session LoadSession();
        SessionDestroyer = DestroySession // PeristenceListener  delegate void DestroySession();
    };
    var client = new Client(options);
    // Load the session from persistence
    newClient.LoadSession();
    // Loads the session using SessionRetriever and sets state internally.
    await client.RetrieveSessionAsync();
}

//...

internal bool SaveSession(Session session)
{
    try
    {
        var cacheDir = FileSystem.CacheDirectory;
        var path = Path.Join(cacheDir, cacheFileName);
        var str = JsonConvert.SerializeObject(session);

        using (StreamWriter file = new StreamWriter(path))
        {
            file.Write(str);
            file.Dispose();
            return Task.FromResult(true);
        };
    }
    catch (Exception err)
    {
        Debug.WriteLine("Unable to write cache file.");
        throw err;
    }
}
```

## 3rd Party OAuth

Once again, Gotrue client is written to be agnostic of platform. In order for Gotrue to sign in a user from an Oauth
callback, the PKCE flow is preferred:

1) The Callback Url must be set in the Supabase Admin panel
2) The Application should have listener to receive that Callback
3) Generate a sign in request using: `client.SignIn(PROVIDER, options)` and setting the options to use the PKCE `FlowType`
4) Store `ProviderAuthState.PKCEVerifier` so that the application callback can use it to verify the returned code
5) In the Callback, use stored `PKCEVerifier` and received `code` to exchange for a session.


```c#
var state = await client.SignIn(Constants.Provider.Github, new SignInOptions
{
    FlowType = Constants.OAuthFlowType.PKCE,
    RedirectTo = "http://localhost:3000/oauth/callback"
});

// In callback received from Supabase returning to RedirectTo (set above)
// Url is set as: http://REDIRECT_TO_URL?code=CODE
var session = await client.ExchangeCodeForSession(state.PKCEVerifier, RETRIEVE_CODE_FROM_GET_PARAMS);
```

## Troubleshooting

**I've created a User but while attempting to log in it throws an exception:**

Provided the credentials are correct, make sure that the User has also confirmed their email.


## Status

- [x] API
  - [x] Sign Up with Email
  - [x] Sign In with Email
  - [x] Send Magic Link Email
  - [x] Invite User by Email
  - [x] Reset Password for Email
  - [x] Signout
  - [x] Get Url for Provider
  - [x] Get User
  - [x] Update User
  - [x] Refresh Access Token
  - [x] List Users (includes filtering, sorting, pagination)
  - [x] Get User by Id
  - [x] Create User
  - [x] Update User by Id
- [x] Client
  - [x] Get User
  - [x] Refresh Session
  - [x] Auth State Change Handler
  - [x] Provider Sign In (Provides URL)
- [x] Provide Interfaces for Custom Token Persistence Functionality
- [x] Documentation
- [x] Unit Tests
- [x] Nuget Release

## Package made possible through the efforts of:

Join the ranks! See a problem? Help fix it!

<a href="https://github.com/supabase-community/gotrue-csharp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=supabase-community/gotrue-csharp" />
</a>

<small>Made with [contrib.rocks](https://contrib.rocks).</small>

## Contributing

We are more than happy to have contributions! Please submit a PR.

### Testing

To run the tests locally you must have docker and docker-compose installed. Then in the root of the repository run:
- `docker-compose up -d`
- `dotnet test`

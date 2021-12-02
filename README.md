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

## Getting Started

The Gotrue `Client` from this library uses a Singleton class to maintain in-memory state and timers. This is similar
to the method that Firebase uses in its Client libraries. It can maintain sessions, refresh tokens, persistence, etc.
for the developer, rather than having to do that yourself.

Alternatively, a `StatelessClient` is also provided within this library that allows interactions directly with the API
without requiring initialization.

```c#
var options = new ClientOptions { Url = "https://example.com/api" };
var client = await Client.Initialize(options);

var user = await SignUp("new-user@example.com");

// Alternatively, you can use a StatelessClient and do API interactions that way
var options = new StatelessClientOptions { Url = "https://example.com/api" }
await Client.SignUp("new-user@example.com", options);

```

## Persisting, Retrieving, and Destroying Sessions.

This Gotrue client is written to be agnostic when it comes to session persistance, retrieval, and destruction. `ClientOptions` exposes
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
        SessionPersistor = SessionPersistor,
        SessionRetriever = SessionRetriever,
        SessionDestroyer = SessionDestroyer
    };
    await Client.Initialize(options);
}

//...

internal Task<bool> SessionPersistor(Session session)
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

## 3rd Party Authentication and Callbacks.

Once again, Gotrue client is written to be agnostic of platform. In order for Gotrue to sign in a user from an Oauth
callback:

1) The Callback Url must be set in the Supabase Admin panel
2) The Application should recieve that Callback
3) In the Callback, the `Uri` should be passed to `Client.Instance.GetSessionFromUrl(uri)`

Setting the second parameter of `GetSessionFromUrl` to `false` will prevent the storage of the parsed `Session` object.

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

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

The Gotrue Client from this library uses a Singleton class to maintain in-memory state and timers. This is similar
to the method that Firebase uses in its Client libraries.

```c#
var options = new ClientOptions { Url = "https://example.com/api" };
var client = Client.Initialize(options);

var user = await SignUp("new-user@example.com");
```


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

| <img src="https://github.com/acupofjose.png" width="150" height="150"> |
| :--------------------------------------------------------------------: |
|              [acupofjose](https://github.com/acupofjose)               |

## Contributing

We are more than happy to have contributions! Please submit a PR.

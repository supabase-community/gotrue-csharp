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

## Documentation

- [Getting Started](Documentation/GettingStarted.md)
- [Unity](Documentation/Unity.md)
- [Desktop/Mobile Clients (e.g. Xamarin, MAUI, etc.)](Documentation/DesktopClients.md)
- [Server-Side Applications](Documentation/ServerSideApplications.md)
- [Release Notes/Breaking Changes](Documentation/ReleaseNotes.md)

### Specific Features

- [Offline Support](Documentation/OfflineSupport.md)
- [Refresh Token Thread](Documentation/RefreshTokenThread.md)
- [Native Sign in with Apple](Documentation/NativeSignInWithApple.md)

### Troubleshooting

- [Troubleshooting](Documentation/TroubleShooting.md)
- [Discussion Forum](https://github.com/supabase-community/supabase-csharp/discussions)

## Features

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

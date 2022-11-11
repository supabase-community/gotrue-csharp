# Changelog

## 3.0.1 - 2022-11-11

- `ClientOptions` interface updated to support a generic `TSession` to match the `IGotrueClient` interface. 

## 3.0.0 - 2022-11-4

- [#40](https://github.com/supabase-community/gotrue-csharp/pull/40) Adjust to Dependency Injection Structure (Thanks [@HunteRoi](https://github.com/HunteRoi))
- [#34](https://github.com/supabase-community/supabase-csharp/issues/34) Enable nullability in project.

Migration from 2.x.x to 3.x.x:
- `Client` is no longer a Singleton - it should be initialized using its standard constructor.
- `StatelessClient` is no longer `Static` - it should be initialized using a standard constructor.
- Setting/Retrieving state on init has been disabled by default, you will need to call `client.RetrieveSessionAsync()` to retrieve state from your `SessionRetriever` function.

## 2.4.7 - 2022-10-31

- [#41](https://github.com/supabase-community/gotrue-csharp/issues/41) Add support for `VerifyOTP(string email, string token)`

## 2.4.6 - 2022-10-27

- [#39](https://github.com/supabase-community/gotrue-csharp/pull/39) Added GetUser method that supports a JWT. (tahnks [@AlexMeesters](https://github.com/AlexMeesters)!)

## 2.4.5 - 2022-05-24

- [#37](https://github.com/supabase-community/gotrue-csharp/issues/37) Adds a `SetAuth` method to allow setting an arbitrary JWT token.

## 2.4.4 - 2022-05-11

- [#33](https://github.com/supabase-community/gotrue-csharp/pull/32) Refresh timer should be cancelled if the user logs out, CurrentSession object may be null in RefreshToken

## 2.4.3 - 2022-04-27

- [#32](https://github.com/supabase-community/gotrue-csharp/pull/32) RefreshToken() should take an optional refresh token from the caller (Thanks [@RedChops](https://github.com/RedChops))

## 2.4.2 - 2022-04-23

- [#30](https://github.com/supabase-community/gotrue-csharp/pull/30) Update usage of `redirectTo` to reflect gotrue-js usage and adapt `GetSessionFromUrl` to gotrue's return format. (Thanks [@RedChops](https://github.com/RedChops))

## 2.4.1 - 2022-04-13

- Changed `UpdateUserById` to require the more specific `AdminUserAttributes` instead of `UserAttributes` (Thanks [@AydinE](https://github.com/AydinE))

## 2.4.0 - 2022-03-28

- [Minor API Change] - Some `User` Model Attributes will now hydrate as `null` instead of as the object `defaults` (i.e. `ConfirmedAt`)

## 2.3.6 - 2022-02-27

- Added providers for `LinkedIn` and `Notion`

## 2.3.5 - 2022-01-19

- [#23](https://github.com/supabase-community/gotrue-csharp/pull/23) Added `redirect_url` option for MagicLink sign in (Thanks [@MisterJimson](https://github.com/MisterJimson))

## 2.3.4 - 2022-01-07

-  [#21](https://github.com/supabase-community/gotrue-csharp/pull/21) Added SignOut method to Stateless Client (Thanks [@fplaras](https://github.com/fplaras))

## 2.3.3 - 2021-12-29

- Minor: `SignUp` will return a `Session` with a *populated `User` object* on an unconfirmed signup.
    - Fixes [#19](https://github.com/supabase-community/gotrue-csharp/issues/19)
    - Developers who were using a `null` check on `Session.User` will need to adjust accordingly.

## 2.3.2 - 2021-12-25

- Minor: `SignUp` signature now uses a class `SignUpOptions` to include `Data` and `RedirectTo` options. (Ref: [supabase-community/supabase-csharp#16](https://github.com/supabase-community/supabase-csharp/issues/16))
- Fix [#17](https://github.com/supabase-community/gotrue-csharp/issues/17) and [#18](https://github.com/supabase-community/gotrue-csharp/issues/18)

## 2.3.1 - 2021-12-24

- Minor: `CreateUser` signature exchanges `object userdata` with `AdminUserAttributes attributes`.
- [#16](https://github.com/supabase-community/gotrue-csharp/issues/16) Conforms `CreateUser` to the `AdminUserAttributes` request format.

## 2.3.0 - 2021-12-23

- [#15](https://github.com/supabase-community/gotrue-csharp/issues/15) Added optional `metadata` parameter for user `SignUp` functions.
- Introduces a change into `User.AppMetadata` and `User.UserMetadata` where types are now `Dictionary<string,object>` rather than just `object`.

## 2.2.4 - 2021-12-4

- [#14](https://github.com/supabase-community/gotrue-csharp/pull/14) Implemented `ListUsers` (paginate, sort, filter), `GetUserById`, `CreateUser`, `UpdateById` (Thanks [@TheOnlyBeardedBeast](https://github.com/TheOnlyBeardedBeast])!)

## 2.2.3 - 2021-12-2

- [#11](https://github.com/supabase-community/gotrue-csharp/pull/11) Add reset password capability (Thanks [@phxtho](https://github.com/phxtho)!)

## 2.2.2 - 2021-11-29

- [#12](https://github.com/supabase-community/supabase-csharp/issues/12) Add a `AuthState.TokenRefreshed` trigger on Token Refresh (along with test).

## 2.2.1 - 2021-11-24

- [#7](https://github.com/supabase-community/supabase-csharp/issues/7) Add a `StatelessClient` static class that enables API interactions through specifying `StatelessClientOptions`
- Added tests for `StatelessClient`
- Attempting to sign up a User that already exists throws a `BadRequestException` on the latest pull of `supabase/gotrue`so the appropriate tests have been updated.
- Internally, exceptions were moved to a `ExceptionHandler` class to be shared between `Client` and `StatelessClient`
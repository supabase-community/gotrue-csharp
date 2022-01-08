# Changelog

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
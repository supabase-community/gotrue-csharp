# Changelog

## 2.2.3 - 2021-12-2

- [#11](https://github.com/supabase-community/gotrue-csharp/pull/11) Add reset password capability (Thanks [@phxtho](https://github.com/phxtho)!)

## 2.2.2 - 2021-11-29

- [#12](https://github.com/supabase-community/supabase-csharp/issues/12) Add a `AuthState.TokenRefreshed` trigger on Token Refresh (along with test).

## 2.2.1 - 2021-11-24

- [#7](https://github.com/supabase-community/supabase-csharp/issues/7) Add a `StatelessClient` static class that enables API interactions through specifying `StatelessClientOptions`
- Added tests for `StatelessClient`
- Attempting to sign up a User that already exists throws a `BadRequestException` on the latest pull of `supabase/gotrue`so the appropriate tests have been updated.
- Internally, exceptions were moved to a `ExceptionHandler` class to be shared between `Client` and `StatelessClient`
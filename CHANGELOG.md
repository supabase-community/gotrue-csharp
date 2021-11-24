# Changelog

## 2.2.1 - 2021-11-24

- [#7](https://github.com/supabase-community/supabase-csharp/issues/7) Add a `StatelessClient` static class that enables API interactions through specifying `StatelessClientOptions`
- Added tests for `StatelessClient`
- Attempting to sign up a User that already exists throws a `BadRequestException` on the latest pull of `supabase/gotrue`so the appropriate tests have been updated.
- Internally, exceptions were moved to a `ExceptionHandler` class to be shared between `Client` and `StatelessClient`
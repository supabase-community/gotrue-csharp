delete from auth.oauth_consents
where client_id in (
  '11111111-1111-1111-1111-111111111111',
  '33333333-3333-3333-3333-333333333333',
  '55555555-5555-5555-5555-555555555555',
  '99999999-9999-9999-9999-999999999999',
  'aaaaaaaa-0000-0000-0000-000000000000'
);

delete from auth.oauth_authorizations
where authorization_id in (
  'test-authorization-id',
  'test-authorization-id-002',
  'test-authorization-id-003',
  'test-authorization-id-004',
  'test-authorization-id-005',
  'test-authorization-id-006',
  'test-authorization-id-007',
  'test-authorization-id-008',
  'test-authorization-id-009',
  'test-authorization-id-010',
  'test-authorization-id-011'
);

delete from auth.oauth_authorizations
where client_id in (
  '11111111-1111-1111-1111-111111111111',
  '33333333-3333-3333-3333-333333333333',
  '55555555-5555-5555-5555-555555555555',
  '99999999-9999-9999-9999-999999999999',
  'aaaaaaaa-0000-0000-0000-000000000000'
);

delete from auth.oauth_clients
where id in (
  '11111111-1111-1111-1111-111111111111',
  '33333333-3333-3333-3333-333333333333',
  '55555555-5555-5555-5555-555555555555',
  '99999999-9999-9999-9999-999999999999',
  'aaaaaaaa-0000-0000-0000-000000000000'
);

insert into auth.oauth_clients (
  id,
  client_secret_hash,
  registration_type,
  redirect_uris,
  grant_types,
  client_name,
  client_uri,
  logo_uri,
  client_type,
  token_endpoint_auth_method,
  created_at,
  updated_at,
  deleted_at
)
values (
  'aaaaaaaa-0000-0000-0000-000000000000',
  'mock-secret-hash-009',
  'dynamic',
  'http://localhost:3000/callback',
  'authorization_code,refresh_token',
  'Mock OAuth Client 009',
  'http://localhost:3000',
  'http://localhost:3000/logo.png',
  'confidential',
  'client_secret_post',
  now(),
  now(),
  null
);

insert into auth.oauth_authorizations (
  id,
  authorization_id,
  client_id,
  user_id,
  redirect_uri,
  scope,
  state,
  resource,
  code_challenge,
  code_challenge_method,
  response_type,
  status,
  authorization_code,
  created_at,
  expires_at,
  approved_at,
  nonce
)
values
(
  'aaaaaaaa-1111-1111-1111-111111111111',
  'test-authorization-id-009',
  'aaaaaaaa-0000-0000-0000-000000000000',
  '8860434d-6126-4238-b18f-f0ddb615e19e',
  'http://localhost:3000/callback',
  'openid email profile',
  'test-state-009',
  null,
  'E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM',
  's256',
  'code',
  'pending',
  null,
  now(),
  now() + interval '30 minutes',
  null,
  'test-nonce-009'
),
(
  'aaaaaaaa-2222-2222-2222-222222222222',
  'test-authorization-id-010',
  'aaaaaaaa-0000-0000-0000-000000000000',
  '8860434d-6126-4238-b18f-f0ddb615e19e',
  'http://localhost:3000/callback',
  'openid email profile',
  'test-state-010',
  null,
  'E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM',
  's256',
  'code',
  'pending',
  null,
  now(),
  now() + interval '30 minutes',
  null,
  'test-nonce-010'
),
(
  'aaaaaaaa-3333-3333-3333-333333333333',
  'test-authorization-id-011',
  'aaaaaaaa-0000-0000-0000-000000000000',
  '8860434d-6126-4238-b18f-f0ddb615e19e',
  'http://localhost:3000/callback',
  'openid email profile',
  'test-state-011',
  null,
  'E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM',
  's256',
  'code',
  'pending',
  null,
  now(),
  now() + interval '30 minutes',
  null,
  'test-nonce-011'
);

insert into auth.oauth_consents (
  id,
  user_id,
  client_id,
  scopes,
  granted_at,
  revoked_at
)
values (
  'aaaaaaaa-4444-4444-4444-444444444444',
  '8860434d-6126-4238-b18f-f0ddb615e19e',
  'aaaaaaaa-0000-0000-0000-000000000000',
  'openid email profile',
  now(),
  null
);
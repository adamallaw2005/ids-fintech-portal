import assert from 'node:assert/strict';

const base = process.env.API_URL ?? 'http://localhost:5149/api';
const email = process.env.TEST_USER_EMAIL;
const password = process.env.TEST_USER_PASSWORD;
if (!email || !password || !process.env.TEST_ADMIN_PASSWORD) {
  throw new Error('Set TEST_USER_EMAIL, TEST_USER_PASSWORD, and TEST_ADMIN_PASSWORD. Use a disposable test user.');
}
let checks = 0;
async function request(path, token, method = 'GET', body, expected = 200) {
  const response = await fetch(base + path, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  assert.equal(response.status, expected, `${method} ${path}`);
  checks++;
  return response.headers.get('content-type')?.includes('json') ? response.json() : null;
}
async function login(accountEmail, accountPassword) {
  return request('/auth/login', null, 'POST', { email: accountEmail, password: accountPassword });
}
const admin = await login(process.env.TEST_ADMIN_EMAIL ?? 'admin@idsfintech.com', process.env.TEST_ADMIN_PASSWORD);
const user = await login(email, password);
assert.equal(user.user.roleName, 'User', 'The test account must start as a normal user.');
const resources = ['products', 'clients', 'deployments', 'environments', 'team-members',
  'repositories', 'documents', 'product-modules', 'deployment-modules',
  'product-responsibilities', 'client-responsibilities'];
await request('/dashboard', user.token);
for (const resource of resources) {
  const rows = await request(`/${resource}`, user.token);
  await request(`/${resource}/-1`, user.token, 'GET', undefined, 404);
  await request(`/${resource}`, user.token, 'POST', {}, 403);
  await request(`/${resource}/-1`, user.token, 'PUT', {}, 403);
  await request(`/${resource}/-1`, user.token, 'DELETE', undefined, 403);
  await request(`/${resource}`, admin.token, 'POST', {}, 400);
  await request(`/${resource}/-1`, admin.token, 'PUT', {}, 400);
  await request(`/${resource}/-1`, admin.token, 'DELETE', undefined, 404);
  if (rows.length && ['products', 'clients'].includes(resource)) {
    const id = rows[0][resource === 'products' ? 'productId' : 'clientId'];
    await request(`/${resource}/${id}/details`, user.token);
  }
}
await request('/products?search=IDS', user.token);
await request('/clients?country=Lebanon', user.token);
await request('/deployments?status=Testing', user.token);
await request('/users', user.token, 'GET', undefined, 403);
await request('/users', user.token, 'POST', {}, 403);
for (const suffix of ['', '/role', '/status']) {
  await request(`/users/-1${suffix}`, user.token, 'PUT', {}, 403);
}
await request('/auth/admin-password/setup', user.token, 'POST', {}, 403);
await request('/auth/admin-password/change', user.token, 'POST', {}, 403);
await request('/auth/become-admin', user.token, 'POST', { adminPromotionPassword: 'incorrect-role-test-password' }, 400);
const changedPassword = `${password}-changed`;
try {
  await request('/auth/change-password', user.token, 'POST', { currentPassword: password, newPassword: changedPassword }, 204);
  await login(email, changedPassword);
} finally {
  await request('/auth/change-password', user.token, 'POST', { currentPassword: changedPassword, newPassword: password }, 204);
}
if (process.env.TEST_PROMOTION_PASSWORD) {
  try {
    const promoted = await request('/auth/become-admin', user.token, 'POST', {
      adminPromotionPassword: process.env.TEST_PROMOTION_PASSWORD,
    });
    assert.equal(promoted.user.roleName, 'Admin');
    await request('/users', promoted.token);
    await request('/products', promoted.token, 'POST', {}, 400);
    await request('/products', user.token, 'GET', undefined, 401);
  } finally {
    await request(`/users/${user.user.userId}/role`, admin.token, 'PUT', { roleName: 'User' }, 204);
  }
}
await request('/auth/logout', (await login(email, password)).token, 'POST', {}, 204);
console.log(`Passed ${checks} role-access checks.`);

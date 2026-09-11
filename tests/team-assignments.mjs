import assert from 'node:assert/strict';

const base = process.env.API_URL ?? 'http://localhost:5149/api';
if (!process.env.TEST_ADMIN_PASSWORD) throw new Error('Set TEST_ADMIN_PASSWORD.');
let token;
let checks = 0;
const cleanup = [];
async function request(path, method = 'GET', body, expected = 200) {
  const response = await fetch(base + path, {
    method, headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  assert.equal(response.status, expected, `${method} ${path}: ${response.status}`);
  checks++;
  return response.headers.get('content-type')?.includes('json') ? response.json() : null;
}
token = (await request('/auth/login', 'POST', {
  email: process.env.TEST_ADMIN_EMAIL ?? 'admin@idsfintech.com', password: process.env.TEST_ADMIN_PASSWORD,
})).token;
try {
  const product = await request('/products', 'POST', { productName: `Team test ${Date.now()}` }, 201);
  cleanup.push(`/products/${product.productId}`);
  const client = await request('/clients', 'POST', { companyName: `Team test ${Date.now()}` }, 201);
  cleanup.push(`/clients/${client.clientId}`);
  const p = { targetId: product.productId, responsibilityRole: 'Owner', description: 'Product responsibility' };
  const c = { targetId: client.clientId, responsibilityRole: 'Account manager', description: 'Client responsibility' };
  const fields = { fullName: `Team assignment test ${Date.now()}`, status: 'Active' };
  const member = await request('/team-members', 'POST', { ...fields, productAssignments: [p], clientAssignments: [c] }, 201);
  const path = `/team-members/${member.teamMemberId}`;
  cleanup.push(path);
  const assignments = kind => request(`/${kind}-responsibilities?teamMemberId=${member.teamMemberId}`);
  const originalProducts = await assignments('product');
  const originalClients = await assignments('client');
  assert.equal(originalProducts[0].productName, product.productName);
  assert.equal(originalClients[0].companyName, client.companyName);
  await request(path, 'PUT', fields, 204);
  assert.deepEqual(await assignments('product'), originalProducts);
  assert.deepEqual(await assignments('client'), originalClients);
  await request(path, 'PUT', { ...fields, productAssignments: null, clientAssignments: null }, 204);
  assert.deepEqual(await assignments('product'), originalProducts);
  await request(path, 'PUT', { ...fields, productAssignments: [p, { ...p, responsibilityRole: 'Support' }] }, 204);
  assert.equal((await assignments('product')).length, 2);
  const summary = (await request('/team-members')).find(row => row.teamMemberId === member.teamMemberId);
  assert.equal(summary.productResponsibilityCount, 1, 'Count distinct products, not roles.');
  assert.equal(summary.clientResponsibilityCount, 1);
  const snapshot = await assignments('product');
  await request(path, 'PUT', { ...fields, fullName: 'Must roll back', productAssignments: [], clientAssignments: [{ ...c, targetId: 2147483647 }] }, 409);
  assert.equal((await request(path)).fullName, fields.fullName);
  assert.deepEqual(await assignments('product'), snapshot);
  assert.deepEqual(await assignments('client'), originalClients);
  for (const key of ['productAssignments', 'clientAssignments']) {
    for (const invalid of [[null], [{ targetId: 0, responsibilityRole: 'Owner' }], [{ targetId: p.targetId, responsibilityRole: ' ' }]]) {
      await request(path, 'PUT', { ...fields, [key]: invalid }, 400);
      await request('/team-members', 'POST', { ...fields, [key]: invalid }, 400);
    }
  }
  await request('/team-members', 'POST', { ...fields, productAssignments: [p], clientAssignments: [{ ...c, targetId: 2147483647 }] }, 409);
  assert.equal((await request(`/team-members?search=${encodeURIComponent(fields.fullName)}`)).length, 1, 'Failed create must leave no member.');
  await request(path, 'PUT', { ...fields, productAssignments: [], clientAssignments: [{ ...c, responsibilityRole: 'Lead', description: 'Changed' }] }, 204);
  assert.equal((await assignments('product')).length, 0);
  assert.equal((await assignments('client'))[0].responsibilityRole, 'Lead');
  await request(path, 'PUT', { ...fields, productAssignments: [p], clientAssignments: [] }, 204);
  assert.equal((await assignments('client')).length, 0);
  await request(path, 'PUT', { ...fields, clientAssignments: [c] }, 204);
  await request(path, 'DELETE', undefined, 204);
  cleanup.pop();
  assert.equal((await assignments('product')).length, 0);
  assert.equal((await assignments('client')).length, 0);
  await request(`/products/${product.productId}`);
  await request(`/clients/${client.clientId}`);
  await request(path, 'DELETE', undefined, 404);
  await request(path, 'PUT', fields, 404);
} finally {
  for (const path of cleanup.reverse()) await request(path, 'DELETE', undefined, 204);
}
console.log(`Passed ${checks} team assignment API checks with rollback, preservation, validation and deletion assertions. Test data removed.`);

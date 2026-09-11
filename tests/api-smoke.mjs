import assert from 'node:assert/strict';

const base = process.env.API_URL ?? 'http://localhost:5149/api';
const password = process.env.TEST_ADMIN_PASSWORD;
if (!password) throw new Error('Set TEST_ADMIN_PASSWORD before running this test.');
let token;
let checks = 0;
const cleanup = [];
async function request(path, method = 'GET', body, expected = 200) {
  const response = await fetch(base + path, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  assert.equal(response.status, expected, `${method} ${path}`);
  checks++;
  return response.status === 204 || !response.headers.get('content-type')?.includes('json')
    ? null : response.json();
}

try {
  await request('/products', 'GET', undefined, 401);
  await request('/auth/login', 'POST', {}, 400);
  const auth = await request('/auth/login', 'POST', {
    email: process.env.TEST_ADMIN_EMAIL ?? 'admin@idsfintech.com', password,
  });
  token = auth.token;
  for (const resource of ['dashboard', 'products', 'clients', 'deployments', 'environments',
    'team-members', 'repositories', 'documents', 'users', 'product-modules',
    'deployment-modules', 'product-responsibilities', 'client-responsibilities']) {
    await request(`/${resource}`);
  }
  const suffix = Date.now();
  async function create(resource, body, key) {
    const item = await request(`/${resource}`, 'POST', body, 201);
    const path = `/${resource}/${item[key]}`;
    cleanup.push(path);
    await request(path);
    await request(path, 'PUT', body, 204);
    return item[key];
  }
  const productId = await create('products', {
    productName: `Review product ${suffix}`, lifecycleStatus: 'Planned', criticality: 'Medium',
  }, 'productId');
  const clientId = await create('clients', {
    companyName: `Review client ${suffix}`, status: 'Prospect',
  }, 'clientId');
  const deploymentId = await create('deployments', {
    clientId, productId, productVersion: '1.0', deploymentStatus: 'Testing',
  }, 'deploymentId');
  await create('environments', {
    deploymentId, environmentName: `Review environment ${suffix}`, environmentType: 'Testing',
  }, 'environmentId');
  const moduleId = await create('product-modules', {
    productId, moduleName: `Review module ${suffix}`, status: 'Active',
  }, 'moduleId');
  await create('deployment-modules', { deploymentId, moduleId }, 'deploymentModuleId');
  await create('repositories', {
    productId, repositoryName: `Review repository ${suffix}`, gitHubUrl: 'https://github.com/example/review',
  }, 'repositoryId');
  await create('documents', {
    productId, documentName: `Review document ${suffix}`, documentType: 'Technical Documentation',
    urlFileReference: 'review-test.txt',
  }, 'documentId');
  const teamMemberId = await create('team-members', {
    fullName: `Review member ${suffix}`, status: 'Active',
  }, 'teamMemberId');
  await create('product-responsibilities', {
    productId, teamMemberId, responsibilityRole: 'Developer',
  }, 'responsibilityId');
  await create('client-responsibilities', {
    clientId, teamMemberId, responsibilityRole: 'Support',
  }, 'clientResponsibilityId');
  await request(`/products/${productId}/details`);
  await request(`/clients/${clientId}/details`);
  await request('/deployments', 'POST', { clientId, productId, productVersion: '1.0', deploymentStatus: 'Invalid' }, 409);
} finally {
  const failures = [];
  for (const path of cleanup.reverse()) {
    try {
      await request(path, 'DELETE', undefined, 204);
      await request(path, 'GET', undefined, 404);
    } catch (error) { failures.push(error); }
  }
  if (failures.length) throw new AggregateError(failures, 'Temporary record cleanup failed.');
}
console.log(`Passed ${checks} API checks; temporary records removed.`);

import assert from 'node:assert/strict';

const base = process.env.API_URL ?? 'http://localhost:5149/api';
if (!process.env.TEST_ADMIN_PASSWORD) throw new Error('Set TEST_ADMIN_PASSWORD.');
let token;
let checks = 0;
const parents = [];
async function request(path, method = 'GET', body, status = 200) {
  const response = await fetch(base + path, {
    method, headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  assert.equal(response.status, status, `${method} ${path}`);
  checks++;
  return response.headers.get('content-type')?.includes('json') ? response.json() : null;
}
token = (await request('/auth/login', 'POST', {
  email: process.env.TEST_ADMIN_EMAIL ?? 'admin@idsfintech.com', password: process.env.TEST_ADMIN_PASSWORD,
})).token;
const members = await request('/team-members?status=Active');
assert.ok(members.length >= 2, 'Two active team members are required.');
try {
  for (const kind of ['product', 'client']) {
    const nameKey = kind === 'product' ? 'productName' : 'companyName';
    const fields = { [nameKey]: `Edit assignment test ${kind} ${Date.now()}` };
    const assignments = members.slice(0, 2).map((member, index) => ({
      teamMemberId: member.teamMemberId, responsibilityRole: index ? 'Support' : 'Owner', description: `Original ${index}`,
    }));
    const item = await request(`/${kind}s`, 'POST', { ...fields, responsibilities: assignments }, 201);
    const path = `/${kind}s/${item[`${kind}Id`]}`;
    parents.push({ kind, path });
    const before = await request(`${path}/details`);
    await request(path, 'PUT', { ...fields, notes: 'Parent-only edit' }, 204);
    assert.deepEqual((await request(`${path}/details`)).responsibilities, before.responsibilities,
      'Omitting assignments must preserve rows and their IDs.');
    await request(path, 'PUT', { ...fields, responsibilities: null }, 204);
    assert.deepEqual((await request(`${path}/details`)).responsibilities, before.responsibilities);

    const changed = [{ ...assignments[0], responsibilityRole: 'Technical lead', description: 'Updated role' }];
    await request(path, 'PUT', { ...fields, responsibilities: changed }, 204);
    let details = await request(`${path}/details`);
    assert.equal(details.responsibilities.length, 1, 'The removed assignment must disappear.');
    assert.equal(details.responsibilities[0].responsibilityRole, 'Technical lead');
    assert.equal(details.responsibilities[0].description, 'Updated role');
    await request(path, 'PUT', { ...fields, responsibilities: [...changed, assignments[1]] }, 204);
    details = await request(`${path}/details`);
    assert.equal(details.responsibilities.length, 2, 'The new assignment must be added.');
    const snapshot = details.responsibilities;
    await request(path, 'PUT', { ...fields, notes: 'Must not be saved', responsibilities: [
      changed[0], { ...assignments[1], teamMemberId: 2147483647 },
    ] }, 409);
    details = await request(`${path}/details`);
    assert.deepEqual(details.responsibilities, snapshot, 'Failed edit must preserve all original assignment rows.');
    assert.notEqual(details[kind].notes, 'Must not be saved', 'Failed edit must roll back parent fields.');
    for (const invalid of [[null], [{ ...changed[0], responsibilityRole: ' ' }], [{ ...changed[0], teamMemberId: 0 }]]) {
      await request(path, 'PUT', { ...fields, responsibilities: invalid }, 400);
    }
    await request(`/${kind}s/-1`, 'PUT', { ...fields, responsibilities: assignments }, 404);
    await request(path, 'PUT', { ...fields, responsibilities: [] }, 204);
    assert.equal((await request(`${path}/details`)).responsibilities.length, 0, 'An explicit empty list removes all assignments.');
    await request(path, 'PUT', { ...fields, responsibilities: [assignments[0]] }, 204);
    assert.equal((await request(`${path}/details`)).responsibilities.length, 1, 'Assignments can be added after clearing.');
  }
} finally {
  const failures = [];
  for (const { kind, path } of parents.reverse()) {
    try {
      const data = await request(`${path}/details`);
      for (const row of data.responsibilities) {
        await request(`/${kind}-responsibilities/${row.responsibilityId ?? row.clientResponsibilityId}`, 'DELETE', undefined, 204);
      }
      await request(path, 'DELETE', undefined, 204);
    } catch (error) { failures.push(error); }
  }
  if (failures.length) throw new AggregateError(failures, 'Cleanup failed.');
}
console.log(`Passed ${checks} edit-assignment API checks and preservation/rollback assertions. Test records removed.`);

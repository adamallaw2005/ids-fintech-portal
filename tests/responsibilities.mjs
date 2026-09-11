import assert from 'node:assert/strict';

const base = process.env.API_URL ?? 'http://localhost:5149/api';
if (!process.env.TEST_ADMIN_PASSWORD) throw new Error('Set TEST_ADMIN_PASSWORD.');
let token;
let checks = 0;
const cleanup = [];
async function request(path, method = 'GET', body, status = 200) {
  const response = await fetch(base + path, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  assert.equal(response.status, status, `${method} ${path}`);
  checks++;
  return response.headers.get('content-type')?.includes('json') ? response.json() : null;
}
const auth = await request('/auth/login', 'POST', {
  email: process.env.TEST_ADMIN_EMAIL ?? 'admin@idsfintech.com', password: process.env.TEST_ADMIN_PASSWORD,
});
token = auth.token;
const team = await request('/team-members?status=Active');
assert.ok(team.length >= 2, 'Two active team members are required for these tests.');
try {
  for (const kind of ['product', 'client']) {
    const resource = `/${kind}s`;
    const key = `${kind}Id`;
    const assignmentKey = kind === 'product' ? 'responsibilityId' : 'clientResponsibilityId';
    const nameKey = kind === 'product' ? 'productName' : 'companyName';
    const name = `Assignment review ${kind} ${Date.now()}`;
    const fields = { [nameKey]: name };
    const assignments = team.slice(0, 2).map((member, index) => ({
      teamMemberId: member.teamMemberId,
      responsibilityRole: index ? 'Support engineer' : 'Technical owner',
      description: `Test responsibility ${index + 1}`,
    }));
    const item = await request(resource, 'POST', { ...fields, responsibilities: assignments }, 201);
    const path = `${resource}/${item[key]}`;
    cleanup.push(path);
    let details = await request(`${path}/details`);
    assert.equal(details.responsibilities.length, 2);
    for (const assignment of details.responsibilities) {
      cleanup.push(`/${kind}-responsibilities/${assignment[assignmentKey]}`);
      assert.ok(assignments.some((expected) => expected.teamMemberId === assignment.teamMemberId &&
        expected.responsibilityRole === assignment.responsibilityRole && expected.description === assignment.description));
    }
    const first = details.responsibilities[0];
    await request(`/${kind}-responsibilities/${first[assignmentKey]}`, 'PUT', {
      [key]: item[key], teamMemberId: first.teamMemberId,
      responsibilityRole: 'Account manager', description: 'Updated description',
    }, 204);
    details = await request(`${path}/details`);
    assert.equal(details.responsibilities.find((row) => row[assignmentKey] === first[assignmentKey]).description, 'Updated description');
    await request(path, 'PUT', fields, 204);
    assert.equal((await request(`${path}/details`)).responsibilities.length, 2, 'Editing the parent must preserve assignments.');

    const rollbackName = `${name} rollback`;
    await request(resource, 'POST', {
      [nameKey]: rollbackName,
      responsibilities: [assignments[0], { ...assignments[1], teamMemberId: 2147483647 }],
    }, 409);
    assert.equal((await request(`${resource}?search=${encodeURIComponent(rollbackName)}`)).length, 0,
      'Failed assignment must roll back the parent and earlier assignments.');
    for (const responsibilities of [null, [null], [{ ...assignments[0], responsibilityRole: '' }], [{ ...assignments[0], teamMemberId: 0 }]]) {
      await request(resource, 'POST', { ...fields, responsibilities }, 400);
    }
    const unassigned = await request(resource, 'POST', { [nameKey]: `${name} unassigned` }, 201);
    const unassignedPath = `${resource}/${unassigned[key]}`;
    cleanup.push(unassignedPath);
    assert.equal((await request(`${unassignedPath}/details`)).responsibilities.length, 0);
    const later = await request(`/${kind}-responsibilities`, 'POST', { ...assignments[0], [key]: unassigned[key] }, 201);
    cleanup.push(`/${kind}-responsibilities/${later[assignmentKey]}`);
    assert.equal((await request(`${unassignedPath}/details`)).responsibilities.length, 1);
  }
} finally {
  const errors = [];
  for (const path of cleanup.reverse()) {
    try { await request(path, 'DELETE', undefined, 204); }
    catch (error) { errors.push(error); }
  }
  if (errors.length) throw new AggregateError(errors, 'Test record cleanup failed.');
}
console.log(`Passed ${checks} assignment API checks and persistence/rollback assertions. Test records removed.`);

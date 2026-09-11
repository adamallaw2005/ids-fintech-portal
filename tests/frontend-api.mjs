import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
import { pathToFileURL } from 'node:url';

const require = createRequire(new URL('../frontend/client/package.json', import.meta.url));
const ts = require('typescript');
const source = await readFile(new URL('../frontend/client/src/lib/api.ts', import.meta.url), 'utf8');
const compiled = ts.transpileModule(source, {
  compilerOptions: { module: ts.ModuleKind.ESNext, target: ts.ScriptTarget.ES2022 },
}).outputText
  .replace('"react"', JSON.stringify(pathToFileURL(require.resolve('react')).href))
  .replace('import.meta.env.VITE_API_URL', 'undefined');
const { api } = await import(`data:text/javascript;base64,${Buffer.from(compiled).toString('base64')}`);
const storage = new Map([['ids_token', 'test-token'], ['ids_user', '{}']]);
const events = [];
Object.defineProperty(globalThis, 'localStorage', { value: {
  getItem: (key) => storage.get(key) ?? null,
  removeItem: (key) => storage.delete(key),
}, configurable: true });
globalThis.window = { dispatchEvent: (event) => events.push(event.type) };
globalThis.fetch = async (url, options) => {
  assert.equal(url, 'http://localhost:5149/api/products');
  assert.equal(options.headers.get('Authorization'), 'Bearer test-token');
  return Response.json([{ productId: 1 }]);
};
assert.deepEqual(await api('/products'), [{ productId: 1 }]);
globalThis.fetch = async () => new Response(null, { status: 204 });
assert.equal(await api('/products/1', { method: 'DELETE' }), null);
globalThis.fetch = async () => new Response('A record already exists.', { status: 409 });
await assert.rejects(api('/products'), /A record already exists/);
globalThis.fetch = async () => Response.json({
  title: 'Validation failed', errors: { Email: ['Email is required.'] },
}, { status: 400 });
await assert.rejects(api('/products'), /Email is required/);
globalThis.fetch = async () => new Response('{broken', {
  status: 500, headers: { 'Content-Type': 'application/json' },
});
await assert.rejects(api('/products'), /Request failed \(500\)/);
globalThis.fetch = async () => new Response(null, { status: 401 });
await assert.rejects(api('/products'), /Request failed \(401\)/);
assert.equal(storage.size, 0);
assert.deepEqual(events, ['ids-auth-expired']);
console.log('Passed 6 frontend API scenarios.');

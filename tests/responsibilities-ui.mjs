import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { fileURLToPath, pathToFileURL } from 'node:url';

const require = createRequire(new URL('../frontend/client/package.json', import.meta.url));
const { createServer } = await import(pathToFileURL(require.resolve('vite')));
const React = require('react');
const { renderToStaticMarkup } = require('react-dom/server');
const server = await createServer({
  root: fileURLToPath(new URL('../frontend/client', import.meta.url)),
  server: { middlewareMode: true }, appType: 'custom',
});
try {
  const { ResponsibilityPanel } = await server.ssrLoadModule('/src/components/Responsibilities.tsx');
  for (const kind of ['product', 'client']) {
    for (const populated of [false, true]) {
      const props = {
        kind, parentId: 1, onChanged() {},
        data: populated ? [{ responsibilityId: 1, clientResponsibilityId: 1,
          teamMemberId: 1, teamMemberName: 'Test employee', responsibilityRole: 'Owner', description: 'Test description' }] : [],
      };
      const readonly = renderToStaticMarkup(React.createElement(ResponsibilityPanel, { ...props, canManage: false }));
      assert.ok(!readonly.includes('<button'), 'Normal users must not see assignment controls.');
      const admin = renderToStaticMarkup(React.createElement(ResponsibilityPanel, { ...props, canManage: true }));
      assert.ok(admin.includes('+ Assign team member'));
      if (populated) {
        assert.ok(readonly.includes('Test employee') && readonly.includes('Test description'));
        assert.ok(admin.includes('>Edit</button>') && admin.includes('>Remove</button>'));
      }
    }
  }
  console.log('Passed 8 assignment panel rendering scenarios for admin and read-only users.');
} finally {
  await server.close();
}

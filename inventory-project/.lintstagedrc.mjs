import path from 'node:path';

// lint-staged passes absolute paths, but `dotnet format --include` silently
// ignores them, so convert each one to a path relative to this directory.
const toRelative = (file) => `"${path.relative(import.meta.dirname, file)}"`;

export default {
    '*.cs': (files) => `dotnet format Inventory.sln --include ${files.map(toRelative).join(' ')}`,
};

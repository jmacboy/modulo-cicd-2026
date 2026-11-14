import { defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config.js';

export default mergeConfig(
    viteConfig,
    defineConfig({
        test: {
            environment: 'jsdom',
            setupFiles: ['./vitest.setup.js'],
            // Only Vitest unit tests; the Mocha Pact specs in src/tests/** run with `yarn test:pact`.
            include: ['src/**/*.test.{js,jsx}'],
            restoreMocks: true,
            coverage: {
                provider: 'v8',
                reporter: ['text', 'lcov', 'html'],
                reportsDirectory: 'coverage',
                include: ['src/**/*.{js,jsx}'],
                exclude: ['src/tests/**', 'src/**/*.test.*', 'src/main.jsx', 'vitest.setup.js'],
            },
        },
    }),
);

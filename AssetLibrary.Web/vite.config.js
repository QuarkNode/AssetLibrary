import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
    build: {
        outDir: 'wwwroot/dist',
        emptyOutDir: true,

        rollupOptions: {
            input: {
                viewer: resolve(
                    __dirname,
                    'wwwroot/js/viewer.js'
                )
            }
        }
    }
});
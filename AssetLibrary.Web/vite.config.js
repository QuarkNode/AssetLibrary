import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
    base: './',

    build: {
        outDir: 'wwwroot/dist',
        emptyOutDir: true,

        rolldownOptions: {
            input: {
                viewer: resolve(__dirname, 'wwwroot/js/viewer.js'),
                uploadThumbnail: resolve(__dirname, 'wwwroot/js/upload-thumbnail.js'),
                thumbnailRenderer: resolve(__dirname, 'thumbnail-renderer.html')
            }
        }
    }
});
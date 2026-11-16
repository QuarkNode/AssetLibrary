
import { generateThumbnail } from './thumbnail-renderer.js';

const modelFileInput = document.getElementById('File');
const thumbnailInput = document.getElementById('Thumbnail');

if (modelFileInput && thumbnailInput) {

    let generationId = 0;

    modelFileInput.addEventListener('change', async () => {

        const currentGenerationId = ++generationId;
        thumbnailInput.value = '';

        const file = modelFileInput.files[0];

        if (!file) {
            return;
        }

        try {
            const thumbnailBlob = await generateThumbnail(file, 512);

            // Ignore results from an earlier file selection.
            if (currentGenerationId !== generationId) {
                return;
            }

            const thumbnailFile = new File(
                [thumbnailBlob],
                'thumbnail.png',
                { type: 'image/png' }
            );

            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(thumbnailFile);

            thumbnailInput.files = dataTransfer.files;

            console.log('Thumbnail generated and attached successfully.');

        } catch (error) {

            console.error('Thumbnail generation failed:', error);

            // Thumbnail is optional; original upload can still proceed.
        }
    });
}


import {
    generateThumbnail,
    generateProjectThumbnail
} from './thumbnail-renderer.js';

const modelFileInput = document.getElementById('File');
const thumbnailInput = document.getElementById('Thumbnail');
const projectThumbnailElement = document.getElementById('projectThumbnailUpdater');

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

if (projectThumbnailElement) {

    async function updateProjectThumbnail() {

        const projectId = projectThumbnailElement.dataset.projectId;
        const modelPathsUrl = projectThumbnailElement.dataset.modelPathsUrl;
        const saveThumbnailUrl = projectThumbnailElement.dataset.saveThumbnailUrl;

        try {

            // Retrieve the current model paths.
            const pathsResponse = await fetch(
                `${modelPathsUrl}?id=${encodeURIComponent(projectId)}`
            );

            if (!pathsResponse.ok) {
                throw new Error('Could not retrieve project model paths.');
            }

            const modelPaths = await pathsResponse.json();

            // Prepare the request.
            const formData = new FormData();
            formData.append('projectId', projectId);

            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value;

            if (!token) {
                throw new Error('Antiforgery token was not found.');
            }

            formData.append('__RequestVerificationToken', token);

            // If the project has no assets, send an empty request.
            // The controller will clear the existing thumbnail.
            if (modelPaths.length === 0) {

                console.log('Project contains no models. Clearing thumbnail.');

            } else {

                // Download the models and convert them into File objects.
                const files = await Promise.all(
                    modelPaths.map(async path => {

                        const response = await fetch(path);

                        if (!response.ok) {
                            throw new Error(`Could not load model: ${path}`);
                        }

                        const blob = await response.blob();

                        const fileName = decodeURIComponent(
                            new URL(path, window.location.origin)
                                .pathname.split('/')
                                .pop()
                        );

                        return new File([blob], fileName, {
                            type: blob.type
                        });
                    })
                );

                // Generate the complete project thumbnail.
                const thumbnailBlob = await generateProjectThumbnail(files, 512);

                // Attach the generated thumbnail.
                formData.append(
                    'thumbnail',
                    thumbnailBlob,
                    'project-thumbnail.png'
                );
            }

            // Save or clear the project thumbnail.
            const saveResponse = await fetch(saveThumbnailUrl, {
                method: 'POST',
                body: formData
            });

            if (!saveResponse.ok) {
                throw new Error(
                    modelPaths.length === 0
                        ? 'Could not clear project thumbnail.'
                        : 'Could not save project thumbnail.'
                );
            }

            console.log(
                modelPaths.length === 0
                    ? 'Project thumbnail cleared successfully.'
                    : 'Project thumbnail updated successfully.'
            );

        } catch (error) {

            console.error('Project thumbnail generation failed:', error);

        }
    }

    updateProjectThumbnail();
}

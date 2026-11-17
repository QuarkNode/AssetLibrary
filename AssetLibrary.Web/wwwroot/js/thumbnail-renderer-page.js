import {
    generateThumbnail,
    generateProjectThumbnail as renderProjectThumbnail
} from './thumbnail-renderer.js';

async function loadModelFile(modelUrl) {
    const response = await fetch(modelUrl);

    if (!response.ok) {
        throw new Error(
            `Failed to load model at ${modelUrl}: ${response.status}`
        );
    }

    const blob = await response.blob();
    const fileName = modelUrl.split('/').pop();

    return new File(
        [blob],
        fileName,
        { type: blob.type }
    );
}

async function convertThumbnailToBase64(thumbnail) {
    const arrayBuffer = await thumbnail.arrayBuffer();
    const bytes = new Uint8Array(arrayBuffer);

    let binary = '';

    for (const byte of bytes) {
        binary += String.fromCharCode(byte);
    }

    return btoa(binary);
}

// Generate a thumbnail for a single model.
async function generateModelThumbnail(modelUrl) {
    const file = await loadModelFile(modelUrl);
    const thumbnail = await generateThumbnail(file, 512);

    return convertThumbnailToBase64(thumbnail);
}

// Generate a thumbnail for an assembled project.
async function generateProjectThumbnail(modelUrls) {
    if (!Array.isArray(modelUrls) || modelUrls.length === 0) {
        throw new Error('At least one model URL is required.');
    }

    const files = await Promise.all(
        modelUrls.map(loadModelFile)
    );

    const thumbnail = await renderProjectThumbnail(files, 512);

    return convertThumbnailToBase64(thumbnail);
}

Object.assign(window, {
    generateModelThumbnail,
    generateProjectThumbnail
});
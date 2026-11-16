
import { generateThumbnail } from './thumbnail-renderer.js';

async function generateModelThumbnail(modelUrl) {
    const response = await fetch(modelUrl);

    if (!response.ok) {
        throw new Error(`Failed to load model at ${modelUrl}: ${response.status}`);
    }

    const blob = await response.blob();
    const fileName = modelUrl.split('/').pop();

    const file = new File(
        [blob],
        fileName,
        { type: blob.type }
    );

    const thumbnail = await generateThumbnail(file, 512);
    const arrayBuffer = await thumbnail.arrayBuffer();
    const bytes = new Uint8Array(arrayBuffer);

    let binary = '';

    for (const byte of bytes) {
        binary += String.fromCharCode(byte);
    }

    return btoa(binary);
}

Object.assign(window, { generateModelThumbnail });

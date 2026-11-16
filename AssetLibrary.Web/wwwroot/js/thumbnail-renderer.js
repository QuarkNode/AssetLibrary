
import * as THREE from 'three';

import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader.js';

/**
 * Generates a PNG thumbnail from an uploaded 3D model.
 *
 * @param {File} file - Uploaded 3D model.
 * @param {number} size - Thumbnail width and height.
 * @returns {Promise<Blob>} Generated PNG image.
 */

// Generate a Matcap texture programmatically.
function createMatcapTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;

    const context = canvas.getContext('2d');

    if (!context) {
        throw new Error('Could not create Matcap texture.');
    }

    const gradient = context.createRadialGradient(
        85, 75, 10,
        128, 128, 150
    );

    gradient.addColorStop(0, '#ffffff');
    gradient.addColorStop(0.25, '#dce5ed');
    gradient.addColorStop(0.55, '#8da9b8');
    gradient.addColorStop(0.8, '#526b7a');
    gradient.addColorStop(1, '#263945');

    context.fillStyle = gradient;
    context.fillRect(0, 0, 256, 256);

    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;

    return texture;
}

export async function generateThumbnail(file, size = 512) {

    let model = null;
    let renderer = null;
    let matcapTexture = null;

    const originalMaterials = new Set();

    try {

        // Validate input.
        if (!file || !(file instanceof File)) {
            throw new Error('A valid model file is required.');
        }

        if (!Number.isInteger(size) || size < 64 || size > 2048) {
            throw new Error('Thumbnail size must be between 64 and 2048 pixels.');
        }

        const extension = file.name
            .split('.')
            .pop()
            .toLowerCase();

        const buffer = await file.arrayBuffer();

        // Load model.
        switch (extension) {

            case 'obj': {
                const text = new TextDecoder().decode(buffer);
                model = new OBJLoader().parse(text);
                break;
            }

            case 'stl': {
                const geometry = new STLLoader().parse(buffer);
                geometry.computeVertexNormals();

                const material = new THREE.MeshStandardMaterial({
                    color: 0x8da9b8,
                    metalness: 0.25,
                    roughness: 0.55
                });

                model = new THREE.Mesh(geometry, material);
                break;
            }

            case 'fbx': {
                model = new FBXLoader().parse(buffer, '');
                break;
            }

            default:
                throw new Error(`Unsupported 3D model format: .${extension}`);
        }

        if (!model) {
            throw new Error('Could not load the 3D model.');
        }

        // Create scene.
        const scene = new THREE.Scene();
        scene.background = null;
        scene.add(model);

        // Create Matcap texture.
        matcapTexture = createMatcapTexture();

        // Replace model materials with Matcap materials.
        model.traverse(object => {

            if (!object.isMesh) {
                return;
            }

            const oldMaterials = Array.isArray(object.material)
                ? object.material
                : [object.material];

            oldMaterials.forEach(material => {
                if (material) {
                    originalMaterials.add(material);
                }
            });

            const newMaterials = oldMaterials.map(material => {

                return new THREE.MeshMatcapMaterial({
                    matcap: matcapTexture,
                    color: material?.color
                        ? material.color.clone()
                        : 0x8da9b8
                });

            });

            object.material = Array.isArray(object.material)
                ? newMaterials
                : newMaterials[0];

        });

        // Calculate model dimensions.
        const boundingBox = new THREE.Box3().setFromObject(model);

        if (boundingBox.isEmpty()) {
            throw new Error('The model contains no renderable geometry.');
        }

        const center = boundingBox.getCenter(new THREE.Vector3());
        const dimensions = boundingBox.getSize(new THREE.Vector3());

        const maxDimension = Math.max(
            dimensions.x,
            dimensions.y,
            dimensions.z
        );

        if (!Number.isFinite(maxDimension) || maxDimension <= 0) {
            throw new Error('Invalid model dimensions.');
        }

        // Center model.
        model.position.sub(center);

        // Create orthographic camera.
        const sphereRadius = dimensions.length() * 0.5;
        const frustumSize = sphereRadius * 2 * 1.15;

        const camera = new THREE.OrthographicCamera(
            -frustumSize / 2,
            frustumSize / 2,
            frustumSize / 2,
            -frustumSize / 2,
            Math.max(maxDimension / 1000, 0.0001),
            maxDimension * 100
        );

        // Position camera at an isometric-style angle.
        const direction = new THREE.Vector3(1, 0.7, 1).normalize();
        const distance = sphereRadius * 3;

        camera.position.copy(direction).multiplyScalar(distance);
        camera.lookAt(0, 0, 0);

        // Create renderer.
        renderer = new THREE.WebGLRenderer({
            antialias: true,
            alpha: true,
            preserveDrawingBuffer: true
        });

        // Transparent background.
        renderer.setClearColor(0x000000, 0);
        renderer.setSize(size, size);
        renderer.setPixelRatio(1);

        renderer.outputColorSpace = THREE.SRGBColorSpace;
        renderer.toneMapping = THREE.ACESFilmicToneMapping;

        // Render model.
        renderer.render(scene, camera);

        // Export canvas as PNG.
        const thumbnail = await new Promise((resolve, reject) => {

            renderer.domElement.toBlob(blob => {

                if (!blob) {
                    reject(new Error('Could not generate PNG thumbnail.'));
                    return;
                }

                resolve(blob);

            }, 'image/png');

        });

        return thumbnail;

    } finally {

        // Release renderer resources.
        if (renderer) {
            renderer.dispose();
        }

        // Dispose model geometry and materials.
        if (model) {

            const geometries = new Set();
            const materials = new Set();

            model.traverse(object => {

                if (!object.isMesh) {
                    return;
                }

                if (object.geometry) {
                    geometries.add(object.geometry);
                }

                const objectMaterials = Array.isArray(object.material)
                    ? object.material
                    : [object.material];

                objectMaterials.forEach(material => {
                    if (material) {
                        materials.add(material);
                    }
                });

            });

            geometries.forEach(geometry => geometry.dispose());

            originalMaterials.forEach(material => material.dispose());
            materials.forEach(material => material.dispose());
        }

        // Dispose Matcap texture.
        if (matcapTexture) {
            matcapTexture.dispose();
        }
    }
}

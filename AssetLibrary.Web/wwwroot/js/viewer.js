
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader.js';

let scene;
let camera;
let renderer;
let controls;
let currentModel = null;
let animationId;
let initialCameraPosition;
let isWireframe = false;
let gridHelper;
let isGridVisible = false;

const container = document.getElementById('modelViewerContainer');
const modalElement = document.getElementById('modelViewerModal');

function initialiseViewer() {
    if (renderer) return;

    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x202020);

    // Measurement reference grid
    gridHelper = new THREE.GridHelper(10, 10, 0x888888, 0x444444);
    scene.add(gridHelper);

    camera = new THREE.PerspectiveCamera(
        75,
        container.clientWidth / container.clientHeight,
        0.1,
        1000
    );

    camera.position.set(3, 3, 5);
    initialCameraPosition = new THREE.Vector3(3, 3, 5);

    renderer = new THREE.WebGLRenderer({
        antialias: true
    });

    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(container.clientWidth, container.clientHeight);

    container.appendChild(renderer.domElement);

    // Lighting
    scene.add(new THREE.AmbientLight(0xffffff, 1.5));

    const directionalLight = new THREE.DirectionalLight(0xffffff, 2);
    directionalLight.position.set(5, 5, 5);
    scene.add(directionalLight);

    // // Temporary test cube
    // const geometry = new THREE.BoxGeometry();
    // const material = new THREE.MeshNormalMaterial();
    // const cube = new THREE.Mesh(geometry, material);
    // scene.add(cube);

    // Camera controls
    controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    animate();
}

function animate() {
    animationId = requestAnimationFrame(animate);

    controls.update();
    renderer.render(scene, camera);
}

function resizeViewer() {
    if (!renderer || !camera) return;

    const width = container.clientWidth;
    const height = container.clientHeight;

    camera.aspect = width / height;
    camera.updateProjectionMatrix();

    renderer.setSize(width, height);
}

// Initialise when the Bootstrap modal is fully visible
modalElement.addEventListener('shown.bs.modal', () => {
    initialiseViewer();
    resizeViewer();

    window.dispatchEvent(new Event('viewer-ready'));
});

window.addEventListener('resize', resizeViewer);

// Reset camera view when the reset button is clicked
document.getElementById('resetView').addEventListener('click', () => {
    console.log('Reset View clicked');

    if (!camera || !controls) {
        console.log('Viewer is not initialised yet');
        return;
    }

    camera.position.set(3, 3, 5);
    controls.target.set(0, 0, 0);
    controls.update();
});

// Toggle wireframe mode
document.getElementById('toggleWireframe').addEventListener('click', () => {
    if (!scene) return;

    isWireframe = !isWireframe;

    scene.traverse((object) => {
        if (object.isMesh) {
            const materials = Array.isArray(object.material)
                ? object.material
                : [object.material];

            materials.forEach((material) => {
                material.wireframe = isWireframe;
            });
        }
    });

    document.getElementById('toggleWireframe').textContent =
        isWireframe ? 'Solid View' : 'Wireframe';

});

// Toggle grid visibility
document.getElementById('toggleGrid').addEventListener('click', () => {
    if (!gridHelper) return;

    isGridVisible = !isGridVisible;
    gridHelper.visible = isGridVisible;

    document.getElementById('toggleGrid').textContent =
        isGridVisible ? 'Hide Grid' : 'Show Grid';
});



// // Toggle fullscreen mode
// document.getElementById('viewerFullscreen').addEventListener('click', async () => {
//     const modalDialog = modalElement.querySelector('.modal-dialog');

//     try {
//         if (!document.fullscreenElement) {
//             await modalDialog.requestFullscreen();
//         }
//         else {
//             await document.exitFullscreen();
//         }
//     }
//     catch (error) {
//         console.log('Fullscreen could not be activated:', error);
//     }
// });

// Resize the viewer when entering or exiting fullscreen
document.addEventListener('fullscreenchange', resizeViewer);

// Load a 3D model based on its file extension
function loadModel(url, extension) {
    const fileExtension = extension.toLowerCase().replace('.', '');

    let loader;

    switch (fileExtension) {
        case 'obj':
            loader = new OBJLoader();
            break;

        case 'stl':
            loader = new STLLoader();
            break;

        case 'fbx':
            loader = new FBXLoader();
            break;

        default:
            console.error('Unsupported model format:', extension);
            return;
    }

    loader.load(
        url,
        function (loadedObject) {

            // Remove the previously loaded model.
            if (currentModel) {
                scene.remove(currentModel);
                currentModel = null;
            }

            // STLLoader returns geometry rather than an Object3D.
            if (fileExtension === 'stl') {
                const material = new THREE.MeshStandardMaterial({
                    color: 0xaaaaaa,
                    metalness: 0.2,
                    roughness: 0.7
                });

                currentModel = new THREE.Mesh(loadedObject, material);
            } else {
                currentModel = loadedObject;
            }

            // Add the new model to the scene.
            scene.add(currentModel);

            console.log('3D model loaded successfully:', extension);
        },
        undefined,
        function (error) {
            console.error('Error loading 3D model:', error);
        }
    );
}

// Listen for a selected asset from the Razor view.
window.addEventListener('load-asset-model', async function (event) {
    const assetId = event.detail.assetId;
    const previewUrl = event.detail.previewUrl;

    const statusElement = document.getElementById('viewerStatus');

    try {
        statusElement.textContent = 'Waiting for viewer...';

        // Wait until the Three.js scene is initialized.
        if (!scene) {
            await new Promise(resolve => {
                window.addEventListener('viewer-ready', resolve, { once: true });
            });
        }

        statusElement.textContent = 'Loading model information...';

        const response = await fetch(
            `${previewUrl}?id=${encodeURIComponent(assetId)}`
        );

        if (!response.ok) {
            throw new Error('Could not retrieve model information.');
        }

        const modelInfo = await response.json();

        statusElement.textContent = `Loading ${modelInfo.fileName}...`;

        loadModel(modelInfo.url, modelInfo.extension);

    } catch (error) {
        console.error('Model preview error:', error);
        statusElement.textContent = 'Failed to load model.';
    }
});
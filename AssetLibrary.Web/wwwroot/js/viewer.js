
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader.js';

let scene;
let camera;
let perspectiveCamera;
let orthographicCamera;
let projectionMode = 'perspective';

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
    gridHelper.visible = false;
    scene.add(gridHelper);

    isGridVisible = false;

    const width = Math.max(container.clientWidth, 1);
    const height = Math.max(container.clientHeight, 1);

    perspectiveCamera = new THREE.PerspectiveCamera(
        50,
        width / height,
        0.1,
        1000
    );

    orthographicCamera = new THREE.OrthographicCamera(
        -5,
        5,
        5,
        -5,
        0.1,
        1000
    );

    camera = perspectiveCamera;
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

    // Camera controls
    controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    updateProjectionButton();
    animate();
}

function animate() {
    animationId = requestAnimationFrame(animate);

    controls.update();
    renderer.render(scene, camera);
}

function resizeViewer() {
    if (!renderer || !perspectiveCamera || !orthographicCamera) return;

    const width = Math.max(container.clientWidth, 1);
    const height = Math.max(container.clientHeight, 1);
    const aspect = width / height;

    perspectiveCamera.aspect = aspect;
    perspectiveCamera.updateProjectionMatrix();

    const halfHeight = Math.max(
        Math.abs(orthographicCamera.top),
        Math.abs(orthographicCamera.bottom),
        0.1
    );

    orthographicCamera.left = -halfHeight * aspect;
    orthographicCamera.right = halfHeight * aspect;
    orthographicCamera.top = halfHeight;
    orthographicCamera.bottom = -halfHeight;
    orthographicCamera.updateProjectionMatrix();

    renderer.setSize(width, height);
}

// Fit the orthographic camera around the currently loaded model.
function updateOrthographicFrustum() {
    if (!orthographicCamera) return;

    const aspect = Math.max(container.clientWidth, 1) /
        Math.max(container.clientHeight, 1);

    let halfHeight = 5;

    if (currentModel) {
        const box = new THREE.Box3().setFromObject(currentModel);
        const size = box.getSize(new THREE.Vector3());
        const maxDimension = Math.max(size.x, size.y, size.z);

        if (maxDimension > 0) {
            halfHeight = maxDimension * 0.65;
        }
    }

    orthographicCamera.left = -halfHeight * aspect;
    orthographicCamera.right = halfHeight * aspect;
    orthographicCamera.top = halfHeight;
    orthographicCamera.bottom = -halfHeight;
    orthographicCamera.near = 0.001;
    orthographicCamera.far = 100000;
    orthographicCamera.updateProjectionMatrix();
}

function updateProjectionButton() {
    const button = document.getElementById('toggleProjection');
    if (!button) return;

    // The label indicates the projection the user can switch to.
    button.textContent = projectionMode === 'perspective'
        ? 'Orthographic'
        : 'Perspective';
}

function setProjection(mode) {
    if (!controls || !renderer) return;
    if (mode !== 'perspective' && mode !== 'orthographic') return;
    if (mode === projectionMode) return;

    const target = controls.target.clone();
    const direction = camera.position.clone().sub(target);

    camera = mode === 'orthographic'
        ? orthographicCamera
        : perspectiveCamera;

    camera.position.copy(target).add(direction);
    camera.up.set(0, 1, 0);
    camera.lookAt(target);

    projectionMode = mode;

    if (mode === 'orthographic') {
        updateOrthographicFrustum();
    } else {
        perspectiveCamera.aspect = Math.max(container.clientWidth, 1) /
            Math.max(container.clientHeight, 1);
        perspectiveCamera.updateProjectionMatrix();
    }

    // OrbitControls must be recreated for the newly active camera.
    controls.dispose();
    controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.target.copy(target);
    controls.update();

    updateProjectionButton();
}

// Fit the camera to the model's bounding box
function fitCameraToModel(model) {
    const box = new THREE.Box3().setFromObject(model);
    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxDimension = Math.max(size.x, size.y, size.z);

    if (maxDimension === 0) return;

    const direction = new THREE.Vector3(3, 2, 3).normalize();

    const fitHeightDistance = maxDimension / (
        2 * Math.tan(THREE.MathUtils.degToRad(perspectiveCamera.fov / 2))
    );
    const perspectiveDistance = fitHeightDistance * 1.5;

    const distance = projectionMode === 'orthographic'
        ? maxDimension * 2.5
        : perspectiveDistance;

    camera.position.copy(center).add(
        direction.clone().multiplyScalar(distance)
    );
    camera.lookAt(center);
    controls.target.copy(center);

    perspectiveCamera.near = Math.max(perspectiveDistance / 1000, 0.001);
    perspectiveCamera.far = Math.max(perspectiveDistance * 100, 1000);
    perspectiveCamera.updateProjectionMatrix();

    if (projectionMode === 'orthographic') {
        updateOrthographicFrustum();
    }

    controls.update();
}

// Set the camera view to a specific orientation
function setCameraView(view) {
    if (!currentModel || !camera || !controls) return;

    // Preset views always use orthographic projection.
    setProjection('orthographic');

    // Get the centre and size of the current model.
    const box = new THREE.Box3().setFromObject(currentModel);
    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());

    const maxDimension = Math.max(size.x, size.y, size.z);
    if (maxDimension === 0) return;

    // Calculate a suitable viewing distance.
    const distance = maxDimension * 2.5;

    let direction;

    switch (view) {
        case 'front':
            direction = new THREE.Vector3(0, 0, 1);
            break;

        case 'side':
            direction = new THREE.Vector3(1, 0, 0);
            break;

        case 'top':
            direction = new THREE.Vector3(0, 1, 0);
            break;

        case 'isometric':
            direction = new THREE.Vector3(3, 2, 3).normalize();
            break;

        default:
            return;
    }

    camera.position.copy(center).add(
        direction.multiplyScalar(distance)
    );

    // Avoid the up vector becoming parallel to the viewing direction in Top view.
    camera.up.set(0, 1, 0);
    if (view === 'top') {
        camera.up.set(0, 0, -1);
    }

    controls.target.copy(center);
    camera.lookAt(center);
    controls.update();
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

    setProjection('perspective');

    if (currentModel) {
        fitCameraToModel(currentModel);
    } else {
        camera.position.set(3, 3, 5);
        controls.target.set(0, 0, 0);
        camera.lookAt(0, 0, 0);
        controls.update();
    }
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

// Set camera views
document.getElementById('viewFront').addEventListener('click', () => {
    setCameraView('front');
});

document.getElementById('viewSide').addEventListener('click', () => {
    setCameraView('side');
});

document.getElementById('viewTop').addEventListener('click', () => {
    setCameraView('top');
});

document.getElementById('viewIsometric').addEventListener('click', () => {
    setCameraView('isometric');
});

// Toggle between perspective and orthographic projection.
document.getElementById('toggleProjection')?.addEventListener('click', () => {
    setProjection(
        projectionMode === 'perspective' ? 'orthographic' : 'perspective'
    );
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

// // Resize the viewer when entering or exiting fullscreen
// document.addEventListener('fullscreenchange', resizeViewer);

// Load a 3D model based on its file extension
function loadModel(url, extension) {
    const fileExtension = extension.toLowerCase().replace('.', '');
    const statusElement = document.getElementById('viewerStatus');

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

            if (statusElement) {
                statusElement.textContent = 'Unsupported model format.';
            }
            return;
    }

    // Show loading status.
    if (statusElement) {
        statusElement.textContent = `Loading ${extension.toUpperCase()} model...`;
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

            // Automatically centre the camera and fit the model.
            fitCameraToModel(currentModel);

            console.log('3D model loaded successfully:', extension);

            // Update status.
            if (statusElement) {
                statusElement.textContent = 'Model loaded successfully.';
            }
        },

        // Loading progress.
        function (xhr) {
            if (statusElement && xhr.total > 0) {
                const percentage = Math.round(
                    (xhr.loaded / xhr.total) * 100
                );

                statusElement.textContent =
                    `Loading model... ${percentage}%`;
            }
        },

        // Loading error.
        function (error) {
            console.error('Error loading 3D model:', error);

            if (statusElement) {
                statusElement.textContent =
                    'Failed to load model. Please try again.';
            }
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

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Automatically dismiss notifications, pausing the timer on hover.
document.querySelectorAll('[data-auto-dismiss]').forEach(notification => {
    const delay = Number(notification.dataset.autoDismiss);

    if (!Number.isFinite(delay) || delay <= 0) {
        return;
    }

    let remaining = delay;
    let timeoutId;
    let startTime;

    const progress = notification.querySelector('.notification-progress');

    function startTimer() {
        startTime = Date.now();

        timeoutId = setTimeout(() => {
            bootstrap.Alert.getOrCreateInstance(notification).close();
        }, remaining);

        if (progress) {
            progress.style.animationPlayState = 'running';
        }
    }

    function pauseTimer() {
        clearTimeout(timeoutId);
        remaining -= Date.now() - startTime;

        if (progress) {
            progress.style.animationPlayState = 'paused';
        }
    }

    notification.addEventListener('mouseenter', pauseTimer);

    notification.addEventListener('mouseleave', () => {
        if (remaining > 0 && notification.classList.contains('show')) {
            startTimer();
        }
    });

    startTimer();
});


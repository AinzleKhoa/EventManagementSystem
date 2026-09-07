window.showToast = function (type, message, duration = 2500) {
    if (!message) return;

    const container = document.createElement("div");
    container.className = "toast-container position-fixed top-0 end-0 p-3";
    container.style.zIndex = 1100;

    container.innerHTML = `
        <div class="toast align-items-center text-bg-${type} border-0 overflow-hidden" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button"
                        class="btn-close btn-close-white me-2 m-auto"
                        data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-progress"></div>
        </div>
    `;

    document.body.appendChild(container);

    const toastEl = container.querySelector(".toast");
    const progressEl = container.querySelector(".toast-progress");

    // Style cho thanh progress
    progressEl.style.height = "3px";
    progressEl.style.background = "rgba(255,255,255,0.8)";
    progressEl.style.width = "100%";
    progressEl.style.transition = `width ${duration}ms linear`;

    const toast = new bootstrap.Toast(toastEl, { delay: duration });
    toast.show();

    // Trigger animation
    requestAnimationFrame(() => {
        progressEl.style.width = "0%";
    });

    toastEl.addEventListener("hidden.bs.toast", () => {
        container.remove();
    });
};

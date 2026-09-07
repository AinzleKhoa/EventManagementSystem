(function () {

    if (!document.getElementById("confirm-popup-style")) {
        const style = document.createElement("style");
        style.id = "confirm-popup-style";
        style.innerHTML = `
            #confirm-popup {
                position: fixed;
                inset: 0;
                z-index: 9999;
                font-family: system-ui, -apple-system, BlinkMacSystemFont, sans-serif;
            }

            .confirm-backdrop {
                position: absolute;
                inset: 0;
                background: rgba(0,0,0,0.45);
                backdrop-filter: blur(2px);
            }

            .confirm-modal {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                background: #fff;
                padding: 1.5rem;
                border-radius: 14px;
                width: 380px;
                box-shadow: 0 25px 60px rgba(0,0,0,0.25);
                animation: popupFade 0.2s ease-out;
            }

            .confirm-modal h5 {
                margin: 0 0 0.5rem;
                font-size: 1.1rem;
                font-weight: 600;
            }

            .confirm-modal p {
                margin: 0 0 1.5rem;
                color: #6b7280;
                font-size: 0.95rem;
            }

            .actions {
                display: flex;
                justify-content: flex-end;
                gap: 0.75rem;
            }

            .actions button {
                border: none;
                border-radius: 8px;
                padding: 0.55rem 1.1rem;
                font-weight: 500;
                cursor: pointer;
            }

            .btn-cancel {
                background: #f3f4f6;
                color: #111827;
            }

            .btn-confirm.danger {
                background: #ef4444;
                color: white;
            }

            .btn-confirm.success {
                background: #22c55e;
                color: white;
            }

            .btn-confirm.warning {
                background: #f59e0b;
                color: white;
            }

            .btn-confirm.default {
                background: #6366f1;
                color: white;
            }

            @keyframes popupFade {
                from {
                    opacity: 0;
                    transform: translate(-50%, -55%);
                }
                to {
                    opacity: 1;
                    transform: translate(-50%, -50%);
                }
            }
        `;
        document.head.appendChild(style);
    }

    /**
     * Reusable confirm popup
     */
    window.confirmAction = function ({
        title,
        message,
        url,
        id,
        confirmText = "Confirm",
        type = "default" // danger | success | warning | default
    }) {

        document.getElementById("confirm-popup")?.remove();

        const popup = document.createElement("div");
        popup.id = "confirm-popup";

        popup.innerHTML = `
            <div class="confirm-backdrop"></div>
            <div class="confirm-modal">
                <h5>${title ?? "Confirm action"}</h5>
                <p>${message ?? "Are you sure you want to continue?"}</p>
                <div class="actions">
                    <button class="btn-cancel">Cancel</button>
                    <button class="btn-confirm ${type}">${confirmText}</button>
                </div>
            </div>
        `;

        document.body.appendChild(popup);

        popup.querySelector(".btn-cancel").onclick =
            popup.querySelector(".confirm-backdrop").onclick =
            () => popup.remove();

        popup.querySelector(".btn-confirm").onclick = () => {
            const form = document.createElement("form");
            form.method = "POST";
            form.action = id ? `${url}/${id}` : url;
            document.body.appendChild(form);
            form.submit();
        };
    };

})();

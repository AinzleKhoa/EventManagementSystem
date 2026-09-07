document.addEventListener("DOMContentLoaded", () => {
    const forms = document.querySelectorAll("form");
    if (forms.length === 0) return;

    forms.forEach((form) => {
        form.addEventListener("submit", (e) => {
            // ✅ KIỂM TRA NGAY LẬP TỨC: Nếu validation trước đó đã chặn, thì dừng ngay.
            if (e.defaultPrevented) return;

            if (!form.checkValidity()) return;

            // ⚙️ Chờ 1 tick để browser gom dữ liệu và để các script validation khác (nếu có) chạy trước.
            setTimeout(() => {
                // KIỂM TRA QUAN TRỌNG: Nếu một script khác đã gọi e.preventDefault()
                // (ví dụ: script validation ở trang Checkout), thì không hiển thị loading và không disable form.
                if (e.defaultPrevented) {
                    console.log("Form submission was prevented. Loading overlay will not be shown.");
                    return;
                }

                // Set readOnly for inputs to keep values in post, disable buttons
                const inputs = form.querySelectorAll("input, select, textarea");
                inputs.forEach(el => {
                    el.readOnly = true;
                    el.classList.add("disabled-during-submit");
                });
                const buttons = form.querySelectorAll("button");
                buttons.forEach(el => {
                    el.disabled = true;
                    el.classList.add("disabled-during-submit");
                });

                // ✅ Nếu chưa có overlay thì tạo
                if (!document.querySelector(".loading-overlay")) {
                    const overlay = document.createElement("div");
                    overlay.className = "loading-overlay";
                    overlay.innerHTML = `<p class="loading-text">Please wait...</p>`;
                    document.body.appendChild(overlay);

                    // ✅ CSS: chữ góc phải trên, nền trong suốt
                    const style = document.createElement("style");
                    style.textContent = `
                        .loading-overlay {
                            position: fixed;
                            inset: 0;
                            background: transparent;
                            display: flex;
                            justify-content: flex-end;
                            align-items: flex-start;
                            padding: 20px 30px;
                            z-index: 9999;
                            pointer-events: none; /* để không cản click */
                        }
                        .loading-text {
                            color: white;
                            background: rgba(0,0,0,0.6);
                            padding: 8px 14px;
                            border-radius: 8px;
                            font-size: 0.95rem;
                            font-weight: 500;
                            box-shadow: 0 0 8px rgba(0,0,0,0.2);
                        }
                        .disabled-during-submit {
                            opacity: 0.6;
                            pointer-events: none;
                            cursor: not-allowed;
                        }
                    `;
                    document.head.appendChild(style);
                }

                // ✅ Hiển thị overlay
                document.querySelector(".loading-overlay").style.display = "flex";
            }, 1);
        });
    });
});

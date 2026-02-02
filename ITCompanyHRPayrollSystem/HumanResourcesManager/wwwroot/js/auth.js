// ===== TOGGLE PASSWORD =====
function togglePassword(icon) {
    const input = icon.closest(".position-relative")
        .querySelector(".password-input");

    if (input.type === "password") {
        input.type = "text";
        icon.textContent = "🙈";
    } else {
        input.type = "password";
        icon.textContent = "👁";
    }
}

// ===== FORM LOADING =====
document.addEventListener("DOMContentLoaded", function () {

    const forms = document.querySelectorAll("form");

    forms.forEach(form => {
        form.addEventListener("submit", function (e) {

            // ✅ Nếu form INVALID → không làm gì cả
            if (!form.checkValidity()) {
                return;
            }

            const btn = form.querySelector("button[type='submit']");
            const spinner = btn.querySelector(".spinner-border");
            const text = btn.querySelector(".btn-text");

            if (spinner && text) {
                spinner.classList.remove("d-none");
                text.classList.add("d-none");
            }

            btn.disabled = true;
        });
    });

});


    // ===== DARK MODE =====
    const toggle = document.getElementById("darkModeToggle");

    if (toggle) {
        toggle.checked = localStorage.getItem("darkMode") === "true";

        if (toggle.checked) {
            document.body.classList.add("dark-mode");
        }

        toggle.addEventListener("change", () => {
            document.body.classList.toggle("dark-mode");
            localStorage.setItem("darkMode", toggle.checked);
        });
    }


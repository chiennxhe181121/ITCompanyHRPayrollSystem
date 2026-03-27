// Manager pages
// - Header clock + date
// - Logout behavior

document.addEventListener("DOMContentLoaded", () => {
    // Prevent double-submit (spam click) for all forms
    document.addEventListener("submit", (e) => {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;

        if (form.dataset.submitting === "1") {
            e.preventDefault();
            return;
        }

        form.dataset.submitting = "1";
        form.querySelectorAll('button[type="submit"], input[type="submit"]').forEach((btn) => {
            try { btn.disabled = true; } catch { }
        });
    }, true);

    // Header date + clock
    const dateEl = document.getElementById("currentDate");
    const timeEl = document.getElementById("currentTime");

    function toTitleCaseVietnamese(s) {
        return String(s ?? "")
            .split(" ")
            .filter(Boolean)
            .map(w => w.charAt(0).toUpperCase() + w.slice(1))
            .join(" ");
    }

    function updateClock() {
        const now = new Date();
        if (dateEl) {
            const weekday = toTitleCaseVietnamese(
                now.toLocaleDateString("vi-VN", { weekday: "long" })
            );
            const datePart = now.toLocaleDateString("vi-VN", {
                day: "2-digit",
                month: "2-digit",
                year: "numeric"
            });
            dateEl.textContent = `${weekday}, ${datePart}`;
        }
        if (timeEl) {
            timeEl.textContent = now.toLocaleTimeString("vi-VN", {
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit"
            });
        }
    }

    updateClock();
    setInterval(updateClock, 1000);

    // Logout button hook (chưa gắn backend, chỉ là chỗ để sau này xử lý)
    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            // Hiển thị loading (nếu script loading.js đang dùng)
            if (typeof window.showPageLoading === "function") {
                window.showPageLoading();
            }

            // Gọi thẳng endpoint logout của ASP.NET
            window.location.href = "/HumanResourcesManager/logout";
        });
    }

    initializeProfileActions();
});

let profileEditMode = false;

function initializeProfileActions() {
    const profileForm = document.getElementById("profileForm");
    if (!profileForm) {
        return;
    }

    document.getElementById("profileCancelBtn")?.addEventListener("click", cancelProfileEdit);
    document.getElementById("profileAvatarBtn")?.addEventListener("click", () => {
        document.getElementById("profileAvatarInput")?.click();
    });

    document.getElementById("profileEditSaveBtn")?.addEventListener("click", (e) => {
        e.preventDefault();
        if (!profileEditMode) {
            setProfileEditMode(true);
            return;
        }

        if (!validateProfileForm()) {
            return;
        }

        profileForm.submit();
    });

    document.getElementById("profileAvatarInput")?.addEventListener("change", function (e) {
        const file = e.target.files?.[0];
        if (!file || !file.type.startsWith("image/")) return;

        const imgEl = document.getElementById("profileAvatarImg");
        const initialEl = document.getElementById("profileAvatarInitial");
        const removeFlag = document.getElementById("removeAvatarFlag");
        if (removeFlag) removeFlag.value = "false";

        const previewUrl = URL.createObjectURL(file);
        if (imgEl) {
            imgEl.src = previewUrl;
            imgEl.classList.remove("hidden");
            imgEl.onload = () => URL.revokeObjectURL(previewUrl);
        }
        initialEl?.classList.add("hidden");
    });

    document.getElementById("profileAvatarRemoveBtn")?.addEventListener("click", function () {
        const imgEl = document.getElementById("profileAvatarImg");
        const initialEl = document.getElementById("profileAvatarInitial");
        const fileInput = document.getElementById("profileAvatarInput");
        const removeFlag = document.getElementById("removeAvatarFlag");
        const root = document.getElementById("profileRoot");
        const fullName = root?.dataset.fullname || "M";

        if (fileInput) fileInput.value = "";
        if (removeFlag) removeFlag.value = "true";
        if (imgEl) {
            imgEl.src = "";
            imgEl.classList.add("hidden");
        }
        if (initialEl) {
            initialEl.classList.remove("hidden");
            initialEl.textContent = fullName.charAt(0).toUpperCase();
        }
    });
}

function setProfileEditMode(editing) {
    profileEditMode = editing;
    const editableIds = ["profileFullName", "profileEmail", "profilePhone", "profileDob", "profileAddress"];
    const btn = document.getElementById("profileEditSaveBtn");
    const cancelBtn = document.getElementById("profileCancelBtn");
    const changePasswordBtn = document.getElementById("profileChangePasswordBtn");
    const avatarActions = document.getElementById("profileAvatarActions");
    const gender = document.getElementById("profileGender");

    editableIds.forEach((id) => {
        const el = document.getElementById(id);
        if (!el) return;
        if (editing) {
            el.removeAttribute("readonly");
            el.classList.remove("bg-slate-50", "text-slate-500");
            el.classList.add("text-slate-800");
        } else {
            el.setAttribute("readonly", "readonly");
            el.classList.add("bg-slate-50", "text-slate-500");
            el.classList.remove("text-slate-800");
        }
    });

    if (gender) {
        if (editing) gender.removeAttribute("disabled");
        else gender.setAttribute("disabled", "disabled");
    }

    if (btn) btn.textContent = editing ? "Lưu thay đổi" : "Chỉnh sửa thông tin";
    if (cancelBtn) cancelBtn.classList.toggle("hidden", !editing);
    if (changePasswordBtn) changePasswordBtn.classList.toggle("hidden", editing);
    if (avatarActions) avatarActions.classList.toggle("hidden", !editing);
}

function cancelProfileEdit() {
    document.getElementById("profileForm")?.reset();
    const root = document.getElementById("profileRoot");
    const avatarPath = root?.dataset.avatar;
    const fullName = root?.dataset.fullname || "M";
    const imgEl = document.getElementById("profileAvatarImg");
    const initialEl = document.getElementById("profileAvatarInitial");
    const removeFlag = document.getElementById("removeAvatarFlag");

    if (removeFlag) removeFlag.value = "false";
    if (avatarPath) {
        if (imgEl) {
            imgEl.src = `${avatarPath}?v=${Date.now()}`;
            imgEl.classList.remove("hidden");
        }
        initialEl?.classList.add("hidden");
    } else {
        if (imgEl) {
            imgEl.src = "";
            imgEl.classList.add("hidden");
        }
        if (initialEl) {
            initialEl.classList.remove("hidden");
            initialEl.textContent = fullName.charAt(0).toUpperCase();
        }
    }
    setProfileEditMode(false);
}

function validateProfileForm() {
    const fullName = document.getElementById("profileFullName");
    const email = document.getElementById("profileEmail");
    const phone = document.getElementById("profilePhone");
    const dob = document.getElementById("profileDob");

    if (fullName && !fullName.value.trim()) return false;
    if (email && !/^\S+@\S+\.\S+$/.test(email.value)) return false;
    if (phone && !/^\d{10,11}$/.test(phone.value)) return false;
    if (dob && !dob.value) return false;

    return true;
}

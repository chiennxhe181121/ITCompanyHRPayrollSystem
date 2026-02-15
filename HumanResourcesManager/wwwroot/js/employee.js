// Employee Dashboard - JavaScript giao diện (không dùng getCurrentUser/getDatabase/saveDatabase)
let profileEditMode = false;

const STAT_IDS = ['monthAttendance', 'leavesRemaining', 'overtimeHours', 'currentSalary'];
let statsVisibility = { monthAttendance: true, leavesRemaining: true, overtimeHours: true, currentSalary: true };

const CHECKIN_FROM = "07:30:00";
const CHECKIN_TO = "09:00:00";
const CHECKOUT_FROM = "16:30:00";
const CHECKOUT_TO = "20:00:00";

// ===== STATS =====
function loadStatsVisibility() {
    try {
        const saved = localStorage.getItem('employeeStatsVisibility');
        if (saved) Object.assign(statsVisibility, JSON.parse(saved));
    } catch (e) { }
}

function updateStatsDisplay() {
    STAT_IDS.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        const value = el.dataset.value ?? el.textContent;
        if (el.dataset.value !== undefined) el.dataset.value = value;
        el.textContent = statsVisibility[id] ? value : '•••';
    });
}

function updateStatButtonIcons() {
    document.querySelectorAll('.stat-toggle').forEach(btn => {
        const statId = btn.dataset.stat;
        const visible = statsVisibility[statId];
        const eye = btn.querySelector('.stat-eye');
        const eyeOff = btn.querySelector('.stat-eye-off');
        if (eye) eye.classList.toggle('hidden', !visible);
        if (eyeOff) eyeOff.classList.toggle('hidden', visible);
    });
}

function updateClock() {
    const el = document.getElementById('currentTime');
    if (!el) return;
    const now = new Date();
    el.textContent = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function updateCurrentDate() {
    const el = document.getElementById('currentDate');
    if (!el) return;

    const now = new Date();
    el.textContent = now.toLocaleDateString('vi-VN', {
        weekday: 'long',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}

function loadStatsUI() {
    const monthEl = document.getElementById('monthAttendance');
    if (monthEl) { monthEl.dataset.value = '0'; monthEl.textContent = '0'; }
    const overtimeEl = document.getElementById('overtimeHours');
    if (overtimeEl) { overtimeEl.dataset.value = '0h'; overtimeEl.textContent = '0h'; }
    const salaryEl = document.getElementById('currentSalary');
    if (salaryEl) { salaryEl.dataset.value = '--'; salaryEl.textContent = '--'; }
    const leavesEl = document.getElementById('leavesRemaining');
    if (leavesEl) leavesEl.dataset.value = leavesEl.textContent || '0';
}

// ===== DOM =====
document.addEventListener('DOMContentLoaded', function () {
    const page = document.body.dataset.page;

    switch (page) {
        case "overtime":
            loadOvertimeUI();
            break;
        case "leaves":
            loadLeavesUI();
            break;
        case "payroll":
            loadPayrollUI();
            break;
    }

    loadStatsUI();
    loadStatsVisibility();
    updateStatsDisplay();
    updateStatButtonIcons();
    updateCurrentDate();
    updateClock();
    setInterval(updateClock, 1000);

    // Ràng buộc thời gian chấm công
    setInterval(() => {
        updateCheckInUI();
        updateCheckOutUI();
    }, 1000);

    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function () {
            document.getElementById('logoutForm')?.submit();
        });
    }

    const startCheckIn = document.getElementById('startCheckInCameraBtn');
    const captureCheckIn = document.getElementById('captureCheckInBtn');
    const submitCheckIn = document.getElementById('submitCheckInBtn');
    if (startCheckIn) startCheckIn.addEventListener('click', () => startCamera('checkIn'));
    if (captureCheckIn) captureCheckIn.addEventListener('click', () => capturePhoto('checkIn'));
    if (submitCheckIn) submitCheckIn.addEventListener('click', submitCheckInClick);

    const startCheckOut = document.getElementById('startCheckOutCameraBtn');
    const captureCheckOut = document.getElementById('captureCheckOutBtn');
    const submitCheckOut = document.getElementById('submitCheckOutBtn');
    if (startCheckOut) startCheckOut.addEventListener('click', () => startCamera('checkOut'));
    if (captureCheckOut) captureCheckOut.addEventListener('click', () => capturePhoto('checkOut'));
    if (submitCheckOut) submitCheckOut.addEventListener('click', submitCheckOutClick);

    document.getElementById('profileCancelBtn')
        ?.addEventListener('click', cancelProfileEdit);

    document.getElementById('profileEditSaveBtn')
        ?.addEventListener('click', function (e) {
            e.preventDefault();

            if (!profileEditMode) {
                setProfileEditMode(true);
                return;
            }

            // ===== ĐANG EDIT → VALIDATE =====
            if (!validateProfileForm()) {
                // có lỗi → CHẶN SUBMIT
                return;
            }

            // OK → SUBMIT
            document.getElementById('profileForm').submit();
        });

    // ===== CLEAR ERROR WHEN USER INPUT =====
    ['profileFullName', 'profileEmail', 'profilePhone', 'profileDob'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;

        el.addEventListener('input', () => {
            el.classList.remove('border-red-500');
            el.closest('div')?.querySelector('.field-error')?.remove();
        });
    });

    document.getElementById('addLeaveBtn')?.addEventListener('click', showLeaveModal);

    document.getElementById('profileAvatarBtn')
        ?.addEventListener('click', () =>
            document.getElementById('profileAvatarInput')?.click()
        );

    document.getElementById('profileAvatarInput')
        ?.addEventListener('change', function (e) {

            const file = e.target.files?.[0];
            if (!file || !file.type.startsWith('image/')) return;

            const imgEl = document.getElementById('profileAvatarImg');
            const initialEl = document.getElementById('profileAvatarInitial');
            const removeBtn = document.getElementById('profileAvatarRemoveBtn');
            const removeFlag = document.getElementById('removeAvatarFlag');

            // 🔥 QUAN TRỌNG: nếu chọn ảnh mới thì KHÔNG xóa nữa
            if (removeFlag) removeFlag.value = 'false';

            const previewUrl = URL.createObjectURL(file);

            if (imgEl) {
                imgEl.src = previewUrl;
                imgEl.classList.remove('hidden');
                imgEl.onload = () => URL.revokeObjectURL(previewUrl);
            }

            initialEl?.classList.add('hidden');
            removeBtn?.classList.remove('hidden');
        });

    document.getElementById('profileAvatarRemoveBtn')
        ?.addEventListener('click', function () {

            const imgEl = document.getElementById('profileAvatarImg');
            const initialEl = document.getElementById('profileAvatarInitial');
            const fileInput = document.getElementById('profileAvatarInput');
            const removeFlag = document.getElementById('removeAvatarFlag');

            if (fileInput) fileInput.value = '';
            if (removeFlag) removeFlag.value = 'true';

            if (imgEl) {
                imgEl.src = '';
                imgEl.classList.add('hidden');
            }

            if (initialEl) {
                initialEl.classList.remove('hidden');
                initialEl.textContent =
                    (window.currentEmployee?.fullName ?? 'E').charAt(0).toUpperCase();
            }

            this.classList.add('hidden');
        });

    document.getElementById('overtimeBtnList')?.addEventListener('click', () => switchOvertimeView('available'));
    document.getElementById('overtimeBtnHistory')?.addEventListener('click', () => switchOvertimeView('history'));

    document.querySelectorAll('.stat-toggle').forEach(btn => {
        btn.addEventListener('click', function () {
            const statId = this.dataset.stat;
            if (!statId) return;
            statsVisibility[statId] = !statsVisibility[statId];
            try { localStorage.setItem('employeeStatsVisibility', JSON.stringify(statsVisibility)); } catch (e) { }
            updateStatsDisplay();
            updateStatButtonIcons();
        });
    });
});

// ===== PROFILE =====
function loadProfileAvatarFromDB() {

    const root = document.getElementById("profileRoot");
    if (!root) return;

    const avatarPath = root.dataset.avatar;

    const imgEl = document.getElementById('profileAvatarImg');
    const initialEl = document.getElementById('profileAvatarInitial');
    const removeBtn = document.getElementById('profileAvatarRemoveBtn');
    const removeFlag = document.getElementById('removeAvatarFlag');

    // 🔥 luôn reset flag khi load lại từ DB
    if (removeFlag) removeFlag.value = 'false';

    if (avatarPath) {
        imgEl.src = avatarPath + '?v=' + Date.now();
        imgEl.classList.remove('hidden');
        initialEl?.classList.add('hidden');
        removeBtn?.classList.remove('hidden');
    } else {
        imgEl.src = '';
        imgEl.classList.add('hidden');
        initialEl?.classList.remove('hidden');

        const fullName = root.dataset.fullname ?? 'E';
        initialEl.textContent = fullName.charAt(0).toUpperCase();

        removeBtn?.classList.add('hidden');
    }
}

function setProfileEditMode(editing) {
    profileEditMode = editing;

    const btn = document.getElementById('profileEditSaveBtn');
    const changePasswordBtn = document.getElementById('profileChangePasswordBtn');

    const fullName = document.getElementById('profileFullName');
    const email = document.getElementById('profileEmail');
    const phone = document.getElementById('profilePhone');
    const gender = document.getElementById('profileGender');
    const dob = document.getElementById('profileDob');
    const address = document.getElementById('profileAddress');
    const avatarActions = document.getElementById('profileAvatarActions');

    if (!btn) return;

    if (editing) {

        document.getElementById('profileCancelBtn')?.classList.remove('hidden');
        btn.textContent = 'Lưu thay đổi';

        // 🔥 Ẩn đổi mật khẩu khi đang edit
        if (changePasswordBtn) {
            changePasswordBtn.classList.add('hidden');
        }

        if (fullName) {
            fullName.removeAttribute('readonly');
            fullName.classList.remove('bg-slate-50');
            fullName.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (email) {
            email.removeAttribute('readonly');
            email.classList.remove('bg-slate-50');
            email.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (phone) {
            phone.removeAttribute('readonly');
            phone.classList.remove('bg-slate-50');
            phone.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (gender) {
            gender.removeAttribute('disabled');
            gender.classList.remove('bg-slate-50');
            gender.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (dob) {
            dob.removeAttribute('readonly');
            dob.classList.remove('bg-slate-50');
            dob.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (address) {
            address.removeAttribute('readonly');
            address.classList.remove('bg-slate-50');
            address.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (avatarActions) avatarActions.classList.remove('hidden');

    } else {

        document.getElementById('profileCancelBtn')?.classList.add('hidden');
        btn.textContent = 'Chỉnh sửa thông tin';

        // 🔥 Hiển thị lại đổi mật khẩu
        if (changePasswordBtn) {
            changePasswordBtn.classList.remove('hidden');
        }

        if (fullName) {
            fullName.setAttribute('readonly', 'readonly');
            fullName.classList.add('bg-slate-50');
            fullName.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (email) {
            email.setAttribute('readonly', 'readonly');
            email.classList.add('bg-slate-50');
            email.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (phone) {
            phone.setAttribute('readonly', 'readonly');
            phone.classList.add('bg-slate-50');
            phone.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (gender) {
            gender.setAttribute('disabled', 'disabled');
            gender.classList.add('bg-slate-50');
            gender.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (dob) {
            dob.setAttribute('readonly', 'readonly');
            dob.classList.add('bg-slate-50');
            dob.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (address) {
            address.setAttribute('readonly', 'readonly');
            address.classList.add('bg-slate-50');
            address.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800');
        }

        if (avatarActions) avatarActions.classList.add('hidden');
    }
}

function cancelProfileEdit() {
    document.getElementById('profileForm').reset();
    loadProfileAvatarFromDB();
    setProfileEditMode(false);
}

function normalizeDateForInput(dateStr) {
    if (!dateStr) return '';

    // nếu đã đúng dạng yyyy-mm-dd
    if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) return dateStr;

    const d = new Date(dateStr);
    if (isNaN(d)) return '';

    return d.toISOString().split('T')[0];
}

function validateProfileForm() {
    let isValid = true;

    // ===== CLEAR OLD ERRORS =====
    document.querySelectorAll('.field-error').forEach(e => e.remove());
    document.querySelectorAll('.border-red-500').forEach(e =>
        e.classList.remove('border-red-500')
    );

    function showError(input, message) {
        if (!input) return;

        const err = document.createElement('div');
        err.className = 'field-error text-red-500 text-xs mt-1';
        err.innerText = message;

        input.classList.add('border-red-500');
        input.closest('div')?.appendChild(err);
    }

    // ===== FULL NAME =====
    const fullName = document.getElementById('profileFullName');
    if (fullName && !fullName.hasAttribute('readonly')) {
        if (!fullName.value.trim()) {
            showError(fullName, 'Họ và tên không được để trống');
            isValid = false;
        } else if (fullName.value.length > 100) {
            showError(fullName, 'Họ và tên tối đa 100 ký tự');
            isValid = false;
        }
    }

    // ===== EMAIL =====
    const email = document.getElementById('profileEmail');
    if (email && !email.hasAttribute('readonly')) {
        if (!email.value.trim()) {
            showError(email, 'Email không được để trống');
            isValid = false;
        } else if (!/^\S+@\S+\.\S+$/.test(email.value)) {
            showError(email, 'Email không đúng định dạng');
            isValid = false;
        }
        // ❗ email unique KHÔNG validate được ở FE
    }

    // ===== PHONE =====
    const phone = document.getElementById('profilePhone');
    if (phone && !phone.hasAttribute('readonly')) {
        if (!phone.value.trim()) {
            showError(phone, 'Số điện thoại không được để trống');
            isValid = false;
        } else if (!/^\d{10,11}$/.test(phone.value)) {
            showError(phone, 'Số điện thoại phải 10–11 chữ số');
            isValid = false;
        }
    }

    // ===== DATE OF BIRTH =====
    const dob = document.getElementById('profileDob');
    if (dob && !dob.hasAttribute('readonly')) {
        if (!dob.value) {
            showError(dob, 'Ngày sinh không được để trống');
            isValid = false;
        } else {
            const birth = new Date(dob.value);
            const today = new Date();

            // 1️⃣ future date
            if (birth > today) {
                showError(dob, 'Ngày sinh không được ở tương lai');
                isValid = false;
            } else {
                let age = today.getFullYear() - birth.getFullYear();
                const m = today.getMonth() - birth.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) {
                    age--;
                }

                // 2️⃣ age < 18
                if (age < 18) {
                    showError(dob, 'Nhân viên phải đủ 18 tuổi');
                    isValid = false;
                }

                // 3️⃣ too old
                if (age > 100) {
                    showError(dob, 'Ngày sinh không hợp lệ');
                    isValid = false;
                }
            }
        }
    }

    return isValid;
}

// ===== ATTENDANCE =====
let checkInStream = null;
let checkOutStream = null;
let checkInPhotoData = null;
let checkOutPhotoData = null;

async function startCamera(type) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false });
        if (type === 'checkIn') {
            checkInStream = stream;
            const video = document.getElementById('checkInVideo');
            if (video) { video.srcObject = stream; video.classList.remove('hidden'); }
            document.getElementById('startCheckInCameraBtn')?.classList.add('hidden');
            document.getElementById('captureCheckInBtn')?.classList.remove('hidden');
        } else {
            checkOutStream = stream;
            const video = document.getElementById('checkOutVideo');
            if (video) { video.srcObject = stream; video.classList.remove('hidden'); }
            document.getElementById('startCheckOutCameraBtn')?.classList.add('hidden');
            document.getElementById('captureCheckOutBtn')?.classList.remove('hidden');
        }
    } catch (error) {
        alert('Không thể truy cập camera. Vui lòng cấp quyền camera cho trình duyệt!');
    }
}

function capturePhoto(type) {
    if (type === 'checkIn') {
        const video = document.getElementById('checkInVideo');
        const canvas = document.getElementById('checkInCanvas');
        if (!video || !canvas) return;
        const ctx = canvas.getContext('2d');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        checkInPhotoData = canvas.toDataURL('image/jpeg');
        const previewImg = document.getElementById('checkInPhotoImg');
        const preview = document.getElementById('checkInPhotoPreview');
        if (previewImg) previewImg.src = checkInPhotoData;
        if (preview) preview.classList.remove('hidden');
        video.classList.add('hidden');
        document.getElementById('captureCheckInBtn')?.classList.add('hidden');
        document.getElementById('submitCheckInBtn')?.classList.remove('hidden');
        if (checkInStream) { checkInStream.getTracks().forEach(t => t.stop()); checkInStream = null; }
    } else {
        const video = document.getElementById('checkOutVideo');
        const canvas = document.getElementById('checkOutCanvas');
        if (!video || !canvas) return;
        const ctx = canvas.getContext('2d');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        checkOutPhotoData = canvas.toDataURL('image/jpeg');
        const previewImg = document.getElementById('checkOutPhotoImg');
        const preview = document.getElementById('checkOutPhotoPreview');
        if (previewImg) previewImg.src = checkOutPhotoData;
        if (preview) preview.classList.remove('hidden');
        video.classList.add('hidden');
        document.getElementById('captureCheckOutBtn')?.classList.add('hidden');
        document.getElementById('submitCheckOutBtn')?.classList.remove('hidden');
        if (checkOutStream) { checkOutStream.getTracks().forEach(t => t.stop()); checkOutStream = null; }
    }
}

function submitCheckInClick() {
    if (!checkInPhotoData) {
        alert('Vui lòng chụp ảnh trước khi check-in!');
        return;
    }

    const now = new Date();

    document.getElementById("checkInWorkDate").value =
        now.toISOString().split("T")[0];

    document.getElementById("checkInTime").value =
        now.toTimeString().slice(0, 8);

    // convert base64 -> blob
    const byteString = atob(checkInPhotoData.split(',')[1]);
    const mimeString = checkInPhotoData.split(',')[0].split(':')[1].split(';')[0];

    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);

    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }

    const blob = new Blob([ab], { type: mimeString });
    const file = new File([blob], "check-in.jpg", { type: mimeString });

    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);

    document.getElementById("checkInFileInput").files =
        dataTransfer.files;

    document.getElementById("checkInForm").submit();
}

function submitCheckOutClick() {
    if (!checkOutPhotoData) {
        alert('Vui lòng chụp ảnh trước khi check-out!');
        return;
    }

    const now = new Date();

    document.getElementById("checkOutWorkDate").value =
        now.toISOString().split("T")[0];

    document.getElementById("checkOutTime").value =
        now.toTimeString().slice(0, 8);

    // convert base64 -> blob
    const byteString = atob(checkOutPhotoData.split(',')[1]);
    const mimeString = checkOutPhotoData.split(',')[0].split(':')[1].split(';')[0];

    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);

    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }

    const blob = new Blob([ab], { type: mimeString });
    const file = new File([blob], "check-out.jpg", { type: mimeString });

    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);

    document.getElementById("checkOutFileInput").files =
        dataTransfer.files;

    document.getElementById("checkOutForm").submit();
}

function getTimeDiff(targetTime) {
    const now = new Date();

    const [h, m, s] = targetTime.split(":").map(Number);

    const target = new Date();
    target.setHours(h, m, s, 0);

    const diff = target - now;

    if (diff <= 0) return null;

    const hours = String(Math.floor(diff / 3600000)).padStart(2, '0');
    const minutes = String(Math.floor((diff % 3600000) / 60000)).padStart(2, '0');
    const seconds = String(Math.floor((diff % 60000) / 1000)).padStart(2, '0');

    return `${hours}:${minutes}:${seconds}`;
}

function updateCheckInUI() {
    const btn = document.getElementById("startCheckInCameraBtn");
    const badge = document.getElementById("checkInBadge");
    const countdownEl = document.getElementById("checkInCountdown");
    const progressWrapper = document.getElementById("checkInProgressWrapper");
    const progressBar = document.getElementById("checkInProgress");

    if (!btn || !badge) return;

    if (window.attendanceState?.isLeave || window.attendanceState?.isHoliday) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        if (window.attendanceState?.isHoliday) {
            badge.textContent = "🎉 Nghỉ lễ";
            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-purple-100 text-purple-700";
        } else {
            badge.textContent = "📅 Nghỉ có phép";
            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-700";
        }

        countdownEl?.classList.add("hidden");
        progressWrapper?.classList.add("hidden");

        return;
    }

    if (window.attendanceState?.hasCheckIn) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        badge.textContent =
            "✅ Đã check-in lúc " + window.attendanceState.checkInTime;

        badge.className =
            "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";

        countdownEl?.classList.add("hidden");
        progressWrapper?.classList.add("hidden");

        return;
    }

    const now = new Date();
    const current = now.toTimeString().slice(0, 8);

    if (current < CHECKIN_FROM) {

        const diff = getTimeDiff(CHECKIN_FROM);

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        badge.textContent = "⏳ Chưa tới giờ";
        badge.className = "inline-block px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-700";

        countdownEl.classList.remove("hidden");
        countdownEl.textContent = diff ?? "00:00:00";

        progressWrapper.classList.add("hidden");
    }
    else if (current > CHECKIN_TO) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        countdownEl.classList.add("hidden");
        progressWrapper.classList.add("hidden");

        if (window.attendanceState?.hasCheckIn) {

            badge.textContent =
                "✅ Đã check-in lúc " + window.attendanceState.checkInTime;

            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";
        }
        else {

            badge.textContent = "❌ Đã đóng - Chưa check-in";

            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-700";
        }
    }
    else {

        btn.disabled = false;
        btn.classList.remove("opacity-50", "cursor-not-allowed");

        badge.textContent = "🟢 Đang mở";
        badge.className = "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";

        countdownEl.classList.remove("hidden");

        const diff = getTimeDiff(CHECKIN_TO);
        countdownEl.textContent = diff ?? "00:00:00";

        // progress bar
        const total =
            new Date(`1970-01-01T${CHECKIN_TO}`) -
            new Date(`1970-01-01T${CHECKIN_FROM}`);

        const passed =
            new Date(`1970-01-01T${current}`) -
            new Date(`1970-01-01T${CHECKIN_FROM}`);

        const percent = Math.min(100, Math.max(0, (passed / total) * 100));

        progressWrapper.classList.remove("hidden");
        progressBar.style.width = percent + "%";
    }
}

function updateCheckOutUI() {
    const btn = document.getElementById("startCheckOutCameraBtn");
    const badge = document.getElementById("checkOutBadge");
    const countdownEl = document.getElementById("checkOutCountdown");
    const progressWrapper = document.getElementById("checkOutProgressWrapper");
    const progressBar = document.getElementById("checkOutProgress");

    if (!btn || !badge) return;

    if (window.attendanceState?.isLeave || window.attendanceState?.isHoliday) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        if (window.attendanceState?.isHoliday) {
            badge.textContent = "🎉 Nghỉ lễ";
            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-purple-100 text-purple-700";
        } else {
            badge.textContent = "📅 Nghỉ có phép";
            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-700";
        }

        countdownEl?.classList.add("hidden");
        progressWrapper?.classList.add("hidden");

        return;
    }

    if (window.attendanceState?.hasCheckOut) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        badge.textContent =
            "✅ Đã check-out lúc " + window.attendanceState.checkOutTime;

        badge.className =
            "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";

        countdownEl?.classList.add("hidden");
        progressWrapper?.classList.add("hidden");

        return;
    }

    const now = new Date();
    const current = now.toTimeString().slice(0, 8);

    if (current < CHECKOUT_FROM) {

        const diff = getTimeDiff(CHECKOUT_FROM);

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        badge.textContent = "⏳ Chưa tới giờ";
        badge.className = "inline-block px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-700";

        countdownEl.classList.remove("hidden");
        countdownEl.textContent = diff ?? "00:00:00";

        progressWrapper.classList.add("hidden");
    }
    else if (current > CHECKOUT_TO) {

        btn.disabled = true;
        btn.classList.add("opacity-50", "cursor-not-allowed");

        countdownEl.classList.add("hidden");
        progressWrapper.classList.add("hidden");

        if (window.attendanceState?.hasCheckOut) {

            badge.textContent =
                "✅ Đã check-out lúc " + window.attendanceState.checkOutTime;

            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";
        }
        else if (window.attendanceState?.hasCheckIn) {

            badge.textContent = "⚠ Đã đóng - Chưa check-out";

            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-700";
        }
        else {

            badge.textContent = "❌ Đã đóng - Chưa check-in";

            badge.className =
                "inline-block px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-700";
        }
    }
    else {

        btn.disabled = false;
        btn.classList.remove("opacity-50", "cursor-not-allowed");

        badge.textContent = "🟢 Đang mở";
        badge.className = "inline-block px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-700";

        countdownEl.classList.remove("hidden");

        const diff = getTimeDiff(CHECKOUT_TO);
        countdownEl.textContent = diff ?? "00:00:00";

        // progress bar
        const total =
            new Date(`1970-01-01T${CHECKOUT_TO}`) -
            new Date(`1970-01-01T${CHECKOUT_FROM}`);

        const passed =
            new Date(`1970-01-01T${current}`) -
            new Date(`1970-01-01T${CHECKOUT_FROM}`);

        const percent = Math.min(100, Math.max(0, (passed / total) * 100));

        progressWrapper.classList.remove("hidden");
        progressBar.style.width = percent + "%";
    }
}

// ===== LEAVES =====
function loadLeavesUI() {
    const tbody = document.getElementById('leavesTableBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="6" class="py-8 text-center text-slate-500">Chưa có đơn nghỉ phép</td></tr>';
    document.getElementById('leavesPagination').innerHTML = '';
}

// ===== OVERTIME =====
function switchOvertimeView(view) {
    const btnList = document.getElementById('overtimeBtnList');
    const btnHistory = document.getElementById('overtimeBtnHistory');
    const viewAvailable = document.getElementById('overtimeViewAvailable');
    const viewHistory = document.getElementById('overtimeViewHistory');
    if (!btnList || !btnHistory || !viewAvailable || !viewHistory) return;
    const activeClasses = ['bg-blue-600', 'text-white', 'shadow-sm', 'hover:bg-blue-700'];
    const inactiveClasses = ['bg-white', 'border', 'border-slate-200', 'text-slate-600', 'hover:bg-slate-50', 'hover:border-slate-300', 'hover:text-slate-800'];
    if (view === 'available') {
        viewAvailable.classList.remove('hidden');
        viewHistory.classList.add('hidden');
        btnList.classList.remove(...inactiveClasses);
        btnList.classList.add(...activeClasses);
        btnHistory.classList.remove(...activeClasses);
        btnHistory.classList.add(...inactiveClasses);
    } else {
        viewAvailable.classList.add('hidden');
        viewHistory.classList.remove('hidden');
        btnHistory.classList.remove(...inactiveClasses);
        btnHistory.classList.add(...activeClasses);
        btnList.classList.remove(...activeClasses);
        btnList.classList.add(...inactiveClasses);
    }
}

function loadOvertimeUI() {
    switchOvertimeView('available');
    const availableBody = document.getElementById('overtimeAvailableBody');
    const availableEmpty = document.getElementById('overtimeAvailableEmpty');
    if (availableBody) availableBody.innerHTML = '';
    if (availableEmpty) availableEmpty.classList.remove('hidden');
    document.getElementById('overtimeAvailablePagination').innerHTML = '';

    const tbody = document.getElementById('overtimeTableBody');
    if (tbody) tbody.innerHTML = '<tr><td colspan="5" class="py-8 text-center text-slate-500">Chưa có dữ liệu tăng ca</td></tr>';
    document.getElementById('overtimePagination').innerHTML = '';
}

// ===== PAYROLL =====
function loadPayrollUI() {
    const tbody = document.getElementById('payrollTableBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="7" class="py-8 text-center text-slate-500">Chưa có bảng lương</td></tr>';
    document.getElementById('payrollPagination').innerHTML = '';
}

// ===== SIDEBAR =====
// cần sửa
//function initializePage() {
//    if (typeof window.currentEmployee === "undefined" || !window.currentEmployee)
//        return;

//    const e = window.currentEmployee;

//    // Sidebar name
//    const userNameEl = document.getElementById('userName');
//    if (userNameEl) userNameEl.textContent = e.fullName;

//    // Sidebar position
//    const position = document.getElementById('userPosition');
//    if (position) position.textContent = e.positionName;

//    if (e.imgAvatar) {
//        updateSidebarAvatar(e.imgAvatar);
//    } else {
//        const initial = e.fullName
//            ? e.fullName.charAt(0).toUpperCase()
//            : "?";

//        updateSidebarAvatar(null, initial);
//    }
//}

//function updateSidebarAvatar(avatarDataUrl, initialLetter) {
//    const img = document.getElementById('sidebarAvatarImg');
//    const span = document.getElementById('userInitial');
//    if (!img || !span) return;
//    if (avatarDataUrl) {
//        img.src = avatarDataUrl;
//        img.classList.remove('hidden');
//        span.classList.add('hidden');
//    } else {
//        img.classList.add('hidden');
//        img.src = '';
//        span.textContent = initialLetter || 'E';
//        span.classList.remove('hidden');
//    }
//}
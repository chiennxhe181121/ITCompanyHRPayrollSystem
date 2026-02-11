// Employee Dashboard - JavaScript giao diện (không dùng getCurrentUser/getDatabase/saveDatabase)
let currentPage = { attendance: 1, leaves: 1, overtime: 1, overtimeAvailable: 1, payroll: 1 };
const itemsPerPage = 10;
let profileEditMode = false;
const STAT_IDS = ['monthAttendance', 'leavesRemaining', 'overtimeHours', 'currentSalary'];
let statsVisibility = { monthAttendance: true, leavesRemaining: true, overtimeHours: true, currentSalary: true };

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
        case "attendance":
            loadAttendanceUI();
            break;
        case "profile":
            loadProfileUI();
            break;
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

    // dùng để fallback khi hủy chỉnh sửa profile
    const dobInput = document.getElementById('profileDob');
    if (dobInput) {
        dobInput.dataset.originalValue = dobInput.value;
    }

    loadStatsUI();
    loadStatsVisibility();
    initializePage();
    updateStatsDisplay();
    updateStatButtonIcons();
    updateCurrentDate();
    updateClock();
    setInterval(updateClock, 1000);

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

// ===== SIDEBAR =====
function initializePage() {
    if (!window.currentEmployee) return;

    const e = window.currentEmployee;

    // Sidebar name
    const userNameEl = document.getElementById('userName');
    if (userNameEl) userNameEl.textContent = e.fullName;

    // Sidebar position
    const position = document.getElementById('userPosition');
    if (position) position.textContent = e.positionName;

    // Avatar
    if (e.imgAvatar) {
        updateSidebarAvatar(e.imgAvatar);
    } else {
        updateSidebarAvatar(null, e.fullName.charAt(0).toUpperCase());
    }
}

function updateSidebarAvatar(avatarDataUrl, initialLetter) {
    const img = document.getElementById('sidebarAvatarImg');
    const span = document.getElementById('userInitial');
    if (!img || !span) return;
    if (avatarDataUrl) {
        img.src = avatarDataUrl;
        img.classList.remove('hidden');
        span.classList.add('hidden');
    } else {
        img.classList.add('hidden');
        img.src = '';
        span.textContent = initialLetter || 'E';
        span.classList.remove('hidden');
    }
}

// ===== PROFILE =====
function loadProfileUI() {
    if (!window.currentEmployee) return;

    const e = window.currentEmployee;

    const fullNameInput = document.getElementById('profileFullName');
    const imgEl = document.getElementById('profileAvatarImg');
    const initialEl = document.getElementById('profileAvatarInitial');
    const removeBtn = document.getElementById('profileAvatarRemoveBtn');

    // Full name
    if (fullNameInput && !fullNameInput.value)
        fullNameInput.value = e.fullName ?? '';

    // Avatar render theo DB
    //if (e.imgAvatar) {
    //    imgEl.src = e.imgAvatar + '?v=' + Date.now(); // cache bust
    //    imgEl.classList.remove('hidden');
    //    initialEl.classList.add('hidden');
    //    if (removeBtn) removeBtn.classList.remove('hidden');
    //} else {
    //    imgEl.src = '';
    //    imgEl.classList.add('hidden');
    //    initialEl.classList.remove('hidden');
    //    initialEl.textContent = (e.fullName ?? 'E').charAt(0).toUpperCase();
    //    if (removeBtn) removeBtn.classList.add('hidden');
    //}

    loadProfileAvatarFromDB()

    setProfileEditMode(false);
}

function loadProfileAvatarFromDB() {
    if (!window.currentEmployee) return;

    const avatarPath = window.currentEmployee.imgAvatar;

    const imgEl = document.getElementById('profileAvatarImg');
    const initialEl = document.getElementById('profileAvatarInitial');
    const removeBtn = document.getElementById('profileAvatarRemoveBtn');

    if (avatarPath) {
        imgEl.src = avatarPath + '?v=' + Date.now();
        imgEl.classList.remove('hidden');
        initialEl?.classList.add('hidden');
        removeBtn?.classList.remove('hidden');

        // SYNC SIDEBAR Ở ĐÂY
        updateSidebarAvatar(
            avatarPath + '?v=' + Date.now(),
            window.currentEmployee.fullName?.charAt(0).toUpperCase()
        );
    } else {
        imgEl.src = '';
        imgEl.classList.add('hidden');
        initialEl?.classList.remove('hidden');
        initialEl.textContent =
            window.currentEmployee.fullName?.charAt(0).toUpperCase() ?? 'E';
        removeBtn?.classList.add('hidden');

        updateSidebarAvatar(
            null,
            window.currentEmployee.fullName?.charAt(0).toUpperCase()
        );
    }
}

function setProfileEditMode(editing) {
    profileEditMode = editing;
    const btn = document.getElementById('profileEditSaveBtn');
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
        if (fullName) { fullName.removeAttribute('readonly'); fullName.classList.remove('bg-slate-50'); fullName.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (email) { email.removeAttribute('readonly'); email.classList.remove('bg-slate-50'); email.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (phone) { phone.removeAttribute('readonly'); phone.classList.remove('bg-slate-50'); phone.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (gender) {
            gender.removeAttribute('disabled');
            gender.classList.remove('bg-slate-50');
            gender.classList.add(
                'border-slate-300',
                'focus:ring-2',
                'focus:ring-blue-500/20',
                'focus:border-blue-500', 'text-slate-800'
            );
        }
        if (dob) {
            dob.removeAttribute('readonly');
            dob.classList.remove('bg-slate-50');
            dob.classList.add(
                'border-slate-300',
                'focus:ring-2',
                'focus:ring-blue-500/20',
                'focus:border-blue-500', 'text-slate-800'
            );
        }
        if (address) {
            address.removeAttribute('readonly');
            address.classList.remove('bg-slate-50');
            address.classList.add(
                'border-slate-300',
                'focus:ring-2',
                'focus:ring-blue-500/20',
                'focus:border-blue-500', 'text-slate-800'
            );
        }
        if (avatarActions) avatarActions.classList.remove('hidden');
    } else {
        document.getElementById('profileCancelBtn')?.classList.add('hidden');
        btn.textContent = 'Chỉnh sửa thông tin';
        if (fullName) { fullName.setAttribute('readonly', 'readonly'); fullName.classList.add('bg-slate-50'); fullName.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (email) { email.setAttribute('readonly', 'readonly'); email.classList.add('bg-slate-50'); email.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (phone) { phone.setAttribute('readonly', 'readonly'); phone.classList.add('bg-slate-50'); phone.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (gender) {
            gender.setAttribute('disabled', 'disabled');
            gender.classList.add('bg-slate-50');
            gender.classList.remove(
                'border-slate-300',
                'focus:ring-2',
                'focus:ring-blue-500/20',
                'focus:border-blue-500', 'text-slate-800'
            );
        }
        if (dob) { dob.setAttribute('readonly', 'readonly'); dob.classList.add('bg-slate-50'); dob.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (address) { address.setAttribute('readonly', 'readonly'); address.classList.add('bg-slate-50'); address.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500', 'text-slate-800'); }
        if (avatarActions) avatarActions.classList.add('hidden');
    }
}

function cancelProfileEdit() {
    if (!window.currentEmployee) return;

    const e = window.currentEmployee;

    // reset form fields
    document.getElementById('profileFullName').value = e.fullName ?? '';
    document.getElementById('profileEmail').value = e.email ?? '';
    document.getElementById('profilePhone').value = e.phone ?? '';
    document.getElementById('profileGender').value = e.gender ?? '';
    const dobInput = document.getElementById('profileDob');
    if (dobInput) {
        dobInput.value = dobInput.dataset.originalValue || '';
    }
    document.getElementById('profileAddress').value = e.address ?? '';

    // reset avatar về DB
    loadProfileAvatarFromDB();

    // clear file input + remove flag
    const fileInput = document.getElementById('profileAvatarInput');
    if (fileInput) fileInput.value = '';

    const removeFlag = document.getElementById('removeAvatarFlag');
    if (removeFlag) removeFlag.value = 'false';

    // clear validate errors
    document.querySelectorAll('.field-error').forEach(e => e.remove());
    document.querySelectorAll('.border-red-500').forEach(e => e.classList.remove('border-red-500'));

    // exit edit mode
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

    // ===== AVATAR =====
    const avatarInput = document.getElementById('profileAvatar');
    const removeAvatar = document.getElementById('removeAvatar');

    if (avatarInput && avatarInput.files.length > 0) {
        const file = avatarInput.files[0];

        // size <= 2MB
        if (file.size > 2 * 1024 * 1024) {
            showError(avatarInput, 'Avatar phải nhỏ hơn hoặc bằng 2MB');
            isValid = false;
        }

        // type
        const allowedTypes = ['image/jpeg', 'image/png'];
        if (!allowedTypes.includes(file.type)) {
            showError(avatarInput, 'Avatar chỉ chấp nhận JPG hoặc PNG');
            isValid = false;
        }

        // upload + remove
        if (removeAvatar && removeAvatar.checked) {
            showError(
                avatarInput,
                'Không thể vừa upload vừa xóa avatar'
            );
            isValid = false;
        }
    }

    return isValid;
}

let checkInStream = null;
let checkOutStream = null;
let checkInPhotoData = null;
let checkOutPhotoData = null;

// ===== CAMERA (chỉ giao diện) =====
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
    const currentTime = now.toTimeString().slice(0, 8);
    document.getElementById('checkInPhotoPreview')?.classList.add('hidden');
    document.getElementById('submitCheckInBtn')?.classList.add('hidden');
    document.getElementById('startCheckInCameraBtn')?.classList.remove('hidden');
    checkInPhotoData = null;
    const statusEl = document.getElementById('checkInStatus');
    if (statusEl) statusEl.textContent = 'Đã check-in lúc ' + currentTime + ' (giao diện demo)';
    alert('✓ Check-in (giao diện). Chức năng sẽ kết nối API khi backend sẵn sàng.');
}

function submitCheckOutClick() {
    if (!checkOutPhotoData) {
        alert('Vui lòng chụp ảnh trước khi check-out!');
        return;
    }
    const now = new Date();
    const currentTime = now.toTimeString().slice(0, 8);
    document.getElementById('checkOutPhotoPreview')?.classList.add('hidden');
    document.getElementById('submitCheckOutBtn')?.classList.add('hidden');
    document.getElementById('startCheckOutCameraBtn')?.classList.remove('hidden');
    checkOutPhotoData = null;
    const statusEl = document.getElementById('checkOutStatus');
    if (statusEl) statusEl.textContent = 'Đã check-out lúc ' + currentTime + ' (giao diện demo)';
    alert('✓ Check-out (giao diện). Chức năng sẽ kết nối API khi backend sẵn sàng.');
}

// ===== BẢNG DỮ LIỆU (chỉ giao diện - hiển thị trống) =====
function loadAttendanceUI() {
    const tbody = document.getElementById('attendanceTableBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="6" class="py-8 text-center text-slate-500">Chưa có dữ liệu chấm công</td></tr>';
    document.getElementById('attendancePagination').innerHTML = '';
}

function loadLeavesUI() {
    const tbody = document.getElementById('leavesTableBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="6" class="py-8 text-center text-slate-500">Chưa có đơn nghỉ phép</td></tr>';
    document.getElementById('leavesPagination').innerHTML = '';
}

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

function loadPayrollUI() {
    const tbody = document.getElementById('payrollTableBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="7" class="py-8 text-center text-slate-500">Chưa có bảng lương</td></tr>';
    document.getElementById('payrollPagination').innerHTML = '';
}

// ===== MODAL NGHỈ PHÉP (chỉ giao diện) =====
function showLeaveModal() {
    const modal = `
    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-xl font-bold text-gray-900">Đăng Ký Nghỉ Phép</h3>
          <button type="button" onclick="closeModal()" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>
        <form id="leaveForm" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Loại Phép</label>
            <select id="leaveType" class="w-full px-4 py-2 border border-gray-300 rounded-lg" required>
              <option value="Annual Leave">Phép năm</option>
              <option value="Sick Leave">Nghỉ ốm</option>
              <option value="Personal Leave">Nghỉ cá nhân</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Từ Ngày</label>
            <input type="date" id="startDate" class="w-full px-4 py-2 border border-gray-300 rounded-lg" required>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Đến Ngày</label>
            <input type="date" id="endDate" class="w-full px-4 py-2 border border-gray-300 rounded-lg" required>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Lý Do</label>
            <textarea id="reason" rows="3" class="w-full px-4 py-2 border border-gray-300 rounded-lg" required></textarea>
          </div>
          <div class="flex space-x-3 pt-4">
            <button type="submit" class="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg">Gửi</button>
            <button type="button" onclick="closeModal()" class="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-2 rounded-lg">Hủy</button>
          </div>
        </form>
      </div>
    </div>
  `;
    const container = document.getElementById('modalContainer');
    if (container) container.innerHTML = modal;
    document.getElementById('leaveForm')?.addEventListener('submit', function (e) {
        e.preventDefault();
        closeModal();
        alert('Chức năng gửi đơn nghỉ phép sẽ kết nối API khi backend sẵn sàng.');
    });
}

function closeModal() {
    const container = document.getElementById('modalContainer');
    if (container) container.innerHTML = '';
}

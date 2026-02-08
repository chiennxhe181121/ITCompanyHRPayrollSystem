// Employee Dashboard - JavaScript giao diện (không dùng getCurrentUser/getDatabase/saveDatabase)
let currentTab = 'attendance';
let currentPage = { attendance: 1, leaves: 1, overtime: 1, overtimeAvailable: 1, payroll: 1 };
const itemsPerPage = 10;
let profileEditMode = false;

const STAT_IDS = ['monthAttendance', 'leavesRemaining', 'overtimeHours', 'currentSalary'];
let statsVisibility = { monthAttendance: true, leavesRemaining: true, overtimeHours: true, currentSalary: true };

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

let checkInStream = null;
let checkOutStream = null;
let checkInPhotoData = null;
let checkOutPhotoData = null;

document.addEventListener('DOMContentLoaded', function () {
    loadStatsVisibility();
    initializePage();
    loadStatsUI();
    updateStatsDisplay();
    updateStatButtonIcons();
    loadTabContent(currentTab);
    updateClock();
    setInterval(updateClock, 1000);

    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            switchTab(this.dataset.tab);
        });
    });

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

    document.getElementById('addLeaveBtn')?.addEventListener('click', showLeaveModal);

    document.getElementById('profileEditSaveBtn')?.addEventListener('click', function () {
        if (profileEditMode) {
            saveProfileUI();
            setProfileEditMode(false);
        } else {
            setProfileEditMode(true);
        }
    });

    document.getElementById('profileAvatarBtn')?.addEventListener('click', () => document.getElementById('profileAvatarInput')?.click());
    document.getElementById('profileAvatarInput')?.addEventListener('change', function (e) {
        const file = e.target.files?.[0];
        if (!file || !file.type.startsWith('image/')) return;
        const reader = new FileReader();
        reader.onload = function () {
            const dataEl = document.getElementById('profileAvatarData');
            const imgEl = document.getElementById('profileAvatarImg');
            const initialEl = document.getElementById('profileAvatarInitial');
            const removeBtn = document.getElementById('profileAvatarRemoveBtn');
            if (dataEl) dataEl.value = reader.result;
            if (imgEl) { imgEl.src = reader.result; imgEl.classList.remove('hidden'); }
            if (initialEl) initialEl.classList.add('hidden');
            if (removeBtn) removeBtn.classList.remove('hidden');
        };
        reader.readAsDataURL(file);
        e.target.value = '';
    });
    document.getElementById('profileAvatarRemoveBtn')?.addEventListener('click', function () {
        const dataEl = document.getElementById('profileAvatarData');
        const imgEl = document.getElementById('profileAvatarImg');
        const initialEl = document.getElementById('profileAvatarInitial');
        if (dataEl) dataEl.value = 'REMOVE';
        if (imgEl) { imgEl.classList.add('hidden'); imgEl.src = ''; }
        if (initialEl) {
            initialEl.classList.remove('hidden');
            const userName = document.getElementById('userName');
            initialEl.textContent = (userName && userName.textContent) ? userName.textContent.charAt(0).toUpperCase() : 'E';
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

function updateClock() {
    const el = document.getElementById('currentTime');
    if (!el) return;
    const now = new Date();
    el.textContent = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
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

function switchTab(tab) {
    currentTab = tab;
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
        if (item.dataset.tab === tab) item.classList.add('active');
    });
    document.querySelectorAll('.tab-content').forEach(content => content.classList.add('hidden'));
    const tabMap = { profile: 'profileTab', attendance: 'attendanceTab', leaves: 'leavesTab', overtime: 'overtimeTab', payroll: 'payrollTab' };
    const tabEl = document.getElementById(tabMap[tab]);
    if (tabEl) tabEl.classList.remove('hidden');
    loadTabContent(tab);
}

function loadTabContent(tab) {
    switch (tab) {
        case 'profile': loadProfileUI(); break;
        case 'attendance': loadAttendanceUI(); break;
        case 'leaves': loadLeavesUI(); break;
        case 'overtime': loadOvertimeUI(); break;
        case 'payroll': loadPayrollUI(); break;
    }
}

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

// ===== PROFILE (chỉ giao diện) =====
function loadProfileUI() {
    const fullName = document.getElementById('profileFullName');
    const initialEl = document.getElementById('profileAvatarInitial');
    const userName = document.getElementById('userName');
    if (fullName && !fullName.value && userName) fullName.value = userName.textContent.trim() || '';
    if (initialEl && userName) initialEl.textContent = (userName.textContent || 'E').trim().charAt(0).toUpperCase();
    setProfileEditMode(false);
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
        btn.textContent = 'Lưu thay đổi';
        if (fullName) { fullName.removeAttribute('readonly'); fullName.classList.remove('bg-slate-50'); fullName.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (email) { email.removeAttribute('readonly'); email.classList.remove('bg-slate-50'); email.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (phone) { phone.removeAttribute('readonly'); phone.classList.remove('bg-slate-50'); phone.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (gender) { fullName.removeAttribute('readonly'); gender.classList.remove('bg-slate-50'); gender.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (dob) { email.removeAttribute('readonly'); dob.classList.remove('bg-slate-50'); dob.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (address) { phone.removeAttribute('readonly'); address.classList.remove('bg-slate-50'); address.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (avatarActions) avatarActions.classList.remove('hidden');
    } else {
        btn.textContent = 'Chỉnh sửa thông tin';
        if (fullName) { fullName.setAttribute('readonly', 'readonly'); fullName.classList.add('bg-slate-50'); fullName.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (email) { email.setAttribute('readonly', 'readonly'); email.classList.add('bg-slate-50'); email.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (phone) { phone.setAttribute('readonly', 'readonly'); phone.classList.add('bg-slate-50'); phone.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (gender) { gender.setAttribute('readonly', 'readonly'); gender.classList.add('bg-slate-50'); gender.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (dob) { dob.setAttribute('readonly', 'readonly'); dob.classList.add('bg-slate-50'); dob.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (address) { address.setAttribute('readonly', 'readonly'); address.classList.add('bg-slate-50'); address.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500'); }
        if (avatarActions) avatarActions.classList.add('hidden');
    }
}

function saveProfileUI() {
    alert('Chức năng lưu hồ sơ sẽ kết nối API khi backend sẵn sàng.');
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

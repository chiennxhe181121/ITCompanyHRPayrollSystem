// Employee Dashboard JavaScript - Chỉ giao diện (UI)
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
  } catch (e) {}
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

document.addEventListener('DOMContentLoaded', function() {
  loadStatsVisibility();
  initializePageUI();
  updateStatsDisplay();
  updateStatButtonIcons();
  loadTabContentUI(currentTab);
  updateClock();
  setInterval(updateClock, 1000);

  // Navigation
  document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', function(e) {
      e.preventDefault();
      switchTab(this.dataset.tab);
    });
  });

  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) logoutBtn.addEventListener('click', function() { /* logout do backend/xử lý riêng */ });

  // Camera Buttons
  const startCheckInCameraBtn = document.getElementById('startCheckInCameraBtn');
  const captureCheckInBtn = document.getElementById('captureCheckInBtn');
  const submitCheckInBtn = document.getElementById('submitCheckInBtn');
  if (startCheckInCameraBtn) startCheckInCameraBtn.addEventListener('click', () => startCamera('checkIn'));
  if (captureCheckInBtn) captureCheckInBtn.addEventListener('click', () => capturePhoto('checkIn'));
  if (submitCheckInBtn) submitCheckInBtn.addEventListener('click', () => submitCheckInUI());

  const startCheckOutCameraBtn = document.getElementById('startCheckOutCameraBtn');
  const captureCheckOutBtn = document.getElementById('captureCheckOutBtn');
  const submitCheckOutBtn = document.getElementById('submitCheckOutBtn');
  if (startCheckOutCameraBtn) startCheckOutCameraBtn.addEventListener('click', () => startCamera('checkOut'));
  if (captureCheckOutBtn) captureCheckOutBtn.addEventListener('click', () => capturePhoto('checkOut'));
  if (submitCheckOutBtn) submitCheckOutBtn.addEventListener('click', () => submitCheckOutUI());

  // Other buttons
  const addLeaveBtn = document.getElementById('addLeaveBtn');
  if (addLeaveBtn) addLeaveBtn.addEventListener('click', () => showLeaveModal());

  const profileEditSaveBtn = document.getElementById('profileEditSaveBtn');
  if (profileEditSaveBtn) {
    profileEditSaveBtn.addEventListener('click', function() {
      if (profileEditMode) {
        saveProfileUI();
        setProfileEditMode(false);
      } else {
        setProfileEditMode(true);
      }
    });
  }

  const profileAvatarBtn = document.getElementById('profileAvatarBtn');
  const profileAvatarInput = document.getElementById('profileAvatarInput');
  if (profileAvatarBtn) profileAvatarBtn.addEventListener('click', () => document.getElementById('profileAvatarInput')?.click());
  if (profileAvatarInput) {
    profileAvatarInput.addEventListener('change', function(e) {
      const file = e.target.files?.[0];
      if (!file || !file.type.startsWith('image/')) return;
      const reader = new FileReader();
      reader.onload = function() {
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
  }

  const profileAvatarRemoveBtn = document.getElementById('profileAvatarRemoveBtn');
  if (profileAvatarRemoveBtn) {
    profileAvatarRemoveBtn.addEventListener('click', function() {
      const dataEl = document.getElementById('profileAvatarData');
      const imgEl = document.getElementById('profileAvatarImg');
      const initialEl = document.getElementById('profileAvatarInitial');
      if (dataEl) dataEl.value = 'REMOVE';
      if (imgEl) { imgEl.classList.add('hidden'); imgEl.src = ''; }
      if (initialEl) {
        initialEl.classList.remove('hidden');
        initialEl.textContent = initialEl.textContent || 'E';
      }
      this.classList.add('hidden');
    });
  }

  const overtimeBtnList = document.getElementById('overtimeBtnList');
  const overtimeBtnHistory = document.getElementById('overtimeBtnHistory');
  if (overtimeBtnList) overtimeBtnList.addEventListener('click', () => switchOvertimeView('available'));
  if (overtimeBtnHistory) overtimeBtnHistory.addEventListener('click', () => switchOvertimeView('history'));

  document.querySelectorAll('.stat-toggle').forEach(btn => {
    btn.addEventListener('click', function() {
      const statId = this.dataset.stat;
      if (!statId) return;
      statsVisibility[statId] = !statsVisibility[statId];
      try { localStorage.setItem('employeeStatsVisibility', JSON.stringify(statsVisibility)); } catch (e) {}
      updateStatsDisplay();
      updateStatButtonIcons();
    });
  });
});

function initializePageUI() {
  const userNameEl = document.getElementById('userName');
  if (userNameEl && !userNameEl.textContent) userNameEl.textContent = 'Nhân viên';

  const currentDateEl = document.getElementById('currentDate');
  if (currentDateEl) {
    const today = new Date();
    currentDateEl.textContent = today.toLocaleDateString('vi-VN', {
      year: 'numeric', month: 'long', day: 'numeric'
    });
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
  const now = new Date();
  const timeString = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  const el = document.getElementById('currentTime');
  if (el) el.textContent = timeString;
}

function switchTab(tab) {
  currentTab = tab;

  document.querySelectorAll('.nav-item').forEach(item => {
    item.classList.remove('active');
    if (item.dataset.tab === tab) item.classList.add('active');
  });

  document.querySelectorAll('.tab-content').forEach(content => {
    content.classList.add('hidden');
  });

  const tabMap = {
    'profile': 'profileTab',
    'attendance': 'attendanceTab',
    'leaves': 'leavesTab',
    'overtime': 'overtimeTab',
    'payroll': 'payrollTab'
  };
  const tabEl = document.getElementById(tabMap[tab]);
  if (tabEl) tabEl.classList.remove('hidden');

  loadTabContentUI(tab);
}

function loadTabContentUI(tab) {
  switch (tab) {
    case 'profile': loadProfileUI(); break;
    case 'attendance': loadAttendanceUI(); break;
    case 'leaves': loadLeavesUI(); break;
    case 'overtime': loadOvertimeUI(); break;
    case 'payroll': loadPayrollUI(); break;
  }
}

// ===== CAMERA (UI) =====
async function startCamera(type) {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'user' },
      audio: false
    });

    if (type === 'checkIn') {
      checkInStream = stream;
      const video = document.getElementById('checkInVideo');
      if (video) { video.srcObject = stream; video.classList.remove('hidden'); }
      const startBtn = document.getElementById('startCheckInCameraBtn');
      const captureBtn = document.getElementById('captureCheckInBtn');
      if (startBtn) startBtn.classList.add('hidden');
      if (captureBtn) captureBtn.classList.remove('hidden');
    } else {
      checkOutStream = stream;
      const video = document.getElementById('checkOutVideo');
      if (video) { video.srcObject = stream; video.classList.remove('hidden'); }
      const startBtn = document.getElementById('startCheckOutCameraBtn');
      const captureBtn = document.getElementById('captureCheckOutBtn');
      if (startBtn) startBtn.classList.add('hidden');
      if (captureBtn) captureBtn.classList.remove('hidden');
    }
  } catch (error) {
    alert('Không thể truy cập camera. Vui lòng cấp quyền camera cho trình duyệt!');
    console.error('Camera error:', error);
  }
}

function capturePhoto(type) {
  if (type === 'checkIn') {
    const video = document.getElementById('checkInVideo');
    const canvas = document.getElementById('checkInCanvas');
    if (!video || !canvas) return;
    const context = canvas.getContext('2d');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    checkInPhotoData = canvas.toDataURL('image/jpeg');

    const photoImg = document.getElementById('checkInPhotoImg');
    const photoPreview = document.getElementById('checkInPhotoPreview');
    const captureBtn = document.getElementById('captureCheckInBtn');
    const submitBtn = document.getElementById('submitCheckInBtn');
    if (photoImg) photoImg.src = checkInPhotoData;
    if (photoPreview) photoPreview.classList.remove('hidden');
    if (video) video.classList.add('hidden');
    if (captureBtn) captureBtn.classList.add('hidden');
    if (submitBtn) submitBtn.classList.remove('hidden');

    if (checkInStream) {
      checkInStream.getTracks().forEach(track => track.stop());
      checkInStream = null;
    }
  } else {
    const video = document.getElementById('checkOutVideo');
    const canvas = document.getElementById('checkOutCanvas');
    if (!video || !canvas) return;
    const context = canvas.getContext('2d');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    checkOutPhotoData = canvas.toDataURL('image/jpeg');

    const photoImg = document.getElementById('checkOutPhotoImg');
    const photoPreview = document.getElementById('checkOutPhotoPreview');
    const captureBtn = document.getElementById('captureCheckOutBtn');
    const submitBtn = document.getElementById('submitCheckOutBtn');
    if (photoImg) photoImg.src = checkOutPhotoData;
    if (photoPreview) photoPreview.classList.remove('hidden');
    if (video) video.classList.add('hidden');
    if (captureBtn) captureBtn.classList.add('hidden');
    if (submitBtn) submitBtn.classList.remove('hidden');

    if (checkOutStream) {
      checkOutStream.getTracks().forEach(track => track.stop());
      checkOutStream = null;
    }
  }
}

function submitCheckInUI() {
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
  if (statusEl) statusEl.textContent = 'Đã check-in lúc ' + currentTime;
  alert('✓ Check-in thành công!');
}

function submitCheckOutUI() {
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
  if (statusEl) statusEl.textContent = 'Đã check-out lúc ' + currentTime;
  alert('✓ Check-out thành công!');
}

// ===== PROFILE (UI) =====
function loadProfileUI() {
  setProfileEditMode(false);
}

function setProfileEditMode(editing) {
  profileEditMode = editing;
  const btn = document.getElementById('profileEditSaveBtn');
  const fullName = document.getElementById('profileFullName');
  const email = document.getElementById('profileEmail');
  const phone = document.getElementById('profilePhone');
  const avatarActions = document.getElementById('profileAvatarActions');

  if (editing) {
    if (btn) btn.textContent = 'Lưu thay đổi';
    [fullName, email, phone].forEach(el => {
      if (!el) return;
      el.removeAttribute('readonly');
      el.classList.remove('bg-slate-50');
      el.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    });
    if (avatarActions) avatarActions.classList.remove('hidden');
  } else {
    if (btn) btn.textContent = 'Chỉnh sửa thông tin';
    [fullName, email, phone].forEach(el => {
      if (!el) return;
      el.setAttribute('readonly', 'readonly');
      el.classList.add('bg-slate-50');
      el.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    });
    if (avatarActions) avatarActions.classList.add('hidden');
  }
}

function saveProfileUI() {
  setProfileEditMode(false);
  alert('Đã lưu thông tin cá nhân.');
}

// ===== LOAD DATA (UI - placeholder empty) =====
function loadAttendanceUI() {
  const tbody = document.getElementById('attendanceTableBody');
  if (tbody) tbody.innerHTML = '<tr><td colspan="6" class="py-8 text-center text-gray-500">Chưa có dữ liệu</td></tr>';
  const pagination = document.getElementById('attendancePagination');
  if (pagination) pagination.innerHTML = '';
}

function loadLeavesUI() {
  const tbody = document.getElementById('leavesTableBody');
  if (tbody) tbody.innerHTML = '<tr><td colspan="6" class="py-8 text-center text-gray-500">Chưa có dữ liệu</td></tr>';
  const pagination = document.getElementById('leavesPagination');
  if (pagination) pagination.innerHTML = '';
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
  if (availableBody) availableBody.innerHTML = '<tr><td colspan="4" class="py-6 text-center text-gray-500">Chưa có lịch tăng ca</td></tr>';
  if (availableEmpty) availableEmpty.classList.add('hidden');
  const overtimePagination = document.getElementById('overtimeAvailablePagination');
  if (overtimePagination) overtimePagination.innerHTML = '';

  const tbody = document.getElementById('overtimeTableBody');
  if (tbody) tbody.innerHTML = '<tr><td colspan="5" class="py-8 text-center text-gray-500">Chưa có dữ liệu</td></tr>';
  const pagination = document.getElementById('overtimePagination');
  if (pagination) pagination.innerHTML = '';
}

function loadPayrollUI() {
  const tbody = document.getElementById('payrollTableBody');
  if (tbody) tbody.innerHTML = '<tr><td colspan="7" class="py-8 text-center text-gray-500">Chưa có dữ liệu</td></tr>';
  const pagination = document.getElementById('payrollPagination');
  if (pagination) pagination.innerHTML = '';
}

// ===== MODALS (UI) =====
function showLeaveModal() {
  const modal = `
    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-xl font-bold text-gray-900">Đăng Ký Nghỉ Phép</h3>
          <button onclick="closeModal()" class="text-gray-400 hover:text-gray-600">
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

  const form = document.getElementById('leaveForm');
  if (form) {
    form.addEventListener('submit', function(e) {
      e.preventDefault();
      closeModal();
      alert('Đã gửi đơn nghỉ phép! (Xử lý gửi server ở logic riêng)');
    });
  }
}

function closeModal() {
  const container = document.getElementById('modalContainer');
  if (container) container.innerHTML = '';
}

function renderPagination(containerId, totalPages, currentPageNum, onPageChange) {
  const container = document.getElementById(containerId);
  if (!container) return;
  if (totalPages <= 1) {
    container.innerHTML = '';
    return;
  }

  let html = '<div class="employee-pagination flex flex-wrap items-center justify-center gap-2 py-4">';
  html += '<span class="text-sm text-slate-500 mr-2">Trang</span>';
  html += `<button type="button" ${currentPageNum === 1 ? 'disabled' : ''} class="employee-pagination-btn employee-pagination-prev px-3 py-2 text-sm font-medium rounded-xl border border-slate-200 bg-white text-slate-600 hover:bg-slate-50 hover:border-slate-300 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-white disabled:hover:border-slate-200 transition-colors" onclick="changePage(${currentPageNum - 1}, '${containerId}')">‹ Trước</button>`;
  html += '<div class="flex items-center gap-1">';

  for (let i = 1; i <= totalPages; i++) {
    if (i === 1 || i === totalPages || (i >= currentPageNum - 1 && i <= currentPageNum + 1)) {
      const active = i === currentPageNum;
      html += `<button type="button" class="employee-pagination-num min-w-[2.25rem] px-3 py-2 text-sm font-medium rounded-xl border transition-colors ${active ? 'bg-blue-600 border-blue-600 text-white' : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50 hover:border-slate-300'}" onclick="changePage(${i}, '${containerId}')">${i}</button>`;
    } else if (i === currentPageNum - 2 || i === currentPageNum + 2) {
      html += '<span class="px-2 text-slate-400">…</span>';
    }
  }

  html += '</div>';
  html += `<button type="button" ${currentPageNum === totalPages ? 'disabled' : ''} class="employee-pagination-btn employee-pagination-next px-3 py-2 text-sm font-medium rounded-xl border border-slate-200 bg-white text-slate-600 hover:bg-slate-50 hover:border-slate-300 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-white disabled:hover:border-slate-200 transition-colors" onclick="changePage(${currentPageNum + 1}, '${containerId}')">Sau ›</button>`;
  html += `<span class="text-sm text-slate-500 ml-2">${currentPageNum} / ${totalPages}</span>`;
  html += '</div>';
  container.innerHTML = html;

  window[`pagination_${containerId}`] = onPageChange;
}

function changePage(page, containerId) {
  const callback = window[`pagination_${containerId}`];
  if (callback) callback(page);
}

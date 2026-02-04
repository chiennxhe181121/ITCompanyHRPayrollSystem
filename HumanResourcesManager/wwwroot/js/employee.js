// Employee Dashboard JavaScript
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
  const currentUser = getCurrentUser();
  if (!currentUser || currentUser.role !== 'employee') {
    alert('Bạn không có quyền truy cập trang này!');
    if (typeof showPageLoading === 'function') showPageLoading();
    setTimeout(function() { window.location.href = 'index.html'; }, 1000);
    return;
  }

  loadStatsVisibility();
  initializePage(currentUser);
  loadStats(currentUser);
  updateStatsDisplay();
  updateStatButtonIcons();
  loadTabContent('attendance', currentUser);
  updateClock();
  setInterval(updateClock, 1000);

  // Navigation
  document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', function(e) {
      e.preventDefault();
      switchTab(this.dataset.tab, currentUser);
    });
  });

  document.getElementById('logoutBtn').addEventListener('click', logout);

  // Camera Buttons
  document.getElementById('startCheckInCameraBtn').addEventListener('click', () => startCamera('checkIn'));
  document.getElementById('captureCheckInBtn').addEventListener('click', () => capturePhoto('checkIn'));
  document.getElementById('submitCheckInBtn').addEventListener('click', () => submitCheckIn(currentUser));

  document.getElementById('startCheckOutCameraBtn').addEventListener('click', () => startCamera('checkOut'));
  document.getElementById('captureCheckOutBtn').addEventListener('click', () => capturePhoto('checkOut'));
  document.getElementById('submitCheckOutBtn').addEventListener('click', () => submitCheckOut(currentUser));

  // Other buttons
  document.getElementById('addLeaveBtn')?.addEventListener('click', () => showLeaveModal(currentUser));

  document.getElementById('profileEditSaveBtn')?.addEventListener('click', function() {
    const user = getCurrentUser();
    if (!user) return;
    if (profileEditMode) {
      saveProfile(user);
      setProfileEditMode(false);
    } else {
      setProfileEditMode(true);
    }
  });

  document.getElementById('profileAvatarBtn')?.addEventListener('click', () => document.getElementById('profileAvatarInput').click());
  document.getElementById('profileAvatarInput')?.addEventListener('change', function(e) {
    const file = e.target.files?.[0];
    if (!file || !file.type.startsWith('image/')) return;
    const reader = new FileReader();
    reader.onload = function() {
      document.getElementById('profileAvatarData').value = reader.result;
      document.getElementById('profileAvatarImg').src = reader.result;
      document.getElementById('profileAvatarImg').classList.remove('hidden');
      document.getElementById('profileAvatarInitial').classList.add('hidden');
      document.getElementById('profileAvatarRemoveBtn').classList.remove('hidden');
    };
    reader.readAsDataURL(file);
    e.target.value = '';
    document.getElementById('profileAvatarRemoveBtn').classList.remove('hidden');
  });
  document.getElementById('profileAvatarRemoveBtn')?.addEventListener('click', function() {
    document.getElementById('profileAvatarData').value = 'REMOVE';
    document.getElementById('profileAvatarImg').classList.add('hidden');
    document.getElementById('profileAvatarImg').src = '';
    document.getElementById('profileAvatarInitial').classList.remove('hidden');
    const user = getCurrentUser();
    document.getElementById('profileAvatarInitial').textContent = (user && user.name) ? user.name.charAt(0).toUpperCase() : 'E';
    this.classList.add('hidden');
  });

  document.getElementById('overtimeBtnList')?.addEventListener('click', () => switchOvertimeView('available'));
  document.getElementById('overtimeBtnHistory')?.addEventListener('click', () => switchOvertimeView('history'));

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

  checkTodayAttendance(currentUser);
});

function initializePage(user) {
  document.getElementById('userName').textContent = user.name;
  const initial = user.name ? user.name.charAt(0).toUpperCase() : 'E';
  const db = getDatabase();
  const employee = db.employees.find(e => e.id === user.employeeId);
  updateSidebarAvatar(employee && employee.avatar ? employee.avatar : null, initial);

  const today = new Date();
  document.getElementById('currentDate').textContent = today.toLocaleDateString('vi-VN', { 
    year: 'numeric', month: 'long', day: 'numeric' 
  });
}

function updateSidebarAvatar(avatarDataUrl, initialLetter) {
  const img = document.getElementById('sidebarAvatarImg');
  const span = document.getElementById('userInitial');
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
  document.getElementById('currentTime').textContent = timeString;
}

function loadStats(user) {
  const db = getDatabase();
  const today = new Date();
  const currentMonth = today.toISOString().slice(0, 7);

  const attendance = db.attendance.filter(a => 
    a.employeeId === user.employeeId && a.date.startsWith(currentMonth)
  );

  const overtime = db.overtimeRequests.filter(ot => 
    ot.employeeId === user.employeeId && 
    ot.date.startsWith(currentMonth) && 
    ot.status === 'approved'
  );

  const totalOvertimeMinutes = overtime.reduce((sum, ot) => sum + ot.totalMinutes, 0);
  const overtimeHours = (totalOvertimeMinutes / 60).toFixed(1);

  const payroll = db.payrolls.find(p => 
    p.employeeId === user.employeeId && p.month === currentMonth
  );

  const monthEl = document.getElementById('monthAttendance');
  if (monthEl) { monthEl.dataset.value = attendance.length; monthEl.textContent = attendance.length; }
  const overtimeEl = document.getElementById('overtimeHours');
  if (overtimeEl) { overtimeEl.dataset.value = overtimeHours + 'h'; overtimeEl.textContent = overtimeHours + 'h'; }
  const salaryVal = payroll ? (payroll.netSalary / 1000000).toFixed(1) + 'M' : '--';
  const salaryEl = document.getElementById('currentSalary');
  if (salaryEl) { salaryEl.dataset.value = salaryVal; salaryEl.textContent = salaryVal; }

  const leavesEl = document.getElementById('leavesRemaining');
  if (leavesEl) leavesEl.dataset.value = leavesEl.textContent;
}

function switchTab(tab, user) {
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
  document.getElementById(tabMap[tab]).classList.remove('hidden');

  loadTabContent(tab, user);
}

function loadTabContent(tab, user) {
  switch(tab) {
    case 'profile': loadProfile(user); break;
    case 'attendance': loadAttendance(user); break;
    case 'leaves': loadLeaves(user); break;
    case 'overtime': loadOvertime(user); break;
    case 'payroll': loadPayroll(user); break;
  }
}

// ===== CAMERA FUNCTIONS =====
async function startCamera(type) {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ 
      video: { facingMode: 'user' }, 
      audio: false 
    });

    if (type === 'checkIn') {
      checkInStream = stream;
      const video = document.getElementById('checkInVideo');
      video.srcObject = stream;
      video.classList.remove('hidden');
      
      document.getElementById('startCheckInCameraBtn').classList.add('hidden');
      document.getElementById('captureCheckInBtn').classList.remove('hidden');
    } else {
      checkOutStream = stream;
      const video = document.getElementById('checkOutVideo');
      video.srcObject = stream;
      video.classList.remove('hidden');
      
      document.getElementById('startCheckOutCameraBtn').classList.add('hidden');
      document.getElementById('captureCheckOutBtn').classList.remove('hidden');
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
    const context = canvas.getContext('2d');
    
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    
    checkInPhotoData = canvas.toDataURL('image/jpeg');
    
    // Show preview
    document.getElementById('checkInPhotoImg').src = checkInPhotoData;
    document.getElementById('checkInPhotoPreview').classList.remove('hidden');
    
    // Hide video and capture button, show submit button
    video.classList.add('hidden');
    document.getElementById('captureCheckInBtn').classList.add('hidden');
    document.getElementById('submitCheckInBtn').classList.remove('hidden');
    
    // Stop camera
    if (checkInStream) {
      checkInStream.getTracks().forEach(track => track.stop());
      checkInStream = null;
    }
  } else {
    const video = document.getElementById('checkOutVideo');
    const canvas = document.getElementById('checkOutCanvas');
    const context = canvas.getContext('2d');
    
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    
    checkOutPhotoData = canvas.toDataURL('image/jpeg');
    
    // Show preview
    document.getElementById('checkOutPhotoImg').src = checkOutPhotoData;
    document.getElementById('checkOutPhotoPreview').classList.remove('hidden');
    
    // Hide video and capture button, show submit button
    video.classList.add('hidden');
    document.getElementById('captureCheckOutBtn').classList.add('hidden');
    document.getElementById('submitCheckOutBtn').classList.remove('hidden');
    
    // Stop camera
    if (checkOutStream) {
      checkOutStream.getTracks().forEach(track => track.stop());
      checkOutStream = null;
    }
  }
}

function submitCheckIn(user) {
  if (!checkInPhotoData) {
    alert('Vui lòng chụp ảnh trước khi check-in!');
    return;
  }

  const db = getDatabase();
  const today = new Date().toISOString().split('T')[0];
  const now = new Date();
  const currentTime = now.toTimeString().slice(0, 8);

  // Check if already checked in today
  const existingAttendance = db.attendance.find(a => 
    a.employeeId === user.employeeId && a.date === today
  );

  if (existingAttendance && existingAttendance.checkIn) {
    alert('Bạn đã check-in hôm nay rồi!');
    return;
  }

  // Calculate late minutes (assuming work starts at 08:30)
  const startTime = new Date(`${today}T08:30:00`);
  const checkInTime = new Date(`${today}T${currentTime}`);
  const lateMinutes = checkInTime > startTime ? Math.floor((checkInTime - startTime) / 60000) : 0;

  if (existingAttendance) {
    // Update existing record
    existingAttendance.checkIn = currentTime;
    existingAttendance.lateMinutes = lateMinutes;
    existingAttendance.checkInPhoto = checkInPhotoData;
  } else {
    // Create new record
    const newAttendance = {
      id: Math.max(...db.attendance.map(a => a.id), 0) + 1,
      employeeId: user.employeeId,
      date: today,
      checkIn: currentTime,
      checkOut: null,
      lateMinutes: lateMinutes,
      overtimeMinutes: 0,
      checkInPhoto: checkInPhotoData,
      checkOutPhoto: ''
    };
    db.attendance.push(newAttendance);
  }

  saveDatabase(db);
  
  // Reset UI
  document.getElementById('checkInPhotoPreview').classList.add('hidden');
  document.getElementById('submitCheckInBtn').classList.add('hidden');
  document.getElementById('startCheckInCameraBtn').classList.remove('hidden');
  checkInPhotoData = null;

  alert('✓ Check-in thành công!');
  document.getElementById('checkInStatus').textContent = `Đã check-in lúc ${currentTime}`;
  loadAttendance(user);
  loadStats(user);
}

function submitCheckOut(user) {
  if (!checkOutPhotoData) {
    alert('Vui lòng chụp ảnh trước khi check-out!');
    return;
  }

  const db = getDatabase();
  const today = new Date().toISOString().split('T')[0];
  const now = new Date();
  const currentTime = now.toTimeString().slice(0, 8);

  const attendance = db.attendance.find(a => 
    a.employeeId === user.employeeId && a.date === today
  );

  if (!attendance || !attendance.checkIn) {
    alert('Bạn chưa check-in hôm nay!');
    return;
  }

  if (attendance.checkOut) {
    alert('Bạn đã check-out hôm nay rồi!');
    return;
  }

  // Calculate overtime (assuming work ends at 17:30)
  const endTime = new Date(`${today}T17:30:00`);
  const checkOutTime = new Date(`${today}T${currentTime}`);
  const overtimeMinutes = checkOutTime > endTime ? Math.floor((checkOutTime - endTime) / 60000) : 0;

  attendance.checkOut = currentTime;
  attendance.overtimeMinutes = overtimeMinutes;
  attendance.checkOutPhoto = checkOutPhotoData;

  saveDatabase(db);
  
  // Reset UI
  document.getElementById('checkOutPhotoPreview').classList.add('hidden');
  document.getElementById('submitCheckOutBtn').classList.add('hidden');
  document.getElementById('startCheckOutCameraBtn').classList.remove('hidden');
  checkOutPhotoData = null;

  alert('✓ Check-out thành công!');
  document.getElementById('checkOutStatus').textContent = `Đã check-out lúc ${currentTime}`;
  loadAttendance(user);
  loadStats(user);
}

function checkTodayAttendance(user) {
  const db = getDatabase();
  const today = new Date().toISOString().split('T')[0];
  
  const attendance = db.attendance.find(a => 
    a.employeeId === user.employeeId && a.date === today
  );

  if (attendance) {
    if (attendance.checkIn) {
      document.getElementById('checkInStatus').textContent = `Đã check-in lúc ${attendance.checkIn}`;
      document.getElementById('startCheckInCameraBtn').disabled = true;
      document.getElementById('startCheckInCameraBtn').classList.add('opacity-50', 'cursor-not-allowed');
    }
    if (attendance.checkOut) {
      document.getElementById('checkOutStatus').textContent = `Đã check-out lúc ${attendance.checkOut}`;
      document.getElementById('startCheckOutCameraBtn').disabled = true;
      document.getElementById('startCheckOutCameraBtn').classList.add('opacity-50', 'cursor-not-allowed');
    }
  }
}

// ===== PROFILE =====
function loadProfile(user) {
  const db = getDatabase();
  const employee = db.employees.find(e => e.id === user.employeeId);
  if (!employee) return;

  const department = db.departments.find(d => d.id === employee.departmentId);
  const position = db.positions.find(p => p.id === employee.positionId);

  document.getElementById('profileFullName').value = employee.fullName || '';
  document.getElementById('profileEmail').value = employee.email || '';
  document.getElementById('profilePhone').value = employee.phone || '';
  document.getElementById('profileEmployeeCode').value = employee.employeeCode || '';
  document.getElementById('profileDepartment').value = department ? department.name : '';
  document.getElementById('profilePosition').value = position ? position.name : '';
  document.getElementById('profileJoinDate').value = employee.joinDate 
    ? new Date(employee.joinDate).toLocaleDateString('vi-VN') 
    : '';

  document.getElementById('profileAvatarData').value = '';
  const initial = (employee.fullName || user.name || 'E').charAt(0).toUpperCase();
  if (employee.avatar) {
    document.getElementById('profileAvatarImg').src = employee.avatar;
    document.getElementById('profileAvatarImg').classList.remove('hidden');
    document.getElementById('profileAvatarInitial').classList.add('hidden');
    document.getElementById('profileAvatarRemoveBtn').classList.remove('hidden');
  } else {
    document.getElementById('profileAvatarImg').classList.add('hidden');
    document.getElementById('profileAvatarImg').src = '';
    document.getElementById('profileAvatarInitial').textContent = initial;
    document.getElementById('profileAvatarInitial').classList.remove('hidden');
    document.getElementById('profileAvatarRemoveBtn').classList.add('hidden');
  }

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
    btn.textContent = 'Lưu thay đổi';
    fullName.removeAttribute('readonly');
    email.removeAttribute('readonly');
    phone.removeAttribute('readonly');
    fullName.classList.remove('bg-slate-50');
    fullName.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    email.classList.remove('bg-slate-50');
    email.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    phone.classList.remove('bg-slate-50');
    phone.classList.add('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    if (avatarActions) avatarActions.classList.remove('hidden');
  } else {
    btn.textContent = 'Chỉnh sửa thông tin';
    fullName.setAttribute('readonly', 'readonly');
    email.setAttribute('readonly', 'readonly');
    phone.setAttribute('readonly', 'readonly');
    fullName.classList.add('bg-slate-50');
    fullName.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    email.classList.add('bg-slate-50');
    email.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    phone.classList.add('bg-slate-50');
    phone.classList.remove('border-slate-300', 'focus:ring-2', 'focus:ring-blue-500/20', 'focus:border-blue-500');
    if (avatarActions) avatarActions.classList.add('hidden');
  }
}

function saveProfile(user) {
  const db = getDatabase();
  const fullName = document.getElementById('profileFullName').value.trim();
  const email = document.getElementById('profileEmail').value.trim();
  const phone = document.getElementById('profilePhone').value.trim();
  const avatarData = document.getElementById('profileAvatarData').value;

  const employee = db.employees.find(e => e.id === user.employeeId);
  if (!employee) {
    alert('Không tìm thấy hồ sơ nhân viên.');
    return;
  }

  employee.fullName = fullName;
  employee.email = email;
  employee.phone = phone || employee.phone;
  if (avatarData === 'REMOVE') {
    employee.avatar = '';
  } else if (avatarData) {
    employee.avatar = avatarData;
  }
  document.getElementById('profileAvatarData').value = '';

  const account = db.userAccounts.find(a => a.id === user.id);
  if (account) {
    account.name = fullName;
    account.email = email;
  }

  saveDatabase(db);

  const updatedUser = { ...user, name: fullName, email };
  saveCurrentUser(updatedUser);

  document.getElementById('userName').textContent = fullName;
  const initial = fullName.charAt(0).toUpperCase();
  updateSidebarAvatar(employee.avatar || null, initial);

  alert('Đã lưu thông tin cá nhân.');
  setProfileEditMode(false);
}

// ===== LOAD DATA FUNCTIONS =====
function loadAttendance(user) {
  const db = getDatabase();
  const attendance = db.attendance.filter(a => a.employeeId === user.employeeId);
  attendance.sort((a, b) => new Date(b.date) - new Date(a.date));
  
  const tbody = document.getElementById('attendanceTableBody');
  const totalPages = Math.ceil(attendance.length / itemsPerPage);
  const startIndex = (currentPage.attendance - 1) * itemsPerPage;
  const paginatedData = attendance.slice(startIndex, startIndex + itemsPerPage);

  function calcEarlyLeaveMinutes(dateStr, checkOutStr) {
    if (!dateStr || !checkOutStr) return null;
    // Giờ chuẩn kết thúc làm việc: 17:30
    const endTime = new Date(`${dateStr}T17:30:00`);
    const checkOutTime = new Date(`${dateStr}T${checkOutStr}`);
    if (Number.isNaN(endTime.getTime()) || Number.isNaN(checkOutTime.getTime())) return null;
    if (checkOutTime >= endTime) return 0;
    return Math.floor((endTime - checkOutTime) / 60000);
  }

  tbody.innerHTML = paginatedData.map(att => {
    const earlyLeaveMinutes = calcEarlyLeaveMinutes(att.date, att.checkOut);
    const earlyLeaveText = earlyLeaveMinutes == null ? '-' : `${earlyLeaveMinutes} phút`;
    const earlyLeaveClass = earlyLeaveMinutes > 0 ? 'text-red-600 font-semibold' : 'text-gray-600';
    return `
    <tr class="border-b border-slate-100 hover:bg-slate-50/80">
      <td class="py-3 px-4 text-sm text-gray-900">${new Date(att.date).toLocaleDateString('vi-VN')}</td>
      <td class="py-3 px-4 text-sm font-medium text-gray-900">${att.checkIn || '-'}</td>
      <td class="py-3 px-4 text-sm font-medium text-gray-900">${att.checkOut || '-'}</td>
      <td class="py-3 px-4 text-sm ${att.lateMinutes > 0 ? 'text-red-600 font-semibold' : 'text-gray-600'}">
        ${att.lateMinutes} phút
      </td>
      <td class="py-3 px-4 text-sm ${earlyLeaveClass}">
        ${earlyLeaveText}
      </td>
      <td class="py-3 px-4 text-sm ${att.overtimeMinutes > 0 ? 'text-green-600 font-semibold' : 'text-gray-600'}">
        ${att.overtimeMinutes} phút
      </td>
    </tr>
  `;
  }).join('');

  renderPagination('attendancePagination', totalPages, currentPage.attendance, (page) => {
    currentPage.attendance = page;
    loadAttendance(user);
  });
}

function loadLeaves(user) {
  const db = getDatabase();
  const leaves = db.leaveRequests.filter(l => l.employeeId === user.employeeId);
  leaves.sort((a, b) => new Date(b.submittedDate) - new Date(a.submittedDate));
  
  const tbody = document.getElementById('leavesTableBody');
  const totalPages = Math.ceil(leaves.length / itemsPerPage);
  const startIndex = (currentPage.leaves - 1) * itemsPerPage;
  const paginatedData = leaves.slice(startIndex, startIndex + itemsPerPage);

  tbody.innerHTML = paginatedData.map(leave => {
    let statusBadge = '';
    if (leave.status === 'pending') statusBadge = 'badge-pending';
    else if (leave.status === 'approved') statusBadge = 'badge-approved';
    else statusBadge = 'badge-rejected';

    const statusText = leave.status === 'pending' ? 'Chờ duyệt' : 
                       leave.status === 'approved' ? 'Đã duyệt' : 'Từ chối';

    return `
      <tr class="border-b border-slate-100 hover:bg-slate-50/80">
        <td class="py-3 px-4 text-sm text-gray-900">${leave.leaveType}</td>
        <td class="py-3 px-4 text-sm text-gray-900">${new Date(leave.startDate).toLocaleDateString('vi-VN')}</td>
        <td class="py-3 px-4 text-sm text-gray-900">${new Date(leave.endDate).toLocaleDateString('vi-VN')}</td>
        <td class="py-3 px-4 text-sm font-medium text-gray-900">${leave.totalDays}</td>
        <td class="py-3 px-4 text-sm text-gray-600">${leave.reason}</td>
        <td class="py-3 px-4">
          <span class="badge ${statusBadge}">${statusText}</span>
        </td>
      </tr>
    `;
  }).join('');

  renderPagination('leavesPagination', totalPages, currentPage.leaves, (page) => {
    currentPage.leaves = page;
    loadLeaves(user);
  });
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

function loadOvertime(user) {
  const db = getDatabase();
  const employee = db.employees.find(e => e.id === user.employeeId);
  if (!employee) return;

  switchOvertimeView('available');

  const today = new Date().toISOString().split('T')[0];
  const receivedScheduleIds = db.overtimeRequests
    .filter(ot => ot.employeeId === user.employeeId && ot.scheduleId != null)
    .map(ot => ot.scheduleId);
  const availableSchedules = db.overtimeSchedules.filter(s => {
    if (s.departmentId !== employee.departmentId) return false;
    if (receivedScheduleIds.includes(s.id)) return false;
    if (s.date < today) return false;
    return true;
  });
  availableSchedules.sort((a, b) => a.date.localeCompare(b.date));

  const availableBody = document.getElementById('overtimeAvailableBody');
  const availableEmpty = document.getElementById('overtimeAvailableEmpty');
  if (availableSchedules.length === 0) {
    availableBody.innerHTML = '';
    if (availableEmpty) availableEmpty.classList.remove('hidden');
    document.getElementById('overtimeAvailablePagination').innerHTML = '';
  } else {
    if (availableEmpty) availableEmpty.classList.add('hidden');
    const totalAvailablePages = Math.max(1, Math.ceil(availableSchedules.length / itemsPerPage));
    const startAvailable = (currentPage.overtimeAvailable - 1) * itemsPerPage;
    const paginatedAvailable = availableSchedules.slice(startAvailable, startAvailable + itemsPerPage);
    availableBody.innerHTML = paginatedAvailable.map(schedule => {
      const st = schedule.startTime || '17:30:00';
      const et = schedule.endTime || '19:00:00';
      const timeStr = st.slice(0, 5) + ' - ' + et.slice(0, 5);
      return `
        <tr class="border-b border-slate-100 hover:bg-slate-50/80">
          <td class="py-3 px-4 text-sm text-gray-900">${new Date(schedule.date).toLocaleDateString('vi-VN')}</td>
          <td class="py-3 px-4 text-sm text-gray-900">${timeStr}</td>
          <td class="py-3 px-4 text-sm text-gray-600">${schedule.description || '-'}</td>
          <td class="py-3 px-4">
            <button type="button" onclick="acceptOvertime(${schedule.id})" class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition">Nhận tăng ca</button>
          </td>
        </tr>
      `;
    }).join('');
    renderPagination('overtimeAvailablePagination', totalAvailablePages, currentPage.overtimeAvailable, (page) => {
      currentPage.overtimeAvailable = page;
      loadOvertime(user);
    });
  }

  const overtime = db.overtimeRequests.filter(ot => ot.employeeId === user.employeeId);
  overtime.sort((a, b) => new Date(b.date) - new Date(a.date));

  const tbody = document.getElementById('overtimeTableBody');
  const totalPages = Math.ceil(overtime.length / itemsPerPage);
  const startIndex = (currentPage.overtime - 1) * itemsPerPage;
  const paginatedData = overtime.slice(startIndex, startIndex + itemsPerPage);

  tbody.innerHTML = paginatedData.map(ot => {
    let statusBadge = '';
    if (ot.status === 'pending') statusBadge = 'badge-pending';
    else if (ot.status === 'approved') statusBadge = 'badge-approved';
    else statusBadge = 'badge-rejected';

    const statusText = ot.status === 'pending' ? 'Chờ duyệt' : 
                       ot.status === 'approved' ? 'Đã duyệt' : 'Từ chối';
    const hours = (ot.totalMinutes / 60).toFixed(1);
    const st = (ot.startTime || '').slice(0, 5);
    const et = (ot.endTime || '').slice(0, 5);
    const timeStr = st && et ? st + ' - ' + et : '-';

    return `
      <tr class="border-b border-slate-100 hover:bg-slate-50/80">
        <td class="py-3 px-4 text-sm text-gray-900">${new Date(ot.date).toLocaleDateString('vi-VN')}</td>
        <td class="py-3 px-4 text-sm text-gray-900">${timeStr}</td>
        <td class="py-3 px-4 text-sm font-medium text-gray-900">${hours}h</td>
        <td class="py-3 px-4 text-sm text-gray-600">${ot.reason || '-'}</td>
        <td class="py-3 px-4">
          <span class="badge ${statusBadge}">${statusText}</span>
        </td>
      </tr>
    `;
  }).join('');

  renderPagination('overtimePagination', totalPages, currentPage.overtime, (page) => {
    currentPage.overtime = page;
    loadOvertime(user);
  });
}

function acceptOvertime(scheduleId) {
  const user = getCurrentUser();
  if (!user || user.role !== 'employee') return;

  const db = getDatabase();
  const schedule = db.overtimeSchedules.find(s => s.id === scheduleId);
  if (!schedule) {
    alert('Không tìm thấy lịch tăng ca.');
    return;
  }

  const employee = db.employees.find(e => e.id === user.employeeId);
  if (!employee || employee.departmentId !== schedule.departmentId) {
    alert('Bạn không thuộc phòng ban của lịch tăng ca này.');
    return;
  }

  const existing = db.overtimeRequests.find(ot => ot.employeeId === user.employeeId && ot.scheduleId === scheduleId);
  if (existing) {
    alert('Bạn đã nhận lịch tăng ca này rồi.');
    return;
  }

  const startTime = schedule.startTime || '17:30:00';
  const endTime = schedule.endTime || '19:00:00';
  const start = new Date(`${schedule.date}T${startTime}`);
  const end = new Date(`${schedule.date}T${endTime}`);
  const totalMinutes = Math.floor((end - start) / 60000);

  const newOT = {
    id: Math.max(...db.overtimeRequests.map(o => o.id), 0) + 1,
    employeeId: user.employeeId,
    scheduleId: scheduleId,
    date: schedule.date,
    startTime,
    endTime,
    totalMinutes,
    reason: schedule.description || 'Nhận tăng ca từ lịch',
    status: 'approved',
    submittedDate: new Date().toISOString().split('T')[0],
    approvedBy: schedule.createdBy,
    approvedDate: new Date().toISOString().split('T')[0]
  };

  db.overtimeRequests.push(newOT);
  saveDatabase(db);
  loadOvertime(user);
  loadStats(user);
  alert('Đã nhận tăng ca thành công!');
}

function loadPayroll(user) {
  const db = getDatabase();
  const payrolls = db.payrolls.filter(p => p.employeeId === user.employeeId);
  payrolls.sort((a, b) => b.month.localeCompare(a.month));
  
  const tbody = document.getElementById('payrollTableBody');
  const totalPages = Math.ceil(payrolls.length / itemsPerPage);
  const startIndex = (currentPage.payroll - 1) * itemsPerPage;
  const paginatedData = payrolls.slice(startIndex, startIndex + itemsPerPage);

  tbody.innerHTML = paginatedData.map(p => {
    const statusBadge = p.status === 'paid' ? 'badge-paid' : 'badge-pending';
    const statusText = p.status === 'paid' ? 'Đã thanh toán' : 'Chờ xử lý';

    return `
      <tr class="border-b border-slate-100 hover:bg-slate-50/80">
        <td class="py-3 px-4 text-sm font-medium text-gray-900">${p.month}</td>
        <td class="py-3 px-4 text-sm text-gray-900">${(p.baseSalary / 1000000).toFixed(1)}M</td>
        <td class="py-3 px-4 text-sm text-green-600">+${(p.overtimePay / 1000000).toFixed(2)}M</td>
        <td class="py-3 px-4 text-sm text-red-600">-${(p.latePenalty / 1000).toFixed(0)}K</td>
        <td class="py-3 px-4 text-sm text-blue-600">+${(p.allowances / 1000000).toFixed(2)}M</td>
        <td class="py-3 px-4 text-sm font-bold text-gray-900">${(p.netSalary / 1000000).toFixed(2)}M</td>
        <td class="py-3 px-4">
          <span class="badge ${statusBadge}">${statusText}</span>
        </td>
      </tr>
    `;
  }).join('');

  renderPagination('payrollPagination', totalPages, currentPage.payroll, (page) => {
    currentPage.payroll = page;
    loadPayroll(user);
  });
}

// ===== MODALS =====
function showLeaveModal(user) {
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

  document.getElementById('modalContainer').innerHTML = modal;

  document.getElementById('leaveForm').addEventListener('submit', function(e) {
    e.preventDefault();
    saveLeave(user);
  });
}

function saveLeave(user) {
  const db = getDatabase();
  const leaveType = document.getElementById('leaveType').value;
  const startDate = document.getElementById('startDate').value;
  const endDate = document.getElementById('endDate').value;
  const reason = document.getElementById('reason').value;

  const start = new Date(startDate);
  const end = new Date(endDate);
  const totalDays = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1;

  const newLeave = {
    id: Math.max(...db.leaveRequests.map(l => l.id), 0) + 1,
    employeeId: user.employeeId,
    leaveType,
    startDate,
    endDate,
    totalDays,
    reason,
    status: 'pending',
    submittedDate: new Date().toISOString().split('T')[0],
    approvedBy: null,
    approvedDate: null
  };

  db.leaveRequests.push(newLeave);
  saveDatabase(db);
  closeModal();
  loadLeaves(user);
  alert('Đã gửi đơn nghỉ phép!');
}

function closeModal() {
  document.getElementById('modalContainer').innerHTML = '';
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

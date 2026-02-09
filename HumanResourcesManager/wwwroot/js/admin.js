//// Admin Dashboard JavaScript
//let currentTab = 'profile';
//let currentPage = { users: 1, departments: 1, positions: 1 };
//const itemsPerPage = 10;

//document.addEventListener('DOMContentLoaded', function () {
//    // Check authentication and role
//    //const currentUser = getCurrentUser();
//    //if (!currentUser || currentUser.role !== 'admin') {
//    //    alert('Bạn không có quyền truy cập trang này!');
//    //    if (typeof showPageLoading === 'function') showPageLoading();
//    //    setTimeout(function () { window.location.href = 'index.html'; }, 1000);
//    //    return;
//    //}

//    // Initialize
//    const currentUser = {};
//    initializePage(currentUser);
//    updateClock();
//    setInterval(updateClock, 1000);
//    loadStats();
//    loadTabContent(currentTab);
//    loadSystemConfig();


//    // Event Listeners
//    document.querySelectorAll('.nav-item').forEach(item => {
//        item.addEventListener('click', function (e) {
//            e.preventDefault();
//            const tab = this.dataset.tab;
//            switchTab(tab);
//        });
//    });

//    document.getElementById('logoutBtn').addEventListener('click', logout);
//    document.getElementById('addUserBtn').addEventListener('click', () => showUserModal());
//    document.getElementById('addDepartmentBtn').addEventListener('click', () => showDepartmentModal());
//    document.getElementById('addPositionBtn').addEventListener('click', () => showPositionModal());
//    document.getElementById('addAllowanceBtn').addEventListener('click', addAllowanceRow);
//    document.getElementById('saveSystemConfigBtn').addEventListener('click', saveSystemConfig);
//    const profileEditSaveBtn = document.getElementById('profileEditSaveBtn');
//    if (profileEditSaveBtn) profileEditSaveBtn.addEventListener('click', toggleProfileEditSave);

//});

//function initializePage(user) {
//    // Set user info
//    document.getElementById('userName').textContent = user.name;
//    document.getElementById('userInitial').textContent = user.name ? user.name.charAt(0).toUpperCase() : 'A';

//    // Set current date
//    const today = new Date();
//    document.getElementById('currentDate').textContent = today.toLocaleDateString('vi-VN', {
//        year: 'numeric', month: 'long', day: 'numeric'
//    });
//}

function updateClock() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    const el = document.getElementById('currentTime');
    if (el) el.textContent = timeString;
}

//function loadProfileData() {
//    //const user = getCurrentUser();
//    //if (!user) return;
//    const fullNameEl = document.getElementById('profileFullName');
//    const emailEl = document.getElementById('profileEmail');
//    const phoneEl = document.getElementById('profilePhone');
//    const usernameEl = document.getElementById('profileUsername');
//    const initialEl = document.getElementById('profileAvatarInitial');
//    if (fullNameEl) fullNameEl.value = user.name || '';
//    if (emailEl) emailEl.value = user.email || '';
//    if (phoneEl) phoneEl.value = user.phone || '';
//    if (usernameEl) usernameEl.value = user.username || '';
//    if (initialEl) initialEl.textContent = user.name ? user.name.charAt(0).toUpperCase() : 'A';
//    // Đảm bảo readonly và nút về trạng thái "Chỉnh sửa"
//    setProfileReadonly(true);
//    const btn = document.getElementById('profileEditSaveBtn');
//    if (btn) btn.textContent = 'Chỉnh sửa thông tin';
//}

//function setProfileReadonly(readonly) {
//    const fullNameEl = document.getElementById('profileFullName');
//    const emailEl = document.getElementById('profileEmail');
//    const phoneEl = document.getElementById('profilePhone');
//    [fullNameEl, emailEl, phoneEl].forEach(function (el) {
//        if (!el) return;
//        el.readOnly = readonly;
//        el.classList.toggle('read-only:bg-slate-50', readonly);
//        el.classList.toggle('bg-white', !readonly);
//    });
//}

//function toggleProfileEditSave() {
//    const fullNameEl = document.getElementById('profileFullName');
//    const btn = document.getElementById('profileEditSaveBtn');
//    if (!fullNameEl || !btn) return;
//    const isReadonly = fullNameEl.readOnly;
//    if (isReadonly) {
//        setProfileReadonly(false);
//        btn.textContent = 'Lưu';
//        return;
//    }
//    // Lưu
//    const name = (document.getElementById('profileFullName') || {}).value || '';
//    const email = (document.getElementById('profileEmail') || {}).value || '';
//    const phone = (document.getElementById('profilePhone') || {}).value || '';
//    if (!name.trim()) {
//        alert('Họ và tên không được để trống.');
//        return;
//    }
//    const user = getCurrentUser();
//    if (!user) return;
//    const db = getDatabase();
//    const account = db.userAccounts.find(function (u) { return u.id === user.id; });
//    if (account) {
//        account.name = name.trim();
//        account.email = email.trim();
//        if (account.phone !== undefined) account.phone = phone.trim();
//        else if (phone.trim()) account.phone = phone.trim();
//        saveDatabase(db);
//    }
//    const updatedUser = Object.assign({}, user, { name: name.trim(), email: email.trim(), phone: phone.trim() });
//    saveCurrentUser(updatedUser);
//    document.getElementById('userName').textContent = name.trim();
//    document.getElementById('userInitial').textContent = name.trim() ? name.trim().charAt(0).toUpperCase() : 'A';
//    setProfileReadonly(true);
//    btn.textContent = 'Chỉnh sửa thông tin';
//}

//function loadStats() {
//    const db = getDatabase();
//    document.getElementById('totalUsers').textContent = db.userAccounts.length;
//    document.getElementById('totalDepartments').textContent = db.departments.length;
//    document.getElementById('totalPositions').textContent = db.positions.length;
//    document.getElementById('activeUsers').textContent = db.userAccounts.filter(u => u.status === 'active').length;
//}

//function switchTab(tab) {
//    currentTab = tab;

//    // Update nav items
//    document.querySelectorAll('.nav-item').forEach(item => {
//        item.classList.remove('active');
//        if (item.dataset.tab === tab) {
//            item.classList.add('active');
//        }
//    });

//    // Hide all tabs
//    document.querySelectorAll('.tab-content').forEach(content => {
//        content.classList.add('hidden');
//    });

//    // Show selected tab
//    const tabMap = {
//        'profile': 'profileTab',
//        'users': 'usersTab',
//        'departments': 'departmentsTab',
//        'positions': 'positionsTab',
//        'system': 'systemTab'
//    };
//    const tabId = tabMap[tab];
//    if (tabId) document.getElementById(tabId).classList.remove('hidden');

//    // Load content
//    loadTabContent(tab);
//}

//function loadTabContent(tab) {
//    switch (tab) {
//        case 'profile':
//            loadProfileData();
//            break;
//        case 'users':
//            loadUsers();
//            break;
//        case 'departments':
//            loadDepartments();
//            break;
//        case 'positions':
//            loadPositions();
//            break;
//        case 'system':
//            loadSystemConfig();
//            break;
//    }
//}

//// ===== USERS MANAGEMENT =====
//function loadUsers() {
//    const db = getDatabase();
//    const users = db.userAccounts;
//    const tbody = document.getElementById('usersTableBody');

//    // Pagination
//    const totalPages = Math.ceil(users.length / itemsPerPage);
//    const startIndex = (currentPage.users - 1) * itemsPerPage;
//    const endIndex = startIndex + itemsPerPage;
//    const paginatedUsers = users.slice(startIndex, endIndex);

//    // Render table
//    tbody.innerHTML = paginatedUsers.map(user => {
//        const roleLabels = {
//            'admin': 'Quản Trị Viên',
//            'hr': 'Nhân Sự',
//            'manager': 'Quản Lý',
//            'employee': 'Nhân Viên'
//        };

//        const statusClass = user.status === 'active' ? 'badge-active' : 'badge-inactive';
//        const statusText = user.status === 'active' ? 'Hoạt động' : 'Khóa';

//        return `
//      <tr class="border-b border-gray-100 hover:bg-gray-50">
//        <td class="py-3 px-4 text-sm text-gray-900">${user.id}</td>
//        <td class="py-3 px-4 text-sm font-medium text-gray-900">${user.username}</td>
//        <td class="py-3 px-4 text-sm text-gray-900">${user.name}</td>
//        <td class="py-3 px-4 text-sm text-gray-600">${user.email || '-'}</td>
//        <td class="py-3 px-4 text-sm text-gray-900">${roleLabels[user.role] || user.role}</td>
//        <td class="py-3 px-4">
//          <span class="badge ${statusClass}">${statusText}</span>
//        </td>
//        <td class="py-3 px-4">
//          <div class="flex items-center space-x-2">
//            <button onclick="editUser(${user.id})" class="text-blue-600 hover:text-blue-700">
//              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
//              </svg>
//            </button>
//            <button onclick="toggleUserStatus(${user.id})" class="text-yellow-600 hover:text-yellow-700">
//              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path>
//              </svg>
//            </button>
//            <button onclick="deleteUser(${user.id})" class="text-red-600 hover:text-red-700">
//              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
//              </svg>
//            </button>
//          </div>
//        </td>
//      </tr>
//    `;
//    }).join('');

//    // Render pagination
//    renderPagination('usersPagination', totalPages, currentPage.users, (page) => {
//        currentPage.users = page;
//        loadUsers();
//    });
//}

//function showUserModal(userId = null) {
//    const db = getDatabase();
//    const user = userId ? db.userAccounts.find(u => u.id === userId) : null;
//    const isEdit = !!user;

//    const modal = `
//    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50" id="userModal">
//      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
//        <div class="flex items-center justify-between mb-6">
//          <h3 class="text-xl font-bold text-gray-900">${isEdit ? 'Chỉnh Sửa' : 'Thêm'} Tài Khoản</h3>
//          <button onclick="closeModal()" class="text-gray-400 hover:text-gray-600">
//            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
//            </svg>
//          </button>
//        </div>

//        <form id="userForm" class="space-y-4">
//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Tên Đăng Nhập</label>
//            <input type="text" id="username" value="${user?.username || ''}" ${isEdit ? 'readonly' : ''} class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>

//          ${!isEdit ? `
//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Mật Khẩu</label>
//            <input type="password" id="password" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>
//          ` : ''}

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Họ Tên</label>
//            <input type="text" id="name" value="${user?.name || ''}" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
//            <input type="email" id="email" value="${user?.email || ''}" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Vai Trò</label>
//            <select id="role" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//              <option value="">-- Chọn vai trò --</option>
//              <option value="admin" ${user?.role === 'admin' ? 'selected' : ''}>Quản Trị Viên</option>
//              <option value="hr" ${user?.role === 'hr' ? 'selected' : ''}>Nhân Sự</option>
//              <option value="manager" ${user?.role === 'manager' ? 'selected' : ''}>Quản Lý</option>
//              <option value="employee" ${user?.role === 'employee' ? 'selected' : ''}>Nhân Viên</option>
//            </select>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Nhân Viên Liên Kết</label>
//            <select id="employeeId" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
//              <option value="">-- Không liên kết --</option>
//              ${db.employees.map(emp => `
//                <option value="${emp.id}" ${user?.employeeId === emp.id ? 'selected' : ''}>${emp.fullName} (${emp.employeeCode})</option>
//              `).join('')}
//            </select>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Trạng Thái</label>
//            <select id="status" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//              <option value="active" ${user?.status === 'active' ? 'selected' : ''}>Hoạt động</option>
//              <option value="inactive" ${user?.status === 'inactive' ? 'selected' : ''}>Khóa</option>
//            </select>
//          </div>

//          <div class="flex space-x-3 pt-4">
//            <button type="submit" class="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg transition">
//              ${isEdit ? 'Cập Nhật' : 'Thêm Mới'}
//            </button>
//            <button type="button" onclick="closeModal()" class="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-2 rounded-lg transition">
//              Hủy
//            </button>
//          </div>
//        </form>
//      </div>
//    </div>
//  `;

//    document.getElementById('modalContainer').innerHTML = modal;

//    document.getElementById('userForm').addEventListener('submit', function (e) {
//        e.preventDefault();
//        saveUser(userId);
//    });
//}

//function saveUser(userId) {
//    const db = getDatabase();
//    const username = document.getElementById('username').value.trim();
//    const name = document.getElementById('name').value.trim();
//    const email = document.getElementById('email').value.trim();
//    const role = document.getElementById('role').value;
//    const employeeId = document.getElementById('employeeId').value;
//    const status = document.getElementById('status').value;

//    if (userId) {
//        // Update existing user
//        const userIndex = db.userAccounts.findIndex(u => u.id === userId);
//        db.userAccounts[userIndex] = {
//            ...db.userAccounts[userIndex],
//            name,
//            email,
//            role,
//            employeeId: employeeId ? parseInt(employeeId) : null,
//            status
//        };
//    } else {
//        // Add new user
//        const password = document.getElementById('password').value.trim();
//        const newUser = {
//            id: Math.max(...db.userAccounts.map(u => u.id), 0) + 1,
//            username,
//            passwordHash: password,
//            name,
//            email,
//            role,
//            employeeId: employeeId ? parseInt(employeeId) : null,
//            status
//        };
//        db.userAccounts.push(newUser);
//    }

//    saveDatabase(db);
//    closeModal();
//    loadUsers();
//    loadStats();
//    alert(userId ? 'Cập nhật tài khoản thành công!' : 'Thêm tài khoản mới thành công!');
//}

//function editUser(userId) {
//    showUserModal(userId);
//}

//function toggleUserStatus(userId) {
//    if (!confirm('Bạn có chắc muốn thay đổi trạng thái tài khoản này?')) return;

//    const db = getDatabase();
//    const user = db.userAccounts.find(u => u.id === userId);
//    user.status = user.status === 'active' ? 'inactive' : 'active';

//    saveDatabase(db);
//    loadUsers();
//    loadStats();
//    alert('Đã cập nhật trạng thái tài khoản!');
//}

//function deleteUser(userId) {
//    if (!confirm('Bạn có chắc muốn xóa tài khoản này? Hành động này không thể hoàn tác!')) return;

//    const db = getDatabase();
//    db.userAccounts = db.userAccounts.filter(u => u.id !== userId);

//    saveDatabase(db);
//    loadUsers();
//    loadStats();
//    alert('Đã xóa tài khoản!');
//}

//// ===== DEPARTMENTS MANAGEMENT =====
//function loadDepartments() {
//    //const db = getDatabase();
//    //const departments = db.departments;
//    const tbody = document.getElementById('departmentsTab');

//    //// Pagination
//    //const totalPages = Math.ceil(departments.length / itemsPerPage);
//    //const startIndex = (currentPage.departments - 1) * itemsPerPage;
//    //const endIndex = startIndex + itemsPerPage;
//    //const paginatedDepts = departments.slice(startIndex, endIndex);

//    // Render table
//    //tbody.innerHTML = paginatedDepts.map(dept => {
//        //const manager = dept.managerId ? db.employees.find(e => e.id === dept.managerId) : null;

//    //    return `
//    //  <tr class="border-b border-gray-100 hover:bg-gray-50">
//    //    <td class="py-3 px-4 text-sm text-gray-900"></td>
//    //    <td class="py-3 px-4 text-sm font-medium text-gray-900"></td>
//    //    <td class="py-3 px-4 text-sm text-gray-600"></td>
//    //    <td class="py-3 px-4 text-sm text-gray-900"></td>
//    //    <td class="py-3 px-4">
//    //      <div class="flex items-center space-x-2">
//    //        <button onclick="editDepartment(${dept.id})" class="text-blue-600 hover:text-blue-700">
//    //          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//    //            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
//    //          </svg>
//    //        </button>
//    //        <button onclick="deleteDepartment(${dept.id})" class="text-red-600 hover:text-red-700">
//    //          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//    //            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
//    //          </svg>
//    //        </button>
//    //      </div>
//    //    //</td>
//    //  </tr>
//    //`;
//    //}).join('');

//    tbody.innerHTML =`
//      <tr class="border-b border-gray-100 hover:bg-gray-50">
//        <td class="py-3 px-4 text-sm text-gray-900"></td>
//        <td class="py-3 px-4 text-sm font-medium text-gray-900"></td>
//        <td class="py-3 px-4 text-sm text-gray-600"></td>
//        <td class="py-3 px-4 text-sm text-gray-900"></td>
//      </tr>
//    `;

//    //// Render pagination
//    //renderPagination('departmentsPagination', totalPages, currentPage.departments, (page) => {
//    //    currentPage.departments = page;
//    //    loadDepartments();
//    //});
//}

//function showDepartmentModal(deptId = null) {
//    const db = getDatabase();
//    const dept = deptId ? db.departments.find(d => d.id === deptId) : null;
//    const isEdit = !!dept;

//    const modal = `
//    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50" id="departmentModal">
//      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
//        <div class="flex items-center justify-between mb-6">
//          <h3 class="text-xl font-bold text-gray-900">${isEdit ? 'Chỉnh Sửa' : 'Thêm'} Phòng Ban</h3>
//          <button onclick="closeModal()" class="text-gray-400 hover:text-gray-600">
//            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
//            </svg>
//          </button>
//        </div>

//        <form id="departmentForm" class="space-y-4">
//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Tên Phòng Ban</label>
//            <input type="text" id="deptName" value="${dept?.name || ''}" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Mô Tả</label>
//            <textarea id="deptDescription" rows="3" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>${dept?.description || ''}</textarea>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Quản Lý</label>
//            <select id="deptManagerId" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
//              <option value="">-- Chọn quản lý --</option>
//              ${db.employees.map(emp => `
//                <option value="${emp.id}" ${dept?.managerId === emp.id ? 'selected' : ''}>${emp.fullName} (${emp.employeeCode})</option>
//              `).join('')}
//            </select>
//          </div>

//          <div class="flex space-x-3 pt-4">
//            <button type="submit" class="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg transition">
//              ${isEdit ? 'Cập Nhật' : 'Thêm Mới'}
//            </button>
//            <button type="button" onclick="closeModal()" class="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-2 rounded-lg transition">
//              Hủy
//            </button>
//          </div>
//        </form>
//      </div>
//    </div>
//  `;

//    document.getElementById('modalContainer').innerHTML = modal;

//    document.getElementById('departmentForm').addEventListener('submit', function (e) {
//        e.preventDefault();
//        saveDepartment(deptId);
//    });
//}

//function saveDepartment(deptId) {
//    const db = getDatabase();
//    const name = document.getElementById('deptName').value.trim();
//    const description = document.getElementById('deptDescription').value.trim();
//    const managerId = document.getElementById('deptManagerId').value;

//    if (deptId) {
//        // Update
//        const deptIndex = db.departments.findIndex(d => d.id === deptId);
//        db.departments[deptIndex] = {
//            ...db.departments[deptIndex],
//            name,
//            description,
//            managerId: managerId ? parseInt(managerId) : null
//        };
//    } else {
//        // Add new
//        const newDept = {
//            id: Math.max(...db.departments.map(d => d.id), 0) + 1,
//            name,
//            description,
//            managerId: managerId ? parseInt(managerId) : null
//        };
//        db.departments.push(newDept);
//    }

//    saveDatabase(db);
//    closeModal();
//    loadDepartments();
//    loadStats();
//    alert(deptId ? 'Cập nhật phòng ban thành công!' : 'Thêm phòng ban mới thành công!');
//}

//function editDepartment(deptId) {
//    showDepartmentModal(deptId);
//}

//function deleteDepartment(deptId) {
//    if (!confirm('Bạn có chắc muốn xóa phòng ban này?')) return;

//    const db = getDatabase();
//    db.departments = db.departments.filter(d => d.id !== deptId);

//    saveDatabase(db);
//    loadDepartments();
//    loadStats();
//    alert('Đã xóa phòng ban!');
//}

//// ===== POSITIONS MANAGEMENT =====
//function loadPositions() {
//    const db = getDatabase();
//    const positions = db.positions;
//    const tbody = document.getElementById('positionsTableBody');

//    // Pagination
//    const totalPages = Math.ceil(positions.length / itemsPerPage);
//    const startIndex = (currentPage.positions - 1) * itemsPerPage;
//    const endIndex = startIndex + itemsPerPage;
//    const paginatedPos = positions.slice(startIndex, endIndex);

//    // Render table
//    tbody.innerHTML = paginatedPos.map(pos => {
//        const dept = db.departments.find(d => d.id === pos.departmentId);

//        return `
//      <tr class="border-b border-gray-100 hover:bg-gray-50">
//        <td class="py-3 px-4 text-sm text-gray-900">${pos.id}</td>
//        <td class="py-3 px-4 text-sm font-medium text-gray-900">${pos.name}</td>
//        <td class="py-3 px-4 text-sm text-gray-900">${dept ? dept.name : '-'}</td>
//        <td class="py-3 px-4">
//          <div class="flex items-center space-x-2">
//            <button onclick="editPosition(${pos.id})" class="text-blue-600 hover:text-blue-700">
//              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
//              </svg>
//            </button>
//            <button onclick="deletePosition(${pos.id})" class="text-red-600 hover:text-red-700">
//              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
//              </svg>
//            </button>
//          </div>
//        </td>
//      </tr>
//    `;
//    }).join('');

//    // Render pagination
//    renderPagination('positionsPagination', totalPages, currentPage.positions, (page) => {
//        currentPage.positions = page;
//        loadPositions();
//    });
//}

//function showPositionModal(posId = null) {
//    const db = getDatabase();
//    const pos = posId ? db.positions.find(p => p.id === posId) : null;
//    const isEdit = !!pos;

//    const modal = `
//    <div class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50" id="positionModal">
//      <div class="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
//        <div class="flex items-center justify-between mb-6">
//          <h3 class="text-xl font-bold text-gray-900">${isEdit ? 'Chỉnh Sửa' : 'Thêm'} Chức Vụ</h3>
//          <button onclick="closeModal()" class="text-gray-400 hover:text-gray-600">
//            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
//            </svg>
//          </button>
//        </div>

//        <form id="positionForm" class="space-y-4">
//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Tên Chức Vụ</label>
//            <input type="text" id="posName" value="${pos?.name || ''}" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//          </div>

//          <div>
//            <label class="block text-sm font-medium text-gray-700 mb-2">Phòng Ban</label>
//            <select id="posDepartmentId" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required>
//              <option value="">-- Chọn phòng ban --</option>
//              ${db.departments.map(dept => `
//                <option value="${dept.id}" ${pos?.departmentId === dept.id ? 'selected' : ''}>${dept.name}</option>
//              `).join('')}
//            </select>
//          </div>

//          <div class="flex space-x-3 pt-4">
//            <button type="submit" class="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg transition">
//              ${isEdit ? 'Cập Nhật' : 'Thêm Mới'}
//            </button>
//            <button type="button" onclick="closeModal()" class="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-2 rounded-lg transition">
//              Hủy
//            </button>
//          </div>
//        </form>
//      </div>
//    </div>
//  `;

//    document.getElementById('modalContainer').innerHTML = modal;

//    document.getElementById('positionForm').addEventListener('submit', function (e) {
//        e.preventDefault();
//        savePosition(posId);
//    });
//}

//function savePosition(posId) {
//    const db = getDatabase();
//    const name = document.getElementById('posName').value.trim();
//    const departmentId = parseInt(document.getElementById('posDepartmentId').value);

//    if (posId) {
//        // Update
//        const posIndex = db.positions.findIndex(p => p.id === posId);
//        db.positions[posIndex] = {
//            ...db.positions[posIndex],
//            name,
//            departmentId
//        };
//    } else {
//        // Add new
//        const newPos = {
//            id: Math.max(...db.positions.map(p => p.id), 0) + 1,
//            name,
//            departmentId
//        };
//        db.positions.push(newPos);
//    }

//    saveDatabase(db);
//    closeModal();
//    loadPositions();
//    loadStats();
//    alert(posId ? 'Cập nhật chức vụ thành công!' : 'Thêm chức vụ mới thành công!');
//}

//function editPosition(posId) {
//    showPositionModal(posId);
//}

//function deletePosition(posId) {
//    if (!confirm('Bạn có chắc muốn xóa chức vụ này?')) return;

//    const db = getDatabase();
//    db.positions = db.positions.filter(p => p.id !== posId);

//    saveDatabase(db);
//    loadPositions();
//    loadStats();
//    alert('Đã xóa chức vụ!');
//}

//// ===== SYSTEM CONFIG =====
//function loadSystemConfig() {
//    const db = getDatabase();
//    const config = db.systemConfig;

//    document.getElementById('overtimeRate').value = config.overtimeRate;
//    document.getElementById('latePenaltyRate').value = config.latePenaltyRate;

//    renderAllowances(config.allowances);
//}

//function renderAllowances(allowances) {
//    const container = document.getElementById('allowancesList');
//    container.innerHTML = allowances.map((allowance, index) => `
//    <div class="flex items-center space-x-3 bg-white p-3 rounded-lg border border-gray-200">
//      <input type="text" value="${allowance.name}" class="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm" placeholder="Tên phụ cấp" data-index="${index}" data-field="name">
//      <input type="number" value="${allowance.amount}" class="w-32 px-3 py-2 border border-gray-300 rounded-lg text-sm" placeholder="Số tiền" data-index="${index}" data-field="amount">
//      <button onclick="removeAllowance(${index})" class="text-red-600 hover:text-red-700">
//        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
//          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
//        </svg>
//      </button>
//    </div>
//  `).join('');
//}

//function addAllowanceRow() {
//    const db = getDatabase();
//    db.systemConfig.allowances.push({ name: '', amount: 0 });
//    renderAllowances(db.systemConfig.allowances);
//}

//function removeAllowance(index) {
//    const db = getDatabase();
//    db.systemConfig.allowances.splice(index, 1);
//    renderAllowances(db.systemConfig.allowances);
//}

//function saveSystemConfig() {
//    const db = getDatabase();

//    db.systemConfig.overtimeRate = parseFloat(document.getElementById('overtimeRate').value);
//    db.systemConfig.latePenaltyRate = parseFloat(document.getElementById('latePenaltyRate').value);

//    // Update allowances
//    const allowanceInputs = document.querySelectorAll('#allowancesList input');
//    allowanceInputs.forEach(input => {
//        const index = parseInt(input.dataset.index);
//        const field = input.dataset.field;
//        if (field === 'name') {
//            db.systemConfig.allowances[index].name = input.value;
//        } else if (field === 'amount') {
//            db.systemConfig.allowances[index].amount = parseFloat(input.value);
//        }
//    });

//    saveDatabase(db);
//    alert('Đã lưu cấu hình hệ thống!');
//}

//// ===== UTILITY FUNCTIONS =====
//function renderPagination(containerId, totalPages, currentPageNum, onPageChange) {
//    const container = document.getElementById(containerId);
//    if (totalPages <= 1) {
//        container.innerHTML = '';
//        return;
//    }

//    let html = '<div class="flex items-center justify-center space-x-2">';

//    // Previous button
//    html += `<button onclick="changePage(${currentPageNum - 1}, '${containerId}')" ${currentPageNum === 1 ? 'disabled' : ''} class="px-3 py-1 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">Trước</button>`;

//    // Page numbers
//    for (let i = 1; i <= totalPages; i++) {
//        if (i === 1 || i === totalPages || (i >= currentPageNum - 1 && i <= currentPageNum + 1)) {
//            html += `<button onclick="changePage(${i}, '${containerId}')" class="px-3 py-1 border rounded-lg ${i === currentPageNum ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 hover:bg-gray-50'}">${i}</button>`;
//        } else if (i === currentPageNum - 2 || i === currentPageNum + 2) {
//            html += '<span class="px-2">...</span>';
//        }
//    }

//    // Next button
//    html += `<button onclick="changePage(${currentPageNum + 1}, '${containerId}')" ${currentPageNum === totalPages ? 'disabled' : ''} class="px-3 py-1 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">Sau</button>`;

//    html += '</div>';
//    container.innerHTML = html;

//    // Store callback
//    window[`pagination_${containerId}`] = onPageChange;
//}

//function changePage(page, containerId) {
//    const callback = window[`pagination_${containerId}`];
//    if (callback) callback(page);
//}

//function closeModal() {
//    document.getElementById('modalContainer').innerHTML = '';
//}

// =======================
// ADMIN DASHBOARD (NO DB)
// =======================

let currentTab = 'profile';
let currentPage = { users: 1, departments: 1, positions: 1 };
const itemsPerPage = 10;

// ===== FAKE DATABASE =====
const state = {
    currentUser: {
        id: 1,
        username: 'admin',
        name: 'Admin',
        email: 'admin@gmail.com',
        phone: '',
        role: 'admin'
    },
    users: [
        { id: 1, username: 'admin', name: 'Admin', email: 'admin@gmail.com', role: 'admin', status: 'active' }
    ],
    departments: [],
    positions: [],
    systemConfig: {
        overtimeRate: 1.5,
        latePenaltyRate: 0.5,
        allowances: []
    }
};

// ===== INIT =====
document.addEventListener('DOMContentLoaded', () => {
    initializePage(state.currentUser);
    updateClock();
    setInterval(updateClock, 1000);

    switchTab(currentTab);

    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', e => {
            e.preventDefault();
            switchTab(item.dataset.tab);
        });
    });

    document.getElementById('logoutBtn')?.addEventListener('click', () => {
        alert('Logout (demo)');
    });
});

// ===== PAGE =====
function initializePage(user) {
    document.getElementById('userName').textContent = user.name;
    document.getElementById('userInitial').textContent = user.name.charAt(0).toUpperCase();

    document.getElementById('currentDate').textContent =
        new Date().toLocaleDateString('vi-VN', { year: 'numeric', month: 'long', day: 'numeric' });
}

function updateClock() {
    document.getElementById('currentTime').textContent =
        new Date().toLocaleTimeString('vi-VN');
}

//// ===== TAB =====
//function switchTab(tab) {
//    currentTab = tab;

//    document.querySelectorAll('.nav-item').forEach(i =>
//        i.classList.toggle('active', i.dataset.tab === tab)
//    );

//    document.querySelectorAll('.tab-content').forEach(t =>
//        t.classList.add('hidden')
//    );

//    const map = {
//        profile: 'profileTab',
//        users: 'usersTab',
//        departments: 'departmentsTab',
//        positions: 'positionsTab',
//        system: 'systemTab'
//    };

//    document.getElementById(map[tab])?.classList.remove('hidden');
//    loadTabContent(tab);
//}

//function loadTabContent(tab) {
//    if (tab === 'profile') loadProfile();
//    if (tab === 'users') loadUsers();
//    if (tab === 'departments') loadDepartments();
//    if (tab === 'positions') loadPositions();
//    if (tab === 'system') loadSystemConfig();
//}

// ===== PROFILE =====
function loadProfile() {
    document.getElementById('profileFullName').value = state.currentUser.name;
    document.getElementById('profileEmail').value = state.currentUser.email;
    document.getElementById('profilePhone').value = state.currentUser.phone || '';
}

// ===== USERS =====
function loadUsers() {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = state.users.map(u => `
        <tr>
            <td class="px-4 py-2">${u.id}</td>
            <td class="px-4 py-2">${u.username}</td>
            <td class="px-4 py-2">${u.name}</td>
            <td class="px-4 py-2">${u.email}</td>
            <td class="px-4 py-2">${u.role}</td>
            <td class="px-4 py-2">${u.status}</td>
        </tr>
    `).join('');
}

// ===== DEPARTMENTS =====
function loadDepartments() {
    document.getElementById('departmentsTab').innerHTML =
        `<p class="text-gray-500">Chưa có phòng ban</p>`;
}

// ===== POSITIONS =====
function loadPositions() {
    document.getElementById('positionsTableBody').innerHTML = '';
}

// ===== SYSTEM =====
function loadSystemConfig() {
    document.getElementById('overtimeRate').value = state.systemConfig.overtimeRate;
    document.getElementById('latePenaltyRate').value = state.systemConfig.latePenaltyRate;
}

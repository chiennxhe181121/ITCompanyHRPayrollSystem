var hrCurrentTab = 'employees';
var hrProfileEditMode = false;
var hrCurrentOrgSubTab = 'departments';
var hrCurrentPage = { employees: 1, contracts: 1, leaves: 1, payrolls: 1 };
var hrItemsPerPage = 10;

function formatVND(num) {
    if (num == null || isNaN(num)) return '0';
    return new Intl.NumberFormat('vi-VN', { style: 'decimal' }).format(num) + ' ₫';
}

document.addEventListener('DOMContentLoaded', function () {
    var currentUser = { name: 'HR', email: 'hr@example.com', role: 'hr' };

    hrInitializePage(currentUser);
    hrLoadStats();
    hrLoadTabContent(hrCurrentTab, currentUser);
    hrUpdateClock();
    setInterval(hrUpdateClock, 1000);

    document.querySelectorAll('.nav-item').forEach(function (item) {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            hrSwitchTab(this.getAttribute('data-tab'), currentUser);
        });
    });

    var logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn && logoutBtn.tagName === 'A') {
        // Link đăng xuất - giữ nguyên href
    } else if (logoutBtn) {
        logoutBtn.addEventListener('click', function () {
            window.location.href = '/HumanResourcesManager/Auth/Logout' || '/Account/Login';
        });
    }

    var addContractBtn = document.getElementById('addContractBtn');
    if (addContractBtn) addContractBtn.addEventListener('click', function () { alert('Thêm hợp đồng – chưa nối backend.'); });

    var createPayrollBtn = document.getElementById('createPayrollBtn');
    if (createPayrollBtn) createPayrollBtn.addEventListener('click', function () { alert('Tạo bảng lương – chưa nối backend.'); });

    var reportLoadBtn = document.getElementById('reportLoadBtn');
    if (reportLoadBtn) reportLoadBtn.addEventListener('click', function () { hrLoadReport(currentUser); });

    document.querySelectorAll('[data-org-sub]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            hrSwitchOrgSubTab(this.getAttribute('data-org-sub'), currentUser);
        });
    });

    var profileEditSaveBtn = document.getElementById('profileEditSaveBtn');
    if (profileEditSaveBtn) {
        profileEditSaveBtn.addEventListener('click', function () {
            if (hrProfileEditMode) {
                alert('Đã lưu (giả lập – chưa nối backend).');
                hrSetProfileEditMode(false);
            } else {
                hrSetProfileEditMode(true);
            }
        });
    }

    var profileAvatarBtn = document.getElementById('profileAvatarBtn');
    if (profileAvatarBtn) profileAvatarBtn.addEventListener('click', function () {
        var input = document.getElementById('profileAvatarInput');
        if (input) input.click();
    });

    var profileAvatarInput = document.getElementById('profileAvatarInput');
    if (profileAvatarInput) {
        profileAvatarInput.addEventListener('change', function (e) {
            var file = e.target.files && e.target.files[0];
            if (!file || !file.type.startsWith('image/')) return;
            var reader = new FileReader();
            reader.onload = function () {
                var img = document.getElementById('profileAvatarImg');
                var initial = document.getElementById('profileAvatarInitial');
                if (img) { img.src = reader.result; img.classList.remove('hidden'); }
                if (initial) initial.classList.add('hidden');
            };
            reader.readAsDataURL(file);
            e.target.value = '';
        });
    }

    var profileAvatarRemoveBtn = document.getElementById('profileAvatarRemoveBtn');
    if (profileAvatarRemoveBtn) {
        profileAvatarRemoveBtn.addEventListener('click', function () {
            var img = document.getElementById('profileAvatarImg');
            var initial = document.getElementById('profileAvatarInitial');
            if (img) { img.classList.add('hidden'); img.src = ''; }
            if (initial) { initial.textContent = 'H'; initial.classList.remove('hidden'); }
        });
    }

    var now = new Date();
    var ym = now.getFullYear() + '-' + String(now.getMonth() + 1).padStart(2, '0');
    var payrollMonthInput = document.getElementById('payrollMonthInput');
    if (payrollMonthInput) payrollMonthInput.value = ym;
    var reportMonthInput = document.getElementById('reportMonthInput');
    if (reportMonthInput) reportMonthInput.value = ym;
});

function hrInitializePage(user) {
    var userNameEl = document.getElementById('userName');
    if (userNameEl) userNameEl.textContent = user.name || 'HR';
    var initial = (user.name || 'H').charAt(0).toUpperCase();
    var userInitialEl = document.getElementById('userInitial');
    if (userInitialEl) userInitialEl.textContent = initial;

    var currentDateEl = document.getElementById('currentDate');
    if (currentDateEl) {
        var today = new Date();
        currentDateEl.textContent = today.toLocaleDateString('vi-VN', { year: 'numeric', month: 'long', day: 'numeric' });
    }
}

function hrUpdateClock() {
    var el = document.getElementById('currentTime');
    if (el) el.textContent = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function hrLoadStats() {
    var totalEmployees = document.getElementById('totalEmployees');
    var pendingLeaves = document.getElementById('pendingLeaves');
    var totalContracts = document.getElementById('totalContracts');
    var payrollCount = document.getElementById('payrollCount');
    if (totalEmployees) totalEmployees.textContent = '0';
    if (pendingLeaves) pendingLeaves.textContent = '0';
    if (totalContracts) totalContracts.textContent = '0';
    if (payrollCount) payrollCount.textContent = '0';
}

function hrSwitchTab(tab, user) {
    hrCurrentTab = tab;
    document.querySelectorAll('.nav-item').forEach(function (item) {
        item.classList.remove('active');
        if (item.getAttribute('data-tab') === tab) item.classList.add('active');
    });
    document.querySelectorAll('.tab-content').forEach(function (content) { content.classList.add('hidden'); });
    var tabMap = {
        profile: 'profileTab',
        employees: 'employeesTab',
        departments: 'departmentsTab',
        contracts: 'contractsTab',
        leaves: 'leavesTab',
        payrolls: 'payrollsTab',
        reports: 'reportsTab'
    };
    var tabId = tabMap[tab];
    var tabEl = tabId ? document.getElementById(tabId) : null;
    if (tabEl) tabEl.classList.remove('hidden');
    hrLoadTabContent(tab, user);
}

function hrLoadTabContent(tab, user) {
    switch (tab) {
        case 'profile':
            break;
        case 'employees':
        case 'departments':
        case 'contracts':
        case 'leaves':
        case 'payrolls':
        case 'reports':
            break;
    }
    if (tab === 'departments') hrSwitchOrgSubTab(hrCurrentOrgSubTab, user);
}

function hrSwitchOrgSubTab(sub, user) {
    hrCurrentOrgSubTab = sub;
    var deptPanel = document.getElementById('orgDepartmentsPanel');
    var posPanel = document.getElementById('orgPositionsPanel');
    var btnDept = document.getElementById('orgSubBtnDepartments');
    var btnPos = document.getElementById('orgSubBtnPositions');

    if (sub === 'departments') {
        if (deptPanel) deptPanel.classList.remove('hidden');
        if (posPanel) posPanel.classList.add('hidden');
        if (btnDept) { btnDept.classList.add('bg-blue-600', 'text-white'); btnDept.classList.remove('bg-slate-100', 'text-slate-600'); }
        if (btnPos) { btnPos.classList.remove('bg-blue-600', 'text-white'); btnPos.classList.add('bg-slate-100', 'text-slate-600'); }
    } else {
        if (deptPanel) deptPanel.classList.add('hidden');
        if (posPanel) posPanel.classList.remove('hidden');
        if (btnDept) { btnDept.classList.remove('bg-blue-600', 'text-white'); btnDept.classList.add('bg-slate-100', 'text-slate-600'); }
        if (btnPos) { btnPos.classList.add('bg-blue-600', 'text-white'); btnPos.classList.remove('bg-slate-100', 'text-slate-600'); }
    }
}

function hrSetProfileEditMode(editing) {
    hrProfileEditMode = editing;
    var btn = document.getElementById('profileEditSaveBtn');
    var fullName = document.getElementById('profileFullName');
    var email = document.getElementById('profileEmail');
    var phone = document.getElementById('profilePhone');
    var avatarActions = document.getElementById('profileAvatarActions');

    if (editing) {
        if (btn) btn.textContent = 'Lưu thay đổi';
        if (fullName) { fullName.removeAttribute('readonly'); fullName.classList.remove('bg-slate-50'); }
        if (email) { email.removeAttribute('readonly'); email.classList.remove('bg-slate-50'); }
        if (phone) { phone.removeAttribute('readonly'); phone.classList.remove('bg-slate-50'); }
        if (avatarActions) avatarActions.classList.remove('hidden');
    } else {
        if (btn) btn.textContent = 'Chỉnh sửa thông tin';
        if (fullName) { fullName.setAttribute('readonly', 'readonly'); fullName.classList.add('bg-slate-50'); }
        if (email) { email.setAttribute('readonly', 'readonly'); email.classList.add('bg-slate-50'); }
        if (phone) { phone.setAttribute('readonly', 'readonly'); phone.classList.add('bg-slate-50'); }
        if (avatarActions) avatarActions.classList.add('hidden');
    }
}

function hrLoadReport(user) {
    var reportTotalSalary = document.getElementById('reportTotalSalary');
    var reportEmployeeCount = document.getElementById('reportEmployeeCount');
    var reportAvgSalary = document.getElementById('reportAvgSalary');
    var reportsTableBody = document.getElementById('reportsTableBody');
    if (reportTotalSalary) reportTotalSalary.textContent = formatVND(0);
    if (reportEmployeeCount) reportEmployeeCount.textContent = '0';
    if (reportAvgSalary) reportAvgSalary.textContent = formatVND(0);
    if (reportsTableBody) reportsTableBody.innerHTML = '';
}

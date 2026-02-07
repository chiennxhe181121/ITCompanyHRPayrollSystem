// admin-tabs.js
document.addEventListener('DOMContentLoaded', () => {
    const navItems = document.querySelectorAll('.manager-sidebar .nav-item');
    const tabContents = document.querySelectorAll('.tab-content');

    function switchTab(tabName) {
        // Active sidebar item
        navItems.forEach(item => item.classList.remove('active'));
        const activeNav = document.querySelector(`.nav-item[data-tab="${tabName}"]`);
        if (activeNav) activeNav.classList.add('active');

        // Hide all tab contents
        tabContents.forEach(tab => tab.classList.add('hidden'));

        // Show selected tab
        const targetTab = document.getElementById(tabName + 'Tab');
        if (targetTab) {
            targetTab.classList.remove('hidden');
        } else {
            console.warn('Không tìm thấy tab:', tabName);
        }
    }

    // Gán sự kiện click
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const tabName = item.dataset.tab;
            switchTab(tabName);
        });
    });

    // Mở tab mặc định khi load (ưu tiên nav-item có class active)
    const defaultTab =
        document.querySelector('.nav-item.active')?.dataset.tab
        || navItems[0]?.dataset.tab;

    if (defaultTab) {
        switchTab(defaultTab);
    }
});

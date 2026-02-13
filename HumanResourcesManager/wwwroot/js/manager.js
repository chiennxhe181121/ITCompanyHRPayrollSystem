// Manager Dashboard
// - Handle tab switching
// - Header clock + date
// - Basic hooks for future integration with backend (ASP.NET)

let currentTab = "team";

document.addEventListener("DOMContentLoaded", () => {
    // Tab switching
    const navItems = document.querySelectorAll(".nav-item");
    const tabContents = document.querySelectorAll(".tab-content");

    function switchTab(tabName) {
        currentTab = tabName;

        navItems.forEach(item => {
            item.classList.toggle("active", item.dataset.tab === tabName);
        });

        tabContents.forEach(content => {
            const expectedId =
                tabName === "profile" ? "profileTab" :
                tabName === "team" ? "teamTab" :
                tabName === "overtime" ? "overtimeTab" :
                tabName === "schedule" ? "scheduleTab" :
                "";

            if (!expectedId) return;
            content.classList.toggle("hidden", content.id !== expectedId);
        });
    }

    navItems.forEach(item => {
        item.addEventListener("click", e => {
            e.preventDefault();
            const tab = item.dataset.tab;
            if (!tab) return;
            switchTab(tab);
        });
    });

    // Default tab
    switchTab(currentTab);

    // Header date + clock
    const dateEl = document.getElementById("currentDate");
    const timeEl = document.getElementById("currentTime");

    function updateClock() {
        const now = new Date();
        if (dateEl) {
            dateEl.textContent = now.toLocaleDateString("vi-VN", {
                year: "numeric",
                month: "long",
                day: "numeric"
            });
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
});

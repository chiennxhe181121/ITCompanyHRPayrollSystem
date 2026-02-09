//// Loading overlay khi chuyển trang - dùng loading.gif, tối thiểu 1s
//(function() {
//  var overlayId = 'pageLoadingOverlay';
//  var minLoadingMs = 1000;
//  var pageLoadStart = Date.now();

//  function getOverlay() {
//    return document.getElementById(overlayId);
//  }

//  function showPageLoading() {
//    var el = getOverlay();
//    if (el) el.classList.remove('hidden');
//  }

//  function hidePageLoading() {
//    var el = getOverlay();
//    if (el) el.classList.add('hidden');
//  }

//  function hideAfterMinDuration() {
//    var elapsed = Date.now() - pageLoadStart;
//    var delay = Math.max(0, minLoadingMs - elapsed);
//    if (delay > 0) setTimeout(hidePageLoading, delay);
//    else hidePageLoading();
//  }

//  window.showPageLoading = showPageLoading;
//  window.hidePageLoading = hidePageLoading;

//  // Khi trang load xong thì ẩn overlay sau tối thiểu 1 giây
//  if (document.readyState === 'loading') {
//    document.addEventListener('DOMContentLoaded', hideAfterMinDuration);
//  } else {
//    hideAfterMinDuration();
//  }
//})();


// Loading overlay khi chuyển trang - dùng loading.gif, tối thiểu 1s
(function () {
    var overlayId = 'pageLoadingOverlay';
    var minLoadingMs = 1000;
    var pageLoadStart = Date.now();

    function getOverlay() {
        return document.getElementById(overlayId);
    }

    function showPageLoading() {
        var el = getOverlay();
        if (el) el.classList.remove('hidden');
    }

    function hidePageLoading() {
        var el = getOverlay();
        if (el) el.classList.add('hidden');
    }

    function hideAfterMinDuration() {
        var elapsed = Date.now() - pageLoadStart;
        var delay = Math.max(0, minLoadingMs - elapsed);
        setTimeout(hidePageLoading, delay);
    }

    // expose global (phòng khi cần gọi tay)
    window.showPageLoading = showPageLoading;
    window.hidePageLoading = hidePageLoading;

    // ====== 1️⃣ ẨN LOADING SAU KHI LOAD TRANG ======
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', hideAfterMinDuration);
    } else {
        hideAfterMinDuration();
    }

    // ====== 2️⃣ SHOW LOADING KHI CLICK LINK ======
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a');
        if (!link) return;

        var href = link.getAttribute('href');

        // bỏ qua link fake
        if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;

        // bỏ qua tab mới
        if (link.target === '_blank') return;

        showPageLoading();
    });

})();

// Loading overlay khi chuyển trang - dùng loading.gif, tối thiểu 1s
(function() {
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
    if (delay > 0) setTimeout(hidePageLoading, delay);
    else hidePageLoading();
  }

  window.showPageLoading = showPageLoading;
  window.hidePageLoading = hidePageLoading;

  // Khi trang load xong thì ẩn overlay sau tối thiểu 1 giây
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', hideAfterMinDuration);
  } else {
    hideAfterMinDuration();
  }
})();

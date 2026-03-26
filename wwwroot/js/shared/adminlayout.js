// Toggle Sidebar
function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");
    sidebar.classList.toggle("collapsed");
    mainContent.classList.toggle("expanded");
    // Lưu trạng thái vào localStorage
    localStorage.setItem("sidebarCollapsed", sidebar.classList.contains("collapsed"));
}

// Khôi phục trạng thái sidebar từ localStorage
(function restoreSidebarState() {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");
    const isCollapsed = localStorage.getItem("sidebarCollapsed") === "true";

    if (window.innerWidth < 768) {
        // Mobile: luôn collapsed
        sidebar.classList.add("collapsed");
        mainContent.classList.add("expanded");
    } else if (isCollapsed) {
        sidebar.classList.add("collapsed");
        mainContent.classList.add("expanded");
    }
})();

// Dọn dẹp backdrop và modal-open khi tất cả modal đã đóng (ưu tiên sự kiện)
document.addEventListener('hidden.bs.modal', function () {
    setTimeout(function () {
        if (document.querySelectorAll('.modal.show').length === 0) {
            document.body.classList.remove('modal-open');
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        }
    }, 200);
});
// Force remove modal overlay nếu bị kẹt (dự phòng)
setInterval(function () {
    if (document.body.classList.contains('modal-open') && document.querySelectorAll('.modal.show').length === 0) {
        document.body.classList.remove('modal-open');
        document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
    }
}, 1000);
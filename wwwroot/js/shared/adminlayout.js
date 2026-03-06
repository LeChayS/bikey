// Toggle Sidebar
function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");
    sidebar.classList.toggle("collapsed");
    mainContent.classList.toggle("expanded");
}

// Mobile sidebar
if (window.innerWidth < 768) {
    document.getElementById("sidebar").classList.add("collapsed");
    document.getElementById("mainContent").classList.add("expanded");
}

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
document.addEventListener('DOMContentLoaded', function () {
    const editModalEl = document.getElementById('editLoai');
    if (editModalEl) {
        editModalEl.addEventListener('show.bs.modal', function (event) {
            const triggerBtn = event.relatedTarget;
            const id = triggerBtn ? triggerBtn.getAttribute('data-id') : '';
            const ten = triggerBtn ? triggerBtn.getAttribute('data-ten') : '';

            const editMaLoaiXe = document.getElementById('editMaLoaiXe');
            const editTenLoaiXe = document.getElementById('editTenLoaiXe');
            if (editMaLoaiXe) editMaLoaiXe.value = id || '';
            if (editTenLoaiXe) editTenLoaiXe.value = ten || '';
        });
    }

    const deleteModalEl = document.getElementById('deleteLoai');
    if (deleteModalEl) {
        deleteModalEl.addEventListener('show.bs.modal', function (event) {
            const triggerBtn = event.relatedTarget;
            const id = triggerBtn ? triggerBtn.getAttribute('data-id') : '';
            const ten = triggerBtn ? triggerBtn.getAttribute('data-ten') : '';

            const deleteId = document.getElementById('deleteLoaiId');
            const deleteTenLoaiXeText = document.getElementById('deleteTenLoaiXeText');
            if (deleteId) deleteId.value = id || '';
            if (deleteTenLoaiXeText) deleteTenLoaiXeText.textContent = ten || '';
        });
    }

    // Fallback: hide alert if Bootstrap alert dismiss is not available
    document
        .querySelectorAll('.loaiXe-alert [data-bs-dismiss="alert"]')
        .forEach(function (btn) {
            btn.addEventListener('click', function () {
                if (!window.bootstrap || !window.bootstrap.Alert) {
                    const alertEl = btn.closest('.alert');
                    if (alertEl) alertEl.style.display = 'none';
                }
            });
        });
});
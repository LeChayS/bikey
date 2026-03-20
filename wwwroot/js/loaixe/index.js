let currentDeleteId = null;

function openCreateModal() {
    document.getElementById('createModal').style.display = 'flex';
    document.getElementById('createForm').reset();
}

function openEditModal(id, tenLoaiXe) {
    document.getElementById('editMaLoaiXe').value = id;
    document.getElementById('editTenLoaiXe').value = tenLoaiXe;
    document.getElementById('editModal').style.display = 'flex';
}

function openDeleteModal(id, tenLoaiXe) {
    currentDeleteId = id;
    document.getElementById('deleteConfirmText').textContent = `Bạn có chắc chắn muốn xóa loại xe "${tenLoaiXe}"?`;
    document.getElementById('deleteModal').style.display = 'flex';
}

function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
    clearValidationErrors(modalId.replace('Modal', ''));
}

function clearValidationErrors(type) {
    const errorElements = document.querySelectorAll(`#${type}TenLoaiXeError`);
    errorElements.forEach(el => el.textContent = '');
}

function showAlert(message, type) {
    const alertContainer = document.getElementById('alert-container');
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type}`;
    alertDiv.textContent = message;
    alertContainer.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, 5000);
}

// Create form submission
document.getElementById('createForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const data = {
        TenLoaiXe: formData.get('TenLoaiXe')
    };

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('/LoaiXe/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        console.log('Create response:', result); // Debug log

        if (result.success) {
            closeModal('createModal');
            showAlert(result.message || 'Thêm loại xe thành công!', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            if (result.errors) {
                console.log('Validation errors:', result.errors); // Debug log
                // Hiển thị lỗi validation
                Object.keys(result.errors).forEach(key => {
                    const errorElement = document.getElementById(`create${key}Error`);
                    if (errorElement) {
                        errorElement.textContent = result.errors[key][0];
                    }
                });
            } else {
                showAlert(result.message || 'Có lỗi xảy ra khi thêm loại xe!', 'danger');
            }
        }
    } catch (error) {
        showAlert('Có lỗi xảy ra khi thêm loại xe!', 'danger');
    }
});

// Edit form submission
document.getElementById('editForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const data = {
        MaLoaiXe: parseInt(formData.get('MaLoaiXe')),
        TenLoaiXe: formData.get('TenLoaiXe')
    };
    console.log('Edit data being sent:', data); // Debug log

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch(`/LoaiXe/Edit/${data.MaLoaiXe}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        console.log('Edit response:', result); // Debug log

        if (result.success) {
            closeModal('editModal');
            showAlert(result.message || 'Cập nhật loại xe thành công!', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            if (result.errors) {
                console.log('Edit validation errors:', result.errors); // Debug log
                // Hiển thị lỗi validation
                Object.keys(result.errors).forEach(key => {
                    const errorElement = document.getElementById(`edit${key}Error`);
                    if (errorElement) {
                        errorElement.textContent = result.errors[key][0];
                    }
                });
            } else {
                showAlert(result.message || 'Có lỗi xảy ra khi cập nhật loại xe!', 'danger');
            }
        }
    } catch (error) {
        showAlert('Có lỗi xảy ra khi cập nhật loại xe!', 'danger');
    }
});

// Delete confirmation
async function confirmDelete() {
    if (!currentDeleteId) return;

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch(`/LoaiXe/Delete/${currentDeleteId}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            closeModal('deleteModal');
            showAlert(result.message || 'Xóa loại xe thành công!', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            closeModal('deleteModal');
            showAlert(result.message || 'Có lỗi xảy ra khi xóa loại xe!', 'danger');
        }
    } catch (error) {
        closeModal('deleteModal');
        showAlert('Có lỗi xảy ra khi xóa loại xe!', 'danger');
    }
}

// Close modal when clicking outside
document.querySelectorAll('.modal').forEach(modal => {
    modal.addEventListener('click', function (e) {
        if (e.target === this) {
            this.style.display = 'none';
        }
    });
});
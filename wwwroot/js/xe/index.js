// Biến để lưu timeout cho debounce
let filterTimeout;

$(document).ready(function () {
    // Prevent any accidental clicks on disabled buttons
    $(document).on('click', '.btn-disabled', function (e) {
        e.preventDefault();
        e.stopPropagation();
        return false;
    });

    // Thiết lập event listeners cho lọc real-time
    setupFilterEvents();
    setupDeleteEvents();

    // Kiểm tra trạng thái filter ban đầu và hiển thị/ẩn button xóa lọc
    checkFilterStatus();
});

// Thiết lập các event listeners cho bộ lọc
function setupFilterEvents() {
    // Lọc real-time khi thay đổi input tìm kiếm (với debounce)
    $('#searchString').on('input', function () {
        clearTimeout(filterTimeout);
        filterTimeout = setTimeout(function () {
            filterXe();
        }, 300); // Delay 300ms để tránh gọi quá nhiều
    });

    // Lọc real-time khi thay đổi dropdown
    $('#loaiXe, #hangXe, #trangThai').on('change', function () {
        filterXe();
    });

    // Nút lọc
    $('#filterBtn').on('click', function () {
        filterXe();
    });

    // Nút xóa lọc
    $('#clearFilterBtn').on('click', function () {
        clearFilters();
    });
}

function setupDeleteEvents() {
    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.textContent = value || '-';
    };

    $(document).on('click', '.delete-xe-btn', function () {
        const $btn = $(this);
        const xeId = $btn.data('id');

        setText('deleteXeMaXe', $btn.data('ma-xe'));
        setText('deleteXeTenXe', $btn.data('ten-xe'));
        setText('deleteXeBienSo', $btn.data('bien-so'));
        setText('deleteXeHangXe', $btn.data('hang-xe'));
        setText('deleteXeDongXe', $btn.data('dong-xe'));
        setText('deleteXeLoaiXe', $btn.data('loai-xe'));
        setText('deleteXeGiaThue', $btn.data('gia-thue'));
        setText('deleteXeTrangThai', $btn.data('trang-thai'));

        const confirmBtn = document.getElementById('confirmDeleteXeBtn');
        if (confirmBtn) {
            confirmBtn.setAttribute('data-delete-url', `/Xe/Delete/${xeId}`);
        }

        const historyBadge = document.getElementById('deleteXeHistoryBadge');
        if (historyBadge) {
            historyBadge.style.display = 'none';
        }

        fetch(`/Xe/CheckXeContractHistory?xeId=${encodeURIComponent(xeId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (!data || data.success !== true) return;
                if (historyBadge) {
                    historyBadge.style.display = data.hasContracts ? 'inline-flex' : 'none';
                }
            })
            .catch(error => {
                console.error('Error checking contract history:', error);
                if (historyBadge) {
                    historyBadge.style.display = 'none';
                }
            });

        const deleteModal = document.getElementById('deleteXe');
        if (deleteModal && window.bootstrap) {
            bootstrap.Modal.getOrCreateInstance(deleteModal).show();
        }
    });

    $('#confirmDeleteXeBtn').on('click', function () {
        const deleteUrl = this.getAttribute('data-delete-url');
        if (deleteUrl) {
            window.location.href = deleteUrl;
        }
    });
}

// Hàm lọc xe real-time
function filterXe() {
    // Lấy giá trị từ các input
    const searchString = $('#searchString').val();
    const loaiXe = $('#loaiXe').val();
    const hangXe = $('#hangXe').val();
    const trangThai = $('#trangThai').val();
    // showDeleted lấy từ query string để JS chạy độc lập với Razor
    const showDeleted = new URLSearchParams(window.location.search).get('showDeleted') === 'true';

    // Kiểm tra xem có filter nào được áp dụng không
    const hasFilters = searchString || loaiXe || hangXe || trangThai;

    // Hiển thị/ẩn button xóa lọc dựa trên việc có filter hay không
    if (hasFilters) {
        $('#clearFilterBtn').show();
    } else {
        $('#clearFilterBtn').hide();
    }

    // Hiển thị loading
    $('#xeTableContainer').html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Đang tải...</span></div><br><small class="text-muted">Đang lọc dữ liệu...</small></div>');

    // Gọi AJAX để lọc
    $.ajax({
        url: '/Xe/FilterXe',
        type: 'GET',
        data: {
            searchString: searchString,
            loaiXe: loaiXe,
            hangXe: hangXe,
            trangThai: trangThai,
            showDeleted: showDeleted
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (result) {
            $('#xeTableContainer').html(result);

            // Cập nhật URL để lưu trạng thái filter
            updateUrl(searchString, loaiXe, hangXe, trangThai, showDeleted);
        },
        error: function (xhr, status, error) {
            $('#xeTableContainer').html('<div class="text-center py-4 text-danger"><i class="bi bi-exclamation-triangle"></i><br>Có lỗi xảy ra khi lọc dữ liệu</div>');
        }
    });
}

// Hàm kiểm tra trạng thái filter và hiển thị/ẩn button xóa lọc
function checkFilterStatus() {
    const searchString = $('#searchString').val();
    const loaiXe = $('#loaiXe').val();
    const hangXe = $('#hangXe').val();
    const trangThai = $('#trangThai').val();

    const hasFilters = searchString || loaiXe || hangXe || trangThai;

    if (hasFilters) {
        $('#clearFilterBtn').show();
    } else {
        $('#clearFilterBtn').hide();
    }
}

// Hàm xóa tất cả bộ lọc
function clearFilters() {
    $('#searchString').val('');
    $('#loaiXe').val('');
    $('#hangXe').val('');
    $('#trangThai').val('');

    // Ẩn button xóa lọc
    $('#clearFilterBtn').hide();

    // Lọc lại với dữ liệu rỗng
    filterXe();
}

// Hàm cập nhật URL để lưu trạng thái filter
function updateUrl(searchString, loaiXe, hangXe, trangThai, showDeleted) {
    const params = new URLSearchParams();
    if (searchString) params.append('searchString', searchString);
    if (loaiXe) params.append('loaiXe', loaiXe);
    if (hangXe) params.append('hangXe', hangXe);
    if (trangThai) params.append('trangThai', trangThai);
    if (showDeleted) params.append('showDeleted', showDeleted);

    const newUrl = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
    window.history.pushState({}, '', newUrl);
}

// ===== XE DETAILS MODAL FUNCTIONS =====

// Hiển thị modal chi tiết xe
function showXeDetails(xeId) {
    const modalEl = document.getElementById('detailXe');
    if (modalEl && window.bootstrap) {
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }

    fetch(`/Xe/GetXeDetails?xeId=${encodeURIComponent(xeId)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                populateXeDetails(data.xe);
            } else {
                toastr.error(data.message || 'Có lỗi xảy ra khi tải thông tin xe');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            toastr.error('Có lỗi xảy ra khi tải thông tin xe');
        });
}

// Đóng modal chi tiết xe
function closeXeDetailsModal() {
    const modalEl = document.getElementById('detailXe');
    if (!modalEl || !window.bootstrap) return;
    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
}

// Điền dữ liệu vào modal
function populateXeDetails(xe) {
    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.textContent = value ?? '-';
    };

    const resolveXeImageSrc = (tenFile) => {
        if (!tenFile) return null;
        if (tenFile.startsWith('http://') || tenFile.startsWith('https://')) return tenFile;
        if (tenFile.startsWith('/')) return tenFile;
        if (tenFile.toLowerCase().startsWith('images/')) return `/${tenFile}`;
        // Fallback nếu TenFile chỉ là tên file
        return `/images/xe/${tenFile}`;
    };

    // Hình ảnh chính
    const imageContainer = document.getElementById('xeImageContainer');
    if (imageContainer) {
        const imgSrc = resolveXeImageSrc(xe.hinhAnhHienThi);
        imageContainer.innerHTML = imgSrc
            ? `<img src="${imgSrc}" class="xe-details-main-image" alt="${xe.tenXe || ''}" />`
            : `<div class="xe-details-image-placeholder"><i class="bi bi-image"></i></div>`;
    }

    // Thông tin cơ bản
    setText('xeMaXe', `#XE${String(xe.maXe).padStart(3, '0')}`);
    setText('xeTenXe', xe.tenXe || '-');
    setText('xeBienSo', xe.bienSoXe || '-');
    setText('xeHangXe', xe.hangXe || '-');
    setText('xeDongXe', xe.dongXe || '-');
    setText('xeLoaiXe', xe.loaiXe || '-');

    // Thông tin tài chính
    setText('xeGiaThue', xe.giaThue ? `${Number(xe.giaThue).toLocaleString('vi-VN')}đ` : '-');
    setText('xeGiaTriXe', xe.giaTriXe ? `${Number(xe.giaTriXe).toLocaleString('vi-VN')}đ` : '-');
    setText('xeTongChiPhi', xe.tongChiPhi ? `${Number(xe.tongChiPhi).toLocaleString('vi-VN')}đ` : '-');
    setText('xeChiPhiSuaChua', xe.chiPhiSuaChua ? `${Number(xe.chiPhiSuaChua).toLocaleString('vi-VN')}đ` : '-');

    // Trạng thái
    const statusContainer = document.getElementById('xeTrangThaiContainer');
    if (statusContainer) {
        const statusClass = getStatusClass(xe.trangThai);
        statusContainer.innerHTML = `<span class="xe-details-status ${statusClass}">${xe.trangThai}</span>`;
    }
}

// Lấy class CSS cho trạng thái
function getStatusClass(trangThai) {
    switch (trangThai) {
        case 'Sẵn sàng': return 'san-sang';
        case 'Đang thuê': return 'dang-thue';
        case 'Bảo trì': return 'bao-tri';
        case 'Hư hỏng': return 'hu-hong';
        case 'Mất': return 'mat';
        case 'Đã xóa': return 'da-xoa';
        default: return 'san-sang';
    }
}

// Đóng modal khi click bên ngoài
document.addEventListener('click', function (e) {
    const modalEl = document.getElementById('detailXe');
    if (modalEl && e.target === modalEl) closeXeDetailsModal();
});

// Đóng modal khi nhấn ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeXeDetailsModal();
    }
});


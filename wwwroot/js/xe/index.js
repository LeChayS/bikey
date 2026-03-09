// Hiển thị thông báo TempData nếu có
@if (TempData["Success"] != null) {
    <text>
        toastr.success('@TempData["Success"]');
    </text>
}
@if (TempData["Error"] != null) {
    <text>
        toastr.error('@TempData["Error"]');
    </text>
}

// Biến để lưu timeout cho debounce
let filterTimeout;

// Biến để lưu ID xe hiện tại trong modal
let currentXeId = null;

$(document).ready(function () {
    // Prevent any accidental clicks on disabled buttons
    $('.btn-disabled').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        return false;
    });

    // Close modal when clicking outside
    $(document).on('click', '.custom-modal-overlay', function (e) {
        if (e.target === this) {
            closeCustomModal();
        }
    });

    // Close modal with Escape key
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $('.custom-modal-overlay').hasClass('show')) {
            closeCustomModal();
        }
    });

    // Thiết lập event listeners cho lọc real-time
    setupFilterEvents();

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

// Hàm lọc xe real-time
function filterXe() {
    // Lấy giá trị từ các input
    const searchString = $('#searchString').val();
    const loaiXe = $('#loaiXe').val();
    const hangXe = $('#hangXe').val();
    const trangThai = $('#trangThai').val();
    const showDeleted = @(ViewBag.ShowDeleted == true ? "true" : "false");

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
        url: '@Url.Action("FilterXe", "Xe")',
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
    const modal = document.getElementById('xeDetailsModal');
    const loading = document.getElementById('xeDetailsLoading');
    const content = document.getElementById('xeDetailsContent');

    // Lưu ID xe hiện tại
    currentXeId = xeId;

    // Hiển thị modal và loading
    modal.classList.add('show');
    loading.style.display = 'block';
    content.style.display = 'none';

    // Ẩn phần lịch sử hợp đồng
    document.getElementById('xeContractHistory').style.display = 'none';

    // Gọi API để lấy dữ liệu xe
    fetch(`/Xe/GetXeDetails/${xeId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                populateXeDetails(data.xe);
                loading.style.display = 'none';
                content.style.display = 'block';

                // Load lịch sử hợp đồng
                loadXeContractHistory(xeId);
            } else {
                toastr.error(data.message || 'Có lỗi xảy ra khi tải thông tin xe');
                closeXeDetailsModal();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            toastr.error('Có lỗi xảy ra khi tải thông tin xe');
            closeXeDetailsModal();
        });
}

// Đóng modal chi tiết xe
function closeXeDetailsModal() {
    const modal = document.getElementById('xeDetailsModal');
    modal.classList.remove('show');
}

// Điền dữ liệu vào modal
function populateXeDetails(xe) {
    // Cập nhật tiêu đề
    document.getElementById('modalTitle').textContent = `Chi tiết xe - ${xe.tenXe}`;

    // Cập nhật hình ảnh
    const imageContainer = document.getElementById('xeImageContainer');
    if (xe.hinhAnhHienThi) {
        imageContainer.innerHTML = `<img src="/images/xe/${xe.hinhAnhHienThi}" class="xe-details-main-image" alt="${xe.tenXe}" />`;
    } else {
        imageContainer.innerHTML = `<div class="xe-details-image-placeholder"><i class="bi bi-image"></i></div>`;
    }

    // Cập nhật thông tin cơ bản
    document.getElementById('xeMaXe').textContent = `#XE${xe.maXe.toString().padStart(3, '0')}`;
    document.getElementById('xeTenXe').textContent = xe.tenXe || '-';
    document.getElementById('xeBienSo').textContent = xe.bienSoXe || '-';
    document.getElementById('xeHangXe').textContent = xe.hangXe || '-';
    document.getElementById('xeDongXe').textContent = xe.dongXe || '-';
    document.getElementById('xeLoaiXe').textContent = xe.loaiXe || '-';

    // Cập nhật thông tin tài chính
    document.getElementById('xeGiaThue').textContent = xe.giaThue ? `${xe.giaThue.toLocaleString('vi-VN')}đ` : '-';
    document.getElementById('xeGiaTriXe').textContent = xe.giaTriXe ? `${xe.giaTriXe.toLocaleString('vi-VN')}đ` : '-';
    document.getElementById('xeTongChiPhi').textContent = xe.tongChiPhi ? `${xe.tongChiPhi.toLocaleString('vi-VN')}đ` : '-';
    document.getElementById('xeChiPhiSuaChua').textContent = xe.chiPhiSuaChua ? `${xe.chiPhiSuaChua.toLocaleString('vi-VN')}đ` : '-';

    // Cập nhật trạng thái
    const statusContainer = document.getElementById('xeTrangThaiContainer');
    const statusClass = getStatusClass(xe.trangThai);
    statusContainer.innerHTML = `<span class="xe-details-status ${statusClass}">${xe.trangThai}</span>`;

    // Cập nhật thống kê
    document.getElementById('xeSoHopDong').textContent = xe.soHopDong || 0;
    document.getElementById('xeSoHinhAnh').textContent = xe.soHinhAnh || 0;

    // Cập nhật thông tin sự cố (nếu có)
    const suCoCard = document.getElementById('xeSuCoCard');
    if (xe.ngayGapSuCo || xe.moTaThietHai) {
        document.getElementById('xeNgaySuCo').textContent = xe.ngayGapSuCo ? new Date(xe.ngayGapSuCo).toLocaleDateString('vi-VN') : '-';
        document.getElementById('xeMoTaThietHai').textContent = xe.moTaThietHai || '-';
        suCoCard.style.display = 'block';
    } else {
        suCoCard.style.display = 'none';
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
    const modal = document.getElementById('xeDetailsModal');
    if (e.target === modal) {
        closeXeDetailsModal();
    }
});

// Đóng modal khi nhấn ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('xeDetailsModal');
        if (modal.classList.contains('show')) {
            closeXeDetailsModal();
        }
    }
});

// Load lịch sử hợp đồng
function loadXeContractHistory(xeId) {
    fetch(`/Xe/GetXeLichSuHopDong/${xeId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                populateContractHistory(data.thongKe, data.lichSuHopDong);
                document.getElementById('xeContractHistory').style.display = 'block';
            } else {
                console.error('Error loading contract history:', data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

// Điền dữ liệu lịch sử hợp đồng
function populateContractHistory(thongKe, lichSuHopDong) {
    // Cập nhật thống kê
    document.getElementById('xeTongHopDong').textContent = thongKe.tongHopDong;
    document.getElementById('xeDaHoanThanh').textContent = thongKe.daHoanThanh;
    document.getElementById('xeDangThue').textContent = thongKe.dangThue;
    document.getElementById('xeTongDoanhThu').textContent = thongKe.tongDoanhThu.toLocaleString('vi-VN') + 'đ';

    // Tạo bảng lịch sử hợp đồng
    const tableContainer = document.getElementById('xeContractTable');

    if (lichSuHopDong.length === 0) {
        tableContainer.innerHTML = `
                    <div class="xe-details-contract-empty">
                        <i class="bi bi-inbox"></i>
                        <h5>Không có hợp đồng nào</h5>
                        <p>Chưa có hợp đồng nào được tạo cho xe này.</p>
                    </div>
                `;
        return;
    }

    let tableHTML = `
                <table>
                    <thead>
                        <tr>
                            <th>Mã HĐ</th>
                            <th>Khách hàng</th>
                            <th>Số điện thoại</th>
                            <th>Ngày nhận</th>
                            <th>Ngày trả</th>
                            <th>Số ngày</th>
                            <th>Thành tiền</th>
                            <th>Trạng thái</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

    lichSuHopDong.forEach(hopDong => {
        const ngayNhan = new Date(hopDong.ngayNhanXe);
        const ngayTra = hopDong.ngayTraXeThucTe ? new Date(hopDong.ngayTraXeThucTe) : null;
        const soNgay = ngayTra ?
            Math.ceil((ngayTra - ngayNhan) / (1000 * 60 * 60 * 24)) + 1 :
            Math.ceil((new Date() - ngayNhan) / (1000 * 60 * 60 * 24)) + 1;

        const trangThai = ngayTra ? 'hoan-thanh' : 'dang-thue';
        const trangThaiText = ngayTra ? 'Đã hoàn thành' : 'Đang thuê';

        // Kiểm tra xem thời gian có phải là 00:00 không (thời gian mặc định)
        const isDefaultTime = (date) => {
            return date.getHours() === 0 && date.getMinutes() === 0;
        };

        const formatDateTime = (date) => {
            const dateStr = date.toLocaleDateString('vi-VN');
            const timeStr = isDefaultTime(date) ? '' : date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
            return timeStr ? `${dateStr} ${timeStr}` : dateStr;
        };

        tableHTML += `
                    <tr>
                        <td><strong>#HD${hopDong.maHopDong.toString().padStart(3, '0')}</strong></td>
                        <td>
                            <strong>${hopDong.hoTenKhach}</strong>
                            ${hopDong.soCCCD ? `<br><small class="text-muted">CCCD: ${hopDong.soCCCD}</small>` : ''}
                        </td>
                        <td>${hopDong.soDienThoai}</td>
                        <td>${formatDateTime(ngayNhan)}</td>
                        <td>${ngayTra ? formatDateTime(ngayTra) : '<span class="text-muted">Chưa trả</span>'}</td>
                        <td>${soNgay} ngày</td>
                        <td class="text-danger"><strong>${hopDong.thanhTienTinhToan.toLocaleString('vi-VN')}đ</strong></td>
                        <td><span class="xe-details-contract-status ${trangThai}">${trangThaiText}</span></td>
                        <td>
                            <div class="xe-details-contract-actions">
                                <a href="/QuanLyHopDong/ChiTiet/${hopDong.maHopDong}" class="btn btn-info btn-sm" title="Xem chi tiết">
                                    <i class="bi bi-eye"></i>
                                </a>
                                ${hopDong.coHoaDon ? `<a href="/QuanLyHoaDon/ChiTiet/${hopDong.maHoaDon}" class="btn btn-success btn-sm" title="Xem hóa đơn"><i class="bi bi-receipt"></i></a>` : ''}
                            </div>
                        </td>
                    </tr>
                `;
    });

    tableHTML += `
                    </tbody>
                </table>
            `;

    tableContainer.innerHTML = tableHTML;
}

// Chuyển đến trang lịch sử hợp đồng
function viewContractHistory() {
    if (currentXeId) {
        // Đóng modal trước
        closeXeDetailsModal();

        // Chuyển đến trang lịch sử hợp đồng
        window.location.href = `/Xe/LichSuHopDong/${currentXeId}`;
    } else {
        toastr.error('Không tìm thấy thông tin xe');
    }
}


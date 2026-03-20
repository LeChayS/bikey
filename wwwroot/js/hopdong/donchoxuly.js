$(document).ready(function () {
    console.log('Page loaded - jQuery version:', $.fn.jquery);

    // Khởi tạo toastr nếu chưa có
    if (typeof toastr === 'undefined') {
        window.toastr = {
            success: function (msg) { alert('Success: ' + msg); },
            error: function (msg) { alert('Error: ' + msg); }
        };
    }

    // Đóng modal khi click bên ngoài
    $('.custom-modal').on('click', function (e) {
        if (e.target === this) {
            closeModal($(this).attr('id'));
        }
    });

    // Đóng modal khi nhấn ESC
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllModals();
        }
    });
});

function showModal(modalId) {
    document.getElementById(modalId).classList.add('show');
    document.body.style.overflow = 'hidden';
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('show');
    document.body.style.overflow = '';
}

function closeAllModals() {
    $('.custom-modal').removeClass('show');
    document.body.style.overflow = '';
}

function huyDon(maDatCho) {
    console.log('Huy don clicked:', maDatCho);

    // Set giá trị
    $('#maDatChoHuy').val(maDatCho);
    $('#lyDoHuy').val('');

    // Hiển thị modal
    showModal('huyDonModal');
}

function xacNhanHuyDon() {
    var maDatCho = $('#maDatChoHuy').val();
    var lyDoHuy = $('#lyDoHuy').val().trim();

    if (!lyDoHuy) {
        alert('Vui lòng nhập lý do hủy!');
        return;
    }

    // Hiển thị loading
    var btn = $('.custom-modal-footer .custom-btn-danger');
    var originalText = btn.html();
    btn.html('<span class="custom-spinner"></span> Đang xử lý...').prop('disabled', true);

    $.ajax({
        url: '@Url.Action("HuyDon", "QuanLyHopDong")',
        type: 'POST',
        data: {
            id: maDatCho,
            lyDoHuy: lyDoHuy,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(function () {
                    location.reload();
                }, 1000);
            } else {
                toastr.error(response.message);
                btn.html(originalText).prop('disabled', false);
            }
        },
        error: function (xhr, status, error) {
            toastr.error('Có lỗi xảy ra: ' + error);
            btn.html(originalText).prop('disabled', false);
        },
        complete: function () {
            closeModal('huyDonModal');
        }
    });
}

function xemChiTiet(maDatCho) {
    console.log('Xem chi tiet clicked:', maDatCho);

    // Hiển thị loading
    $('#chiTietContent').html('<div class="text-center p-4"><div class="custom-spinner" style="width: 40px; height: 40px; border-width: 4px;"></div><p class="mt-2">Đang tải...</p></div>');

    // Hiển thị modal
    showModal('chiTietModal');

    // Load nội dung
    $.ajax({
        url: '@Url.Action("ChiTietDonCho", "QuanLyHopDong")',
        type: 'GET',
        data: { id: maDatCho },
        success: function (data) {
            $('#chiTietContent').html(data);
        },
        error: function (xhr, status, error) {
            $('#chiTietContent').html('<div class="alert alert-danger">Không thể tải chi tiết. Lỗi: ' + error + '</div>');
        }
    });
}
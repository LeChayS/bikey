// Biến global để lưu chart instances
let revenueChart, topVehiclesChart, vehicleTypesChart, contractStatusChart, expensesChart, thietHaiChart;
let currentRevenueChartType = 'bar'; // Loại biểu đồ doanh thu hiện tại

// Biểu đồ doanh thu (Horizontal Bar Chart với 2 màu)
const revenueCtx = document.getElementById('revenueChart').getContext('2d');
const revenueData = @Html.Raw(Json.Serialize(Model.BieuDoDoanhThu.Select(x => x.Value).ToList()));
const revenueLabels = @Html.Raw(Json.Serialize(Model.BieuDoDoanhThu.Select(x => x.Label).ToList()));

// Đảm bảo có dữ liệu cho tất cả các ngày
console.log('Revenue Data:', revenueData);
console.log('Revenue Labels:', revenueLabels);

// Kiểm tra xem có dữ liệu doanh thu thực sự không
const hasRevenueData = revenueData.some(value => value > 0);
const maxRevenue = Math.max(...revenueData, 1); // Đảm bảo maxRevenue > 0

if (hasRevenueData) {
    revenueChart = new Chart(revenueCtx, {
        type: 'bar',
        data: {
            labels: @Html.Raw(Json.Serialize(Model.BieuDoDoanhThu.Select(x => x.Label).ToList())),
            datasets: [
                {
                    label: 'Doanh thu',
                    data: revenueData,
                    backgroundColor: 'rgba(54, 162, 235, 0.9)',
                    borderColor: 'rgb(54, 162, 235)',
                    borderWidth: 0,
                    borderRadius: {
                        topLeft: 15,
                        topRight: 15,
                        bottomLeft: 15,
                        bottomRight: 15
                    },
                    barThickness: function () {
                        // Tự động điều chỉnh độ dày thanh dựa trên số lượng dữ liệu
                        const dataCount = revenueData.length;
                        if (dataCount <= 7) return 35; // Ít dữ liệu - thanh dày
                        if (dataCount <= 12) return 25; // Trung bình
                        if (dataCount <= 30) return 20; // Nhiều dữ liệu - thanh mỏng
                        return 15; // Rất nhiều dữ liệu - thanh rất mỏng
                    }()
                }
            ]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Doanh thu: ' + context.parsed.x.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                },
                y: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            },
            elements: {
                bar: {
                    barPercentage: 0.4,
                    categoryPercentage: 0.6
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu doanh thu
    document.getElementById('revenueChart').style.display = 'none';
    document.getElementById('revenueChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu doanh thu</div>';
}

// Biểu đồ Top 5 xe được thuê nhiều nhất (Bar Chart)
const topVehiclesCtx = document.getElementById('topVehiclesChart').getContext('2d');
const topVehicleLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.TopXeThueNhieu.Select(x => x.TenXe + " (" + x.BienSo + ")").ToList()));
const topVehicleData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.TopXeThueNhieu.Select(x => x.SoLanThue).ToList()));

if (topVehicleLabels && topVehicleLabels.length > 0 && topVehicleData.some(value => value > 0)) {
    topVehiclesChart = new Chart(topVehiclesCtx, {
        type: 'bar',
        data: {
            labels: topVehicleLabels,
            datasets: [{
                label: 'Số lần thuê',
                data: topVehicleData,
                backgroundColor: 'rgba(75, 192, 192, 0.8)',
                borderColor: 'rgb(75, 192, 192)',
                borderWidth: 2,
                borderRadius: 8,
                barThickness: function () {
                    // Tự động điều chỉnh độ dày thanh dựa trên số lượng dữ liệu
                    const dataCount = topVehicleLabels.length;
                    if (dataCount <= 3) return 40; // Ít dữ liệu - thanh dày
                    if (dataCount <= 5) return 30; // Trung bình
                    return 25; // Nhiều dữ liệu - thanh mỏng
                }()
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Số lần thuê: ' + context.parsed.y + ' lần';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 11
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            },
            elements: {
                bar: {
                    barPercentage: 0.4,
                    categoryPercentage: 0.6
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu xe được thuê
    document.getElementById('topVehiclesChart').style.display = 'none';
    document.getElementById('topVehiclesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu xe được thuê</div>';
}

// Biểu đồ thống kê loại xe (Horizontal Bar Chart với 2 màu)
const vehicleTypesCtx = document.getElementById('vehicleTypesChart').getContext('2d');
const vehicleTypeLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.BieuDoLoaiXe.Select(x => x.Label).ToList()));
const vehicleTypeData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.BieuDoLoaiXe.Select(x => x.Value).ToList()));

if (vehicleTypeLabels && vehicleTypeLabels.length > 0 && vehicleTypeData.some(value => value > 0)) {
    const maxVehicles = Math.max(...vehicleTypeData);

    vehicleTypesChart = new Chart(vehicleTypesCtx, {
        type: 'bar',
        data: {
            labels: vehicleTypeLabels,
            datasets: [
                {
                    label: 'Số lượng xe',
                    data: vehicleTypeData,
                    backgroundColor: 'rgba(54, 162, 235, 0.9)',
                    borderColor: 'rgb(54, 162, 235)',
                    borderWidth: 0,
                    borderRadius: {
                        topLeft: 15,
                        topRight: 15,
                        bottomLeft: 15,
                        bottomRight: 15
                    },
                    barThickness: function () {
                        // Tự động điều chỉnh độ dày thanh dựa trên số lượng dữ liệu
                        const dataCount = vehicleTypeLabels.length;
                        if (dataCount <= 3) return 35; // Ít dữ liệu - thanh dày
                        if (dataCount <= 5) return 25; // Trung bình
                        return 20; // Nhiều dữ liệu - thanh mỏng
                    }()
                }
            ]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Số lượng: ' + context.parsed.x + ' xe';
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    max: maxVehicles,
                    ticks: {
                        stepSize: 1
                    }
                },
                y: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            },
            elements: {
                bar: {
                    barPercentage: 0.4,
                    categoryPercentage: 0.6
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu loại xe
    document.getElementById('vehicleTypesChart').style.display = 'none';
    document.getElementById('vehicleTypesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu loại xe</div>';
}

// Biểu đồ thống kê hợp đồng theo trạng thái (Line Chart)
const contractStatusCtx = document.getElementById('contractStatusChart').getContext('2d');
const contractStatusLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.BieuDoHopDongTrangThai.Select(x => x.Label).ToList()));
const contractStatusData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.BieuDoHopDongTrangThai.Select(x => x.Value).ToList()));

if (contractStatusLabels && contractStatusLabels.length > 0 && contractStatusData.some(value => value > 0)) {
    // Màu sắc cho từng trạng thái
    const statusColors = {
        'Đang thuê': 'rgba(255, 193, 7, 1)',      // Vàng
        'Hoàn thành': 'rgba(40, 167, 69, 1)',     // Xanh lá
        'Hủy': 'rgba(220, 53, 69, 1)'             // Đỏ
    };

    const backgroundColor = contractStatusLabels.map(label => statusColors[label] || 'rgba(108, 117, 125, 1)');

    contractStatusChart = new Chart(contractStatusCtx, {
        type: 'line',
        data: {
            labels: contractStatusLabels,
            datasets: [{
                label: 'Số lượng hợp đồng',
                data: contractStatusData,
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: backgroundColor,
                pointBorderColor: backgroundColor,
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Số lượng: ' + context.parsed.y + ' hợp đồng';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        callback: function (value) {
                            return value + ' hợp đồng';
                        }
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 20,
                    bottom: 20
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu hợp đồng
    document.getElementById('contractStatusChart').style.display = 'none';
    document.getElementById('contractStatusChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu hợp đồng</div>';
}

// Biểu đồ chi tiêu (Line Chart)
const expensesCtx = document.getElementById('expensesChart').getContext('2d');
const expensesData = @Html.Raw(Json.Serialize(Model.BieuDoDoanhThu.Select(x => x.Value).ToList())); // Tạm thời dùng dữ liệu doanh thu, sẽ được cập nhật qua AJAX
const expensesLabels = @Html.Raw(Json.Serialize(Model.BieuDoDoanhThu.Select(x => x.Label).ToList()));

if (expensesLabels && expensesLabels.length > 0) {
    expensesChart = new Chart(expensesCtx, {
        type: 'line',
        data: {
            labels: expensesLabels,
            datasets: [{
                label: 'Chi tiêu',
                data: expensesData,
                borderColor: 'rgba(255, 99, 132, 1)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Chi tiêu: ' + context.parsed.y.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu chi tiêu
    document.getElementById('expensesChart').style.display = 'none';
    document.getElementById('expensesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu chi tiêu</div>';
}

// Biểu đồ thiệt hại (Line Chart)
const thietHaiCtx = document.getElementById('thietHaiChart').getContext('2d');
const thietHaiData = @Html.Raw(Json.Serialize(Model.BieuDoThietHai.Select(x => x.Value).ToList()));
const thietHaiLabels = @Html.Raw(Json.Serialize(Model.BieuDoThietHai.Select(x => x.Label).ToList()));

if (thietHaiLabels && thietHaiLabels.length > 0 && thietHaiData.some(value => value > 0)) {
    thietHaiChart = new Chart(thietHaiCtx, {
        type: 'line',
        data: {
            labels: thietHaiLabels,
            datasets: [{
                label: 'Tổng tiền thiệt hại',
                data: thietHaiData,
                borderColor: 'rgba(220, 53, 69, 1)',
                backgroundColor: 'rgba(220, 53, 69, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: 'rgba(220, 53, 69, 1)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Tổng tiền thiệt hại: ' + context.parsed.y.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            }
        }
    });
} else {
    // Hiển thị thông báo khi không có dữ liệu thiệt hại
    document.getElementById('thietHaiChart').style.display = 'none';
    document.getElementById('thietHaiChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu thiệt hại</div>';
}

// Function để tạo biểu đồ tuyến tính
function createLineChart(ctx, labels, data) {
    return new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: data,
                backgroundColor: 'transparent',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 2,
                fill: false,
                tension: 0,
                pointBackgroundColor: 'rgba(54, 162, 235, 1)',
                pointBorderColor: 'rgba(54, 162, 235, 1)',
                pointBorderWidth: 0,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Doanh thu: ' + context.parsed.y.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + 'đ';
                        }
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            layout: {
                padding: {
                    top: 30,
                    bottom: 30
                }
            }
        }
    });
}

// Function để tự động điều chỉnh chiều cao container
function adjustChartHeight(chartId, dataCount) {
    const container = document.getElementById(chartId + 'Container');
    if (container) {
        // Tính toán chiều cao dựa trên số lượng dữ liệu
        let height;
        if (dataCount <= 7) {
            height = 500; // Ít dữ liệu - chiều cao thấp
        } else if (dataCount <= 12) {
            height = 550; // Trung bình
        } else if (dataCount <= 30) {
            height = 600; // Nhiều dữ liệu - chiều cao cao
        } else {
            height = 800; // Rất nhiều dữ liệu - chiều cao rất cao
        }
        container.style.height = height + 'px';
    }
}

// Function để cập nhật tất cả charts
function updateCharts(filter, chartType = currentRevenueChartType) {
    showChartLoading(true);

    fetch(`/ThongKeBaoCao/GetChartData?filter=${filter}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateChartTitles(filter);

                // Kiểm tra xem có dữ liệu doanh thu thực sự không
                const hasRevenueData = data.doanhThu.data.some(value => value > 0);
                const hasChiTieuData = data.chiTieu && data.chiTieu.data.some(value => value > 0);
                const hasThietHaiData = data.thietHai && data.thietHai.data.some(value => value > 0);
                const hasTopXeData = data.topXe.data.some(value => value > 0);
                const hasLoaiXeData = data.loaiXe.data.some(value => value > 0);
                const hasContractStatusData = data.hopDongTrangThai && data.hopDongTrangThai.data.some(value => value > 0);

                if (revenueChart) {
                    // Kiểm tra nếu cần thay đổi loại biểu đồ
                    if (chartType !== currentRevenueChartType) {
                        // Hủy biểu đồ cũ
                        revenueChart.destroy();

                        // Tạo biểu đồ mới
                        const revenueCtx = document.getElementById('revenueChart').getContext('2d');
                        if (chartType === 'line') {
                            revenueChart = createLineChart(revenueCtx, data.doanhThu.labels, data.doanhThu.data);
                        } else {
                            // Tạo lại biểu đồ cột ngang
                            const maxRevenue = Math.max(...data.doanhThu.data, 1);
                            revenueChart = new Chart(revenueCtx, {
                                type: 'bar',
                                data: {
                                    labels: data.doanhThu.labels,
                                    datasets: [{
                                        label: 'Doanh thu',
                                        data: data.doanhThu.data,
                                        backgroundColor: 'rgba(54, 162, 235, 0.9)',
                                        borderColor: 'rgb(54, 162, 235)',
                                        borderWidth: 0,
                                        borderRadius: {
                                            topLeft: 15,
                                            topRight: 15,
                                            bottomLeft: 15,
                                            bottomRight: 15
                                        },
                                        barThickness: function () {
                                            const dataCount = data.doanhThu.labels.length;
                                            if (dataCount <= 7) return 35;
                                            if (dataCount <= 12) return 25;
                                            if (dataCount <= 30) return 20;
                                            return 15;
                                        }()
                                    }]
                                },
                                options: {
                                    indexAxis: 'y',
                                    responsive: true,
                                    maintainAspectRatio: false,
                                    plugins: {
                                        legend: { display: false },
                                        tooltip: {
                                            callbacks: {
                                                label: function (context) {
                                                    return 'Doanh thu: ' + context.parsed.x.toLocaleString('vi-VN') + 'đ';
                                                }
                                            }
                                        }
                                    },
                                    scales: {
                                        x: {
                                            beginAtZero: true,
                                            ticks: {
                                                callback: function (value) {
                                                    return value.toLocaleString('vi-VN') + 'đ';
                                                }
                                            }
                                        },
                                        y: {
                                            ticks: { font: { size: 12 } }
                                        }
                                    },
                                    layout: {
                                        padding: { top: 30, bottom: 30 }
                                    },
                                    elements: {
                                        bar: {
                                            barPercentage: 0.4,
                                            categoryPercentage: 0.6
                                        }
                                    }
                                }
                            });
                        }
                        currentRevenueChartType = chartType;
                    } else {
                        // Cập nhật dữ liệu cho biểu đồ hiện tại
                        revenueChart.data.labels = data.doanhThu.labels;
                        revenueChart.data.datasets[0].data = data.doanhThu.data;

                        // Tự động điều chỉnh độ dày thanh và chiều cao container (chỉ cho bar chart)
                        if (chartType === 'bar') {
                            const dataCount = data.doanhThu.labels.length;
                            let newBarThickness;

                            if (dataCount <= 7) {
                                newBarThickness = 35;
                            } else if (dataCount <= 12) {
                                newBarThickness = 25;
                            } else if (dataCount <= 30) {
                                newBarThickness = 20;
                            } else {
                                newBarThickness = 15;
                            }

                            revenueChart.data.datasets[0].barThickness = newBarThickness;
                        }

                        revenueChart.update();
                    }

                    // Điều chỉnh chiều cao container
                    adjustChartHeight('revenueChart', data.doanhThu.labels.length);
                } else if (revenueChart) {
                    // Hiển thị thông báo khi không có dữ liệu doanh thu
                    revenueChart.destroy();
                    document.getElementById('revenueChart').style.display = 'none';
                    document.getElementById('revenueChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu doanh thu</div>';
                }

                if (topVehiclesChart && hasTopXeData) {
                    topVehiclesChart.data.labels = data.topXe.labels;
                    topVehiclesChart.data.datasets[0].data = data.topXe.data;

                    // Tự động điều chỉnh độ dày thanh dựa trên số lượng dữ liệu
                    const dataCount = data.topXe.labels.length;
                    let newBarThickness;
                    if (dataCount <= 3) newBarThickness = 40;
                    else if (dataCount <= 5) newBarThickness = 30;
                    else newBarThickness = 25;

                    topVehiclesChart.data.datasets[0].barThickness = newBarThickness;
                    topVehiclesChart.update();
                } else if (topVehiclesChart) {
                    // Hiển thị thông báo khi không có dữ liệu xe được thuê
                    topVehiclesChart.destroy();
                    document.getElementById('topVehiclesChart').style.display = 'none';
                    document.getElementById('topVehiclesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu xe được thuê</div>';
                }

                if (vehicleTypesChart && hasLoaiXeData) {
                    vehicleTypesChart.data.labels = data.loaiXe.labels;
                    vehicleTypesChart.data.datasets[0].data = data.loaiXe.data;

                    // Tự động điều chỉnh độ dày thanh và chiều cao container
                    const dataCount = data.loaiXe.labels.length;
                    let newBarThickness;

                    if (dataCount <= 3) {
                        newBarThickness = 35;
                    } else if (dataCount <= 5) {
                        newBarThickness = 25;
                    } else {
                        newBarThickness = 20;
                    }

                    vehicleTypesChart.data.datasets[0].barThickness = newBarThickness;

                    // Điều chỉnh chiều cao container
                    adjustChartHeight('vehicleTypesChart', dataCount);

                    vehicleTypesChart.update();
                } else if (vehicleTypesChart) {
                    // Hiển thị thông báo khi không có dữ liệu loại xe
                    vehicleTypesChart.destroy();
                    document.getElementById('vehicleTypesChart').style.display = 'none';
                    document.getElementById('vehicleTypesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu loại xe</div>';
                }

                if (contractStatusChart && hasContractStatusData) {
                    contractStatusChart.data.labels = data.hopDongTrangThai.labels;
                    contractStatusChart.data.datasets[0].data = data.hopDongTrangThai.data;

                    // Cập nhật màu sắc cho từng trạng thái
                    const statusColors = {
                        'Đang thuê': 'rgba(255, 193, 7, 1)',      // Vàng
                        'Hoàn thành': 'rgba(40, 167, 69, 1)',     // Xanh lá
                        'Hủy': 'rgba(220, 53, 69, 1)'             // Đỏ
                    };

                    const backgroundColor = data.hopDongTrangThai.labels.map(label => statusColors[label] || 'rgba(108, 117, 125, 1)');

                    contractStatusChart.data.datasets[0].pointBackgroundColor = backgroundColor;
                    contractStatusChart.data.datasets[0].pointBorderColor = backgroundColor;

                    contractStatusChart.update();
                } else if (contractStatusChart) {
                    // Hiển thị thông báo khi không có dữ liệu hợp đồng
                    contractStatusChart.destroy();
                    document.getElementById('contractStatusChart').style.display = 'none';
                    document.getElementById('contractStatusChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu hợp đồng</div>';
                }

                // Cập nhật biểu đồ chi tiêu
                if (expensesChart && hasChiTieuData) {
                    expensesChart.data.labels = data.chiTieu.labels;
                    expensesChart.data.datasets[0].data = data.chiTieu.data;
                    expensesChart.update();
                } else if (expensesChart) {
                    // Hiển thị thông báo khi không có dữ liệu chi tiêu
                    expensesChart.destroy();
                    document.getElementById('expensesChart').style.display = 'none';
                    document.getElementById('expensesChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu chi tiêu</div>';
                }

                // Cập nhật biểu đồ thiệt hại
                if (thietHaiChart && hasThietHaiData) {
                    thietHaiChart.data.labels = data.thietHai.labels;
                    thietHaiChart.data.datasets[0].data = data.thietHai.data;
                    thietHaiChart.update();
                } else if (thietHaiChart) {
                    // Hiển thị thông báo khi không có dữ liệu thiệt hại
                    thietHaiChart.destroy();
                    document.getElementById('thietHaiChart').style.display = 'none';
                    document.getElementById('thietHaiChart').parentElement.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> Chưa có dữ liệu thiệt hại</div>';
                }
            } else {
                console.error('Error:', data.message);
            }
        })
        .catch(error => {
            console.error('Error fetching chart data:', error);
        })
        .finally(() => {
            showChartLoading(false);
        });
}

// Function để cập nhật thống kê tổng quan
function updateStatistics(filter) {
    fetch(`/ThongKeBaoCao/GetStatistics?filter=${filter}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Cập nhật giá trị
                document.getElementById('doanhThuValue').textContent = data.doanhThu.toLocaleString('vi-VN') + 'đ';
                document.getElementById('doanhThuGocValue').textContent = data.doanhThuGoc.toLocaleString('vi-VN') + 'đ';
                document.getElementById('tongChiTieuValue').textContent = data.tongChiTieu.toLocaleString('vi-VN') + 'đ';
                document.getElementById('tongThietHaiValue').textContent = data.tongThietHai.toLocaleString('vi-VN') + 'đ';
                document.getElementById('donDatValue').textContent = data.tongDonDat;
                document.getElementById('khachHangMoiValue').textContent = data.khachHangMoi;
                document.getElementById('xeDangThueValue').textContent = data.xeDangChoThue;

                // Cập nhật nhãn theo thời gian
                const periodLabels = {
                    'today': 'hôm nay',
                    '7days': '7 ngày gần nhất',
                    'week': 'tuần này',
                    'month': '30 ngày gần nhất',
                    'year': '12 tháng gần nhất'
                };

                const period = periodLabels[filter] || 'hôm nay';
                document.getElementById('doanhThuLabel').textContent = `Doanh thu ${period}`;
                document.getElementById('doanhThuGocLabel').textContent = `Doanh thu gốc ${period}`;
                document.getElementById('tongChiTieuLabel').textContent = `Tổng chi tiêu ${period}`;
                document.getElementById('tongThietHaiLabel').textContent = `Tổng tiền thiệt hại ${period}`;
                document.getElementById('donDatLabel').textContent = `Đơn đặt ${period}`;
                document.getElementById('khachHangMoiLabel').textContent = `Khách hàng mới ${period}`;
            }
        })
        .catch(error => {
            console.error('Error fetching statistics:', error);
        });
}

// Function để cập nhật titles
function updateChartTitles(filter) {
    const titles = {
        'today': 'hôm nay',
        '7days': '7 ngày gần nhất',
        'week': 'tuần này',
        'month': '30 ngày gần nhất',
        'year': '12 tháng gần nhất'
    };

    const period = titles[filter] || 'hôm nay';
    document.getElementById('revenueTitle').textContent = `Doanh thu theo thời gian ${period}`;
    document.getElementById('topVehiclesTitle').textContent = `Top 5 xe được thuê nhiều nhất ${period}`;
    document.getElementById('vehicleTypesTitle').textContent = `Thống kê loại xe theo danh mục`;
    document.getElementById('contractStatusTitle').textContent = `Thống kê hợp đồng theo trạng thái ${period}`;
    document.getElementById('expensesTitle').textContent = `Biểu đồ chi tiêu theo thời gian ${period}`;
    document.getElementById('thietHaiTitle').textContent = `Biểu đồ tổng tiền thiệt hại theo thời gian ${period}`;
}

// Function để hiển thị loading
function showChartLoading(show) {
    const charts = ['revenueChart', 'topVehiclesChart', 'vehicleTypesChart', 'contractStatusChart', 'expensesChart', 'thietHaiChart'];
    charts.forEach(chartId => {
        const canvas = document.getElementById(chartId);
        if (canvas) {
            canvas.style.opacity = show ? '0.5' : '1';
        }
    });
}

// Event listener cho filter dropdown
document.getElementById('chartFilter').addEventListener('change', function () {
    const selectedFilter = this.value;
    updateCharts(selectedFilter);
    updateStatistics(selectedFilter); // Cập nhật thống kê khi filter thay đổi
});

// Event listener cho chart type dropdown
document.getElementById('chartTypeFilter').addEventListener('change', function () {
    const selectedChartType = this.value;
    const selectedFilter = document.getElementById('chartFilter').value;
    updateCharts(selectedFilter, selectedChartType);
    updateStatistics(selectedFilter); // Cập nhật thống kê khi loại biểu đồ thay đổi
});

// Set giá trị mặc định cho filter dropdown
document.getElementById('chartFilter').value = '@Model.ChartFilter';

// Cập nhật titles ban đầu
updateChartTitles('@Model.ChartFilter');

// Cập nhật nhãn thống kê ban đầu
const initialFilter = '@Model.ChartFilter';
const periodLabels = {
    'today': 'hôm nay',
    '7days': '7 ngày gần nhất',
    'week': 'tuần này',
    'month': '30 ngày gần nhất',
    'year': '12 tháng gần nhất'
};

const period = periodLabels[initialFilter] || 'hôm nay';
document.getElementById('doanhThuLabel').textContent = `Doanh thu ${period}`;
document.getElementById('doanhThuGocLabel').textContent = `Doanh thu gốc ${period}`;
document.getElementById('tongChiTieuLabel').textContent = `Tổng chi tiêu ${period}`;
document.getElementById('tongThietHaiLabel').textContent = `Tổng tiền thiệt hại ${period}`;
document.getElementById('donDatLabel').textContent = `Đơn đặt ${period}`;
document.getElementById('khachHangMoiLabel').textContent = `Khách hàng mới ${period}`;

// Điều chỉnh chiều cao ban đầu cho các biểu đồ
adjustChartHeight('revenueChart', revenueData.length);
adjustChartHeight('vehicleTypesChart', vehicleTypeLabels.length);
adjustChartHeight('contractStatusChart', contractStatusLabels.length);
adjustChartHeight('expensesChart', expensesLabels.length);
adjustChartHeight('thietHaiChart', thietHaiLabels.length);
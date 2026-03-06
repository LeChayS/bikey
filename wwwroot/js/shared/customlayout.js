// Tab switching functionality
document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.addEventListener("click", function () {
        document
            .querySelectorAll(".tab-btn")
            .forEach((b) => b.classList.remove("active"));
        this.classList.add("active");
    });
});

// Sticky navbar with scroll effect
window.addEventListener("scroll", function () {
    const navbar = document.querySelector(".navbar");
    if (window.scrollY > 50) {
        navbar.classList.add("scrolled");
    } else {
        navbar.classList.remove("scrolled");
    }
});

// Cart functionality
$(document).ready(function () {
    loadCartCount();
});

function loadCartCount() {
    $.ajax({
        url: '@Url.Action("GetItemCount", "Cart")',
        type: 'GET',
        success: function (response) {
            updateCartBadge(response.count);
        },
        error: function () {
            console.log('Error loading cart count');
        }
    });
}

function updateCartBadge(count) {
    const badge = $('.cart-badge');
    if (count > 0) {
        badge.text(count).show();
    } else {
        badge.hide();
    }
}

function updateCartCountDisplay() {
    // Function to be called from other scripts to update cart count
    loadCartCount();
}
document.addEventListener("DOMContentLoaded", () => {
    const navbar = document.querySelector(".main-navbar");
    const navCollapse = document.querySelector(".navbar-collapse");
    const navLinks = document.querySelectorAll(".navbar-nav .nav-link");

    if (!navbar) {
        return;
    }

    const toggleNavbarShadow = () => {
        if (window.scrollY > 16) {
            navbar.classList.add("scrolled");
        } else {
            navbar.classList.remove("scrolled");
        }
    };

    toggleNavbarShadow();
    window.addEventListener("scroll", toggleNavbarShadow);

    navLinks.forEach((link) => {
        link.addEventListener("click", () => {
            if (!navCollapse?.classList.contains("show")) {
                return;
            }

            const collapseInstance = bootstrap.Collapse.getInstance(navCollapse);
            collapseInstance?.hide();
        });
    });
});
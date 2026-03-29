/**
 * Account status monitor for all authenticated users
 * Automatically logs out user if their account is deactivated
 * Runs on all pages, not just admin pages
 */

(function () {
    const ACCOUNT_CHECK_INTERVAL = 30000; // 30 seconds
    let accountCheckInterval = null;
    let lastAccountStatus = null;
    let isChecking = false;

    /**
     * Check if user is authenticated by looking for auth token/cookie
     */
    function isUserAuthenticated() {
        // Check if there's a non-empty body with authenticated content
        // A better approach would be to check a meta tag set by the server
        return document.documentElement.innerHTML.length > 0;
    }

    /**
     * Fetch current user account status
     */
    async function fetchUserAccountStatus() {
        if (isChecking) {
            return null; // Prevent concurrent requests
        }

        isChecking = true;
        try {
            const response = await fetch('/Account/GetUserAccountStatus', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                isChecking = false;
                return null;
            }

            const data = await response.json();
            isChecking = false;
            return data;
        } catch (error) {
            console.warn('Error fetching account status:', error);
            isChecking = false;
            return null;
        }
    }

    /**
     * Check user account status and logout if deactivated
     */
    async function monitorAccountStatus() {
        const status = await fetchUserAccountStatus();

        if (!status || !status.success) {
            return; // Skip if we couldn't fetch status or user not authenticated
        }

        // Initialize on first check
        if (lastAccountStatus === null) {
            lastAccountStatus = status.isActive;
            return;
        }

        // Check if account was deactivated
        if (lastAccountStatus && !status.isActive) {
            console.log('Account has been deactivated, logging out...');

            // Show warning toast before logout
            if (window.toastr) {
                toastr.warning('Tài khoản của bạn đã bị khóa, vui lòng đăng nhập lại!', 'Thông báo', {
                    positionClass: 'toast-top-right',
                    timeOut: 3000
                });
            } else {
                alert('Tài khoản của bạn đã bị khóa. Vui lòng đăng nhập lại!');
            }

            // Redirect to logout after a short delay
            setTimeout(() => {
                // Force logout by posting to logout endpoint
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = '/Account/Logout';

                // Add anti-forgery token if available
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    form.appendChild(tokenElement.cloneNode(true));
                }

                document.body.appendChild(form);
                form.submit();
            }, 2000);
        }

        lastAccountStatus = status.isActive;
    }

    /**
     * Start monitoring account status
     */
    function startAccountMonitoring() {
        console.log('Starting account status monitoring...');

        // Initial check after a short delay
        setTimeout(() => {
            monitorAccountStatus();
        }, 2000);

        // Set up interval polling for account status
        accountCheckInterval = setInterval(() => {
            monitorAccountStatus();
        }, ACCOUNT_CHECK_INTERVAL);
    }

    /**
     * Stop monitoring
     */
    function stopAccountMonitoring() {
        if (accountCheckInterval) {
            clearInterval(accountCheckInterval);
            accountCheckInterval = null;
            console.log('Account status monitoring stopped');
        }
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        // Check if user is authenticated - simple way is to check if there's a logout button/link
        const logoutForm = document.querySelector('form[action*="Logout"]');
        if (logoutForm || document.querySelector('a[href*="Logout"]')) {
            startAccountMonitoring();
        }
    });

    // Cleanup when leaving the page
    window.addEventListener('beforeunload', function () {
        stopAccountMonitoring();
    });

    // Global access
    window.AccountMonitor = {
        start: startAccountMonitoring,
        stop: stopAccountMonitoring,
        checkNow: monitorAccountStatus
    };
})();

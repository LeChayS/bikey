/**
 * Account status monitor for all authenticated users
 * Automatically logs out user IMMEDIATELY if their account is deactivated or deleted
 */

(function () {
    const ACCOUNT_CHECK_INTERVAL = 10000; // Check every 10 seconds
    let accountCheckInterval = null;
    let lastAccountStatus = null;
    let isChecking = false;
    let hasLoggedOut = false;

    /**
     * Fetch current user account status
     */
    async function fetchUserAccountStatus() {
        if (isChecking || hasLoggedOut) {
            return null;
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
     * Logout immediately when account is deactivated or deleted
     */
    function logoutImmediately(reason) {
        if (hasLoggedOut) return;
        hasLoggedOut = true;

        console.log('Account status changed: ' + reason + '. Logging out immediately...');
        
        // Show warning toast before logout
        if (window.toastr) {
            toastr.warning('Tài khoản của bạn đã bị khóa, vui lòng đăng nhập lại!', 'Thông báo', {
                positionClass: 'toast-top-right',
                timeOut: 1000,
                onHidden: performLogout
            });
        } else {
            alert('Tài khoản của bạn đã bị khóa. Vui lòng đăng nhập lại!');
            performLogout();
        }
    }

    /**
     * Perform the actual logout
     */
    function performLogout() {
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
    }

    /**
     * Combined monitor function - checks if account is still active
     */
    async function monitorAccountStatus() {
        const status = await fetchUserAccountStatus();

        if (!status || !status.success) {
            return; // Skip if we couldn't fetch status
        }

        // First check - initialize status
        if (lastAccountStatus === null) {
            lastAccountStatus = {
                isActive: status.isActive,
                exists: true
            };
            return;
        }

        // Account was deleted (can't find user)
        if (lastAccountStatus.exists && !status.success) {
            logoutImmediately('Account deleted');
            return;
        }

        // Account was deactivated
        if (lastAccountStatus.isActive && !status.isActive) {
            logoutImmediately('Account deactivated');
            return;
        }

        // Update current status
        lastAccountStatus = {
            isActive: status.isActive,
            exists: status.success
        };
    }

    /**
     * Start monitoring account status
     */
    function startAccountMonitoring() {
        console.log('Starting account status monitoring...');

        // Initial check immediately
        monitorAccountStatus();

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
        // Start monitoring immediately (user must be authenticated to access this page)
        startAccountMonitoring();
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

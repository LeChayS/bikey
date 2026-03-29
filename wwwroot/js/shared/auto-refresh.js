/**
 * Auto-refresh system for admin pages
 * Polls every 15 seconds to check for data changes
 * Only refreshes if data checksum has changed
 * Also monitors user account status for forced logout on deactivation
 */

(function () {
    const POLL_INTERVAL = 15000; // 15 seconds
    const ACCOUNT_CHECK_INTERVAL = 30000; // 30 seconds for account status
    const CHECKSUM_KEY = 'admin-data-checksum';
    let pollInterval = null;
    let accountCheckInterval = null;
    let lastChecksum = null;
    let lastAccountStatus = null;

    /**
     * Get the current page context (controller and action)
     */
    function getPageContext() {
        // Try to get from meta tags
        const controllerMeta = document.querySelector('meta[name="controller"]');
        const actionMeta = document.querySelector('meta[name="action"]');

        if (controllerMeta && actionMeta) {
            return {
                controller: controllerMeta.getAttribute('content'),
                action: actionMeta.getAttribute('content')
            };
        }

        // Fallback: try to parse from URL
        const pathname = window.location.pathname;
        const parts = pathname.split('/').filter(p => p);

        if (parts.length >= 2) {
            return {
                controller: parts[0],
                action: parts[1]
            };
        }

        return null;
    }

    /**
     * Fetch the current data checksum from the server
     */
    async function fetchDataChecksum(controller, action) {
        try {
            const response = await fetch(`/${controller}/GetDataChecksum`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ action: action })
            });

            if (!response.ok) {
                console.warn(`Failed to fetch checksum: ${response.status}`);
                return null;
            }

            const data = await response.json();
            return data.checksum;
        } catch (error) {
            console.warn('Error fetching data checksum:', error);
            return null;
        }
    }

    /**
     * Check if data has changed and refresh if needed
     */
    async function checkAndRefresh(controller, action) {
        const currentChecksum = await fetchDataChecksum(controller, action);

        if (!currentChecksum) {
            return; // Skip if we couldn't fetch checksum
        }

        // Initialize with first checksum
        if (!lastChecksum) {
            lastChecksum = currentChecksum;
            return;
        }

        // Check if checksum has changed
        if (lastChecksum !== currentChecksum) {
            console.log('Data changed detected, refreshing page...');
            lastChecksum = currentChecksum;

            // Use location.reload() with cache bypass
            window.location.href = window.location.href;
        }
    }

    /**
     * Fetch current user account status to detect if account was deactivated
     */
    async function fetchUserAccountStatus() {
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
                return null;
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.warn('Error fetching account status:', error);
            return null;
        }
    }

    /**
     * Check user account status and logout if deactivated
     */
    async function checkAccountStatus() {
        const status = await fetchUserAccountStatus();

        if (!status || !status.success) {
            return; // Skip if we couldn't fetch status
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
            }

            // Redirect to logout after a short delay
            setTimeout(() => {
                window.location.href = '/Account/Logout';
            }, 2000);
        }

        lastAccountStatus = status.isActive;
    }

    /**
     * Start the polling mechanism
     */
    function startPolling() {
        const context = getPageContext();

        if (!context) {
            console.warn('Could not determine page context for auto-refresh');
            return;
        }

        console.log('Starting auto-refresh polling for', context.controller, context.action);

        // Initial check after a short delay
        setTimeout(() => {
            checkAndRefresh(context.controller, context.action);
            checkAccountStatus(); // Also check account status
        }, 1000);

        // Set up interval polling for data changes
        pollInterval = setInterval(() => {
            checkAndRefresh(context.controller, context.action);
        }, POLL_INTERVAL);

        // Set up interval polling for account status (every 30 seconds)
        accountCheckInterval = setInterval(() => {
            checkAccountStatus();
        }, ACCOUNT_CHECK_INTERVAL);
    }

    /**
     * Stop the polling mechanism
     */
    function stopPolling() {
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
            console.log('Auto-refresh polling stopped');
        }
        if (accountCheckInterval) {
            clearInterval(accountCheckInterval);
            accountCheckInterval = null;
            console.log('Account check polling stopped');
        }
    }

    /**
     * Check if we're on an admin page (contains /admin/ or specific admin controllers)
     */
    function isAdminPage() {
        const pathname = window.location.pathname.toLowerCase();
        const adminControllers = ['nguoidung', 'xe', 'hoaodn', 'hopdong', 'loaixe', 'datchoxe', 'admin'];

        return adminControllers.some(controller => pathname.includes('/' + controller));
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        if (isAdminPage()) {
            startPolling();
        }
    });

    // Cleanup when leaving the page
    window.addEventListener('beforeunload', function () {
        stopPolling();
    });

    // Global access for debugging
    window.AdminAutoRefresh = {
        start: startPolling,
        stop: stopPolling,
        checkNow: () => {
            const context = getPageContext();
            if (context) checkAndRefresh(context.controller, context.action);
        },
        checkAccountStatus: checkAccountStatus
    };
})();

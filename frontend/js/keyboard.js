document.addEventListener('DOMContentLoaded', () => {
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            const modals = document.querySelectorAll('[role="dialog"], .modal');
            modals.forEach(modal => {
                if (!modal.classList.contains('hidden')) {
                    modal.classList.add('hidden');
                }
            });

            const sidebar = document.getElementById('sidebar');
            if (sidebar && !sidebar.classList.contains('-translate-x-full') && window.innerWidth < 1024) {
                sidebar.classList.add('-translate-x-full');
            }
        }

        if (e.key === '/' && !['INPUT', 'TEXTAREA', 'SELECT'].includes(e.target.tagName)) {
            e.preventDefault();
            const searchInput = document.querySelector('input[type="search"], input[placeholder*="search" i], input[name*="search" i]');
            if (searchInput) {
                searchInput.focus();
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 's') {
            e.preventDefault();
            const form = document.querySelector('form');
            if (form) {
                const submitButton = form.querySelector('button[type="submit"]');
                if (submitButton && !submitButton.disabled) {
                    submitButton.click();
                }
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('input[type="search"], input[placeholder*="search" i]');
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }
        }
    });

    const focusableElements = document.querySelectorAll(
        'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    
    focusableElements.forEach(element => {
        if (!element.classList.contains('focus:outline-none') && !element.classList.contains('focus:ring')) {
            element.classList.add('focus:outline-none', 'focus:ring-2', 'focus:ring-blue-500', 'focus:ring-offset-2');
        }
    });
});

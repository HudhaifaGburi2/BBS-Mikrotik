// Admin subscribers page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const user = auth.getUser();
    if (user && user.fullName) {
        document.getElementById('user-name').textContent = user.fullName;
    }

    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const closeSidebar = document.getElementById('close-sidebar');
    const searchInput = document.getElementById('search-input');
    const statusFilter = document.getElementById('status-filter');

    menuToggle.addEventListener('click', () => {
        sidebar.classList.toggle('-translate-x-full');
    });

    closeSidebar.addEventListener('click', () => {
        sidebar.classList.add('-translate-x-full');
    });

    let currentPage = 1;
    const pageSize = 20;
    let searchTerm = '';
    let statusValue = '';

    async function loadSubscribers() {
        try {
            utils.showLoading();
            
            const queryParams = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize,
                activeOnly: statusValue === 'Active',
                search: searchTerm
            });

            const response = await api.get('/subscribers?' + queryParams.toString());
            
            if (response && response.data) {
                const subscribers = response.data.items || response.data;
                const totalPages = response.data.totalPages || 1;
                
                const columns = [
                    { label: 'Name', field: 'fullName' },
                    { label: 'Email', field: 'email' },
                    { label: 'Phone', field: 'phoneNumber' },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: (row) => {
                            const statusColors = {
                                'Active': 'bg-green-100 text-green-800',
                                'Suspended': 'bg-yellow-100 text-yellow-800',
                                'Expired': 'bg-red-100 text-red-800'
                            };
                            const colorClass = statusColors[row.status] || 'bg-gray-100 text-gray-800';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + colorClass + '">' + (row.status || 'Unknown') + '</span>';
                        }
                    },
                    { 
                        label: 'Actions', 
                        field: 'id',
                        render: (row) => '<div class="flex space-x-2"><a href="subscriber-form.html?mode=edit&id=' + row.id + '" class="text-blue-600 hover:text-blue-800"><svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg></a><button onclick="suspendSubscriber(\'' + row.id + '\')" class="text-yellow-600 hover:text-yellow-800"><svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"></path></svg></button></div>'
                    }
                ];
                
                components.renderTable(subscribers, columns, 'subscribers-table');
                components.renderPagination(currentPage, totalPages, 'goToPage');
            }
        } catch (error) {
            console.error('Error loading subscribers:', error);
            utils.showToast('Failed to load subscribers', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    window.goToPage = function(page) {
        currentPage = page;
        loadSubscribers();
    };

    window.suspendSubscriber = async function(id) {
        if (!confirm('Are you sure you want to suspend this subscriber?')) {
            return;
        }
        
        try {
            utils.showLoading();
            await api.put('/subscribers/' + id + '/suspend', {});
            utils.showToast('Subscriber suspended successfully', 'success');
            loadSubscribers();
        } catch (error) {
            console.error('Error suspending subscriber:', error);
            utils.showToast('Failed to suspend subscriber', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    const debouncedSearch = utils.debounce(() => {
        currentPage = 1;
        loadSubscribers();
    }, 500);

    searchInput.addEventListener('input', (e) => {
        searchTerm = e.target.value;
        debouncedSearch();
    });

    statusFilter.addEventListener('change', (e) => {
        statusValue = e.target.value;
        currentPage = 1;
        loadSubscribers();
    });

    loadSubscribers();
})();

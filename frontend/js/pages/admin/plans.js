// Admin plans page initialization
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

    menuToggle.addEventListener('click', () => {
        sidebar.classList.toggle('-translate-x-full');
    });

    closeSidebar.addEventListener('click', () => {
        sidebar.classList.add('-translate-x-full');
    });

    async function loadPlans() {
        try {
            utils.showLoading();
            
            const response = await api.get('/plans');
            
            if (response && response.data) {
                const plans = response.data;
                
                const columns = [
                    { label: 'Name', field: 'name' },
                    { 
                        label: 'Speed', 
                        field: 'speedMbps',
                        render: (row) => row.speedMbps + ' Mbps'
                    },
                    { 
                        label: 'Price', 
                        field: 'price',
                        type: 'currency'
                    },
                    { 
                        label: 'Duration', 
                        field: 'durationDays',
                        render: (row) => row.durationDays + ' days'
                    },
                    { 
                        label: 'Status', 
                        field: 'isActive',
                        render: (row) => {
                            const statusClass = row.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800';
                            const statusText = row.isActive ? 'Active' : 'Inactive';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + statusClass + '">' + statusText + '</span>';
                        }
                    },
                    { 
                        label: 'Actions', 
                        field: 'id',
                        render: (row) => '<div class="flex space-x-2"><a href="plan-form.html?mode=edit&id=' + row.id + '" class="text-blue-600 hover:text-blue-800"><svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg></a><button onclick="deletePlan(\'' + row.id + '\', \'' + row.name + '\')" class="text-red-600 hover:text-red-800"><svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg></button></div>'
                    }
                ];
                
                components.renderTable(plans, columns, 'plans-table');
            }
        } catch (error) {
            console.error('Error loading plans:', error);
            utils.showToast('Failed to load plans', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    window.deletePlan = async function(id, name) {
        if (!confirm('Are you sure you want to delete the plan "' + name + '"?')) {
            return;
        }
        
        try {
            utils.showLoading();
            await api.delete('/plans/' + id);
            utils.showToast('Plan deleted successfully', 'success');
            loadPlans();
        } catch (error) {
            console.error('Error deleting plan:', error);
            utils.showToast('Failed to delete plan', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    loadPlans();
})();

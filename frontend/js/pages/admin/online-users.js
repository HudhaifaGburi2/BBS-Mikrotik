// Admin online users page initialization
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

    let refreshInterval;

    async function fetchOnlineUsers() {
        try {
            const response = await api.get('/mikrotik/online-users');
            
            if (response && response.data) {
                const onlineUsers = response.data;
                
                document.getElementById('online-count').textContent = onlineUsers.length;
                document.getElementById('last-update').textContent = utils.formatDateTime(new Date().toISOString());
                
                const columns = [
                    { label: 'Username', field: 'username' },
                    { 
                        label: 'IP Address', 
                        field: 'ipAddress',
                        render: (row) => row.ipAddress || row.ip || '-'
                    },
                    { 
                        label: 'Session Duration', 
                        field: 'sessionDuration',
                        render: (row) => {
                            const seconds = row.sessionDuration || row.uptime || 0;
                            const hours = Math.floor(seconds / 3600);
                            const minutes = Math.floor((seconds % 3600) / 60);
                            return hours + 'h ' + minutes + 'm';
                        }
                    },
                    { 
                        label: 'Download Speed', 
                        field: 'downloadSpeed',
                        render: (row) => {
                            const speed = row.downloadSpeed || row.rxRate || 0;
                            return (speed / 1000000).toFixed(2) + ' Mbps';
                        }
                    },
                    { 
                        label: 'Upload Speed', 
                        field: 'uploadSpeed',
                        render: (row) => {
                            const speed = row.uploadSpeed || row.txRate || 0;
                            return (speed / 1000000).toFixed(2) + ' Mbps';
                        }
                    },
                    { 
                        label: 'Data Used', 
                        field: 'dataUsed',
                        render: (row) => {
                            const bytes = (row.dataUsed || row.rxBytes || 0) + (row.txBytes || 0);
                            return utils.formatBytes(bytes);
                        }
                    },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: () => '<span class="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800"><span class="inline-block h-2 w-2 bg-green-600 rounded-full mr-1"></span>Online</span>'
                    },
                    { 
                        label: 'Actions', 
                        field: 'username',
                        render: (row) => '<button onclick="disconnectUser(\'' + (row.username || row.id) + '\')" class="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition-colors">Disconnect</button>'
                    }
                ];
                
                components.renderTable(onlineUsers, columns, 'online-users-table');
            }
        } catch (error) {
            console.error('Error fetching online users:', error);
            document.getElementById('online-count').textContent = '0';
            document.getElementById('last-update').textContent = utils.formatDateTime(new Date().toISOString());
            
            document.getElementById('online-users-table').innerHTML = '<div class="text-center py-12 text-gray-500"><svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a4.978 4.978 0 01-1.414-2.83m-1.414 5.658a9 9 0 01-2.167-9.238m7.824 2.167a1 1 0 111.414 1.414m-1.414-1.414L3 3m8.293 8.293l1.414 1.414"></path></svg><p class="mt-2">No online users or unable to connect to MikroTik</p></div>';
        }
    }

    window.disconnectUser = async function(username) {
        if (!confirm('Are you sure you want to disconnect user "' + username + '"?')) {
            return;
        }
        
        try {
            utils.showLoading();
            await api.post('/mikrotik/disconnect/' + username, {});
            utils.showToast('User ' + username + ' disconnected successfully', 'success');
            
            setTimeout(() => {
                fetchOnlineUsers();
            }, 1000);
        } catch (error) {
            console.error('Error disconnecting user:', error);
            utils.showToast('Failed to disconnect user', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    function startAutoRefresh() {
        refreshInterval = setInterval(() => {
            fetchOnlineUsers();
        }, 30000);
    }

    function stopAutoRefresh() {
        if (refreshInterval) {
            clearInterval(refreshInterval);
        }
    }

    window.addEventListener('beforeunload', () => {
        stopAutoRefresh();
    });

    fetchOnlineUsers();
    startAutoRefresh();
})();

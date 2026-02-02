// Client dashboard page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const user = auth.getUser();
    if (user && user.fullName) {
        document.getElementById('user-name').textContent = 'Welcome, ' + user.fullName;
    }

    async function loadDashboardData() {
        try {
            utils.showLoading();
            
            const [subscriberResponse, subscriptionResponse] = await Promise.all([
                api.get('/subscribers/me'),
                api.get('/subscriptions/me')
            ]);

            if (subscriberResponse && subscriberResponse.data) {
                const subscriber = subscriberResponse.data;
                document.getElementById('balance').textContent = utils.formatCurrency(subscriber.balance || 0);
            }

            if (subscriptionResponse && subscriptionResponse.data) {
                const subscription = subscriptionResponse.data;
                document.getElementById('plan-name').textContent = subscription.planName || '-';
                document.getElementById('expiry-date').textContent = utils.formatDate(subscription.endDate);
                
                const dataUsed = subscription.dataUsedGB || 0;
                const dataLimit = subscription.dataLimitGB || 0;
                const usagePercentage = dataLimit > 0 ? Math.round((dataUsed / dataLimit) * 100) : 0;
                
                document.getElementById('data-used').textContent = dataUsed + ' GB';
                document.getElementById('usage-percentage').textContent = usagePercentage + '%';
                document.getElementById('usage-bar').style.width = Math.min(usagePercentage, 100) + '%';
                
                if (usagePercentage > 90) {
                    document.getElementById('usage-bar').classList.remove('bg-blue-600');
                    document.getElementById('usage-bar').classList.add('bg-red-600');
                } else if (usagePercentage > 75) {
                    document.getElementById('usage-bar').classList.remove('bg-blue-600');
                    document.getElementById('usage-bar').classList.add('bg-yellow-600');
                }
            }

            await loadRecentTransactions();
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            utils.showToast('Failed to load dashboard data', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    async function loadRecentTransactions() {
        try {
            const response = await api.get('/payments?pageSize=5');
            
            if (response && response.data) {
                const transactions = response.data.items || response.data;
                
                if (transactions.length === 0) {
                    document.getElementById('transactions-table').innerHTML = '<div class="text-center py-8 text-gray-500"><p>No recent transactions</p></div>';
                    return;
                }
                
                const columns = [
                    { label: 'Date', field: 'paymentDate', type: 'date' },
                    { label: 'Description', field: 'description', render: (row) => row.description || 'Payment' },
                    { label: 'Amount', field: 'amount', type: 'currency' },
                    { label: 'Method', field: 'paymentMethod' },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: (row) => {
                            const colors = {
                                'Completed': 'bg-green-100 text-green-800',
                                'Pending': 'bg-yellow-100 text-yellow-800',
                                'Failed': 'bg-red-100 text-red-800'
                            };
                            const colorClass = colors[row.status] || 'bg-gray-100 text-gray-800';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + colorClass + '">' + row.status + '</span>';
                        }
                    }
                ];
                
                components.renderTable(transactions, columns, 'transactions-table');
            }
        } catch (error) {
            console.error('Error loading transactions:', error);
            document.getElementById('transactions-table').innerHTML = '<div class="text-center py-8 text-gray-500"><p>Failed to load transactions</p></div>';
        }
    }

    loadDashboardData();
})();

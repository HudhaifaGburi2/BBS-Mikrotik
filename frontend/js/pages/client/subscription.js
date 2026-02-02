// Client subscription page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    let subscriptionData = null;
    let usageChart = null;

    async function loadSubscription() {
        try {
            utils.showLoading();
            
            const response = await api.get('/subscriptions/me');
            
            if (response && response.data) {
                subscriptionData = response.data;
                
                document.getElementById('plan-name').textContent = subscriptionData.planName || '-';
                document.getElementById('download-speed').textContent = subscriptionData.downloadSpeedMbps ? subscriptionData.downloadSpeedMbps + ' Mbps' : '-';
                document.getElementById('upload-speed').textContent = subscriptionData.uploadSpeedMbps ? subscriptionData.uploadSpeedMbps + ' Mbps' : '-';
                document.getElementById('start-date').textContent = utils.formatDate(subscriptionData.startDate);
                document.getElementById('end-date').textContent = utils.formatDate(subscriptionData.endDate);
                document.getElementById('price').textContent = utils.formatCurrency(subscriptionData.price || 0);
                
                const statusColors = {
                    'Active': 'text-green-600',
                    'Suspended': 'text-yellow-600',
                    'Expired': 'text-red-600'
                };
                const statusClass = statusColors[subscriptionData.status] || 'text-gray-600';
                document.getElementById('status').innerHTML = '<span class="' + statusClass + '">' + (subscriptionData.status || '-') + '</span>';
                
                document.getElementById('auto-renew').checked = subscriptionData.autoRenew || false;
                
                await loadUsageChart();
            }
        } catch (error) {
            console.error('Error loading subscription:', error);
            utils.showToast('Failed to load subscription details', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    async function loadUsageChart() {
        try {
            const response = await api.get('/usage/daily?days=7');
            
            if (response && response.data) {
                const usageData = response.data;
                
                const ctx = document.getElementById('usage-chart').getContext('2d');
                
                if (usageChart) {
                    usageChart.destroy();
                }

                usageChart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: usageData.map(item => utils.formatDate(item.date)),
                        datasets: [{
                            label: 'Data Usage (GB)',
                            data: usageData.map(item => item.dataGB || 0),
                            backgroundColor: 'rgba(59, 130, 246, 0.1)',
                            borderColor: 'rgba(59, 130, 246, 1)',
                            borderWidth: 2,
                            fill: true,
                            tension: 0.4
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading usage chart:', error);
        }
    }

    window.toggleAutoRenew = async function() {
        const autoRenew = document.getElementById('auto-renew').checked;
        
        try {
            utils.showLoading();
            await api.put('/subscriptions/me/auto-renew', { autoRenew });
            utils.showToast('Auto-renew ' + (autoRenew ? 'enabled' : 'disabled'), 'success');
        } catch (error) {
            console.error('Error updating auto-renew:', error);
            utils.showToast('Failed to update auto-renew setting', 'error');
            document.getElementById('auto-renew').checked = !autoRenew;
        } finally {
            utils.hideLoading();
        }
    };

    window.renewSubscription = async function() {
        if (!confirm('Are you sure you want to renew your subscription?')) {
            return;
        }
        
        try {
            utils.showLoading();
            await api.post('/subscriptions/me/renew', {});
            utils.showToast('Subscription renewed successfully', 'success');
            setTimeout(() => {
                loadSubscription();
            }, 1500);
        } catch (error) {
            console.error('Error renewing subscription:', error);
            utils.showToast('Failed to renew subscription', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    window.upgradePlan = function() {
        utils.showToast('Please contact support to upgrade your plan', 'info');
    };

    loadSubscription();
})();

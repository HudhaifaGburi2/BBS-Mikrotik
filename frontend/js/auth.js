const auth = {
    async login(username, password, rememberMe = false) {
        // Try admin login first, then subscriber if 401
        try {
            const response = await api.post('/auth/admin/login', {
                username: username,
                password: password,
                rememberMe: rememberMe,
                deviceName: navigator.userAgent,
                operatingSystem: navigator.platform
            });

            if (response && response.accessToken) {
                localStorage.setItem('authToken', response.accessToken);
                localStorage.setItem('refreshToken', response.refreshToken);
                localStorage.setItem('userData', JSON.stringify({
                    userType: response.userType,
                    fullName: response.fullName,
                    role: response.role
                }));
                
                // Verify storage
                console.log('✅ Admin login success - Token stored:', !!localStorage.getItem('authToken'));
                console.log('✅ User data stored:', localStorage.getItem('userData'));
                console.log('✅ User data parsed:', JSON.parse(localStorage.getItem('userData')));
                return true;
            }
        } catch (adminError) {
            // Only try subscriber if admin returned 401 (wrong credentials, not network error)
            if (adminError.message && !adminError.message.includes('فشل الاتصال')) {
                try {
                    const response = await api.post('/auth/subscriber/login', {
                        username: username,
                        password: password,
                        rememberMe: rememberMe,
                        deviceName: navigator.userAgent,
                        operatingSystem: navigator.platform
                    });

                    if (response && response.accessToken) {
                        localStorage.setItem('authToken', response.accessToken);
                        localStorage.setItem('refreshToken', response.refreshToken);
                        localStorage.setItem('userData', JSON.stringify({
                            userType: response.userType,
                            fullName: response.fullName,
                            hasActiveSubscription: response.hasActiveSubscription
                        }));
                        
                        // Verify storage
                        console.log('✅ Subscriber login success - Token stored:', !!localStorage.getItem('authToken'));
                        console.log('✅ User data stored:', localStorage.getItem('userData'));
                        console.log('✅ User data parsed:', JSON.parse(localStorage.getItem('userData')));
                        return true;
                    }
                } catch (subscriberError) {
                    throw subscriberError;
                }
            } else {
                // Network error - don't retry
                throw adminError;
            }
        }
        
        throw new Error('استجابة غير صالحة من الخادم');
    },

    async logout() {
        try {
            await api.post('/auth/logout');
        } catch (error) {
            console.error('خطأ في تسجيل الخروج:', error);
        } finally {
            localStorage.removeItem('authToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('userData');
            window.location.href = '/index.html';
        }
    },

    isAuthenticated() {
        const token = localStorage.getItem('authToken');
        const userData = localStorage.getItem('userData');
        return !!(token && userData);
    },

    getUser() {
        const userData = localStorage.getItem('userData');
        if (userData) {
            try {
                return JSON.parse(userData);
            } catch (error) {
                console.error('خطأ في تحليل بيانات المستخدم:', error);
                return null;
            }
        }
        return null;
    },

    getToken() {
        return localStorage.getItem('authToken');
    },

    getUserType() {
        const user = this.getUser();
        return user ? user.userType : null;
    },

    isAdmin() {
        return this.getUserType() === 'Admin';
    },

    isSubscriber() {
        return this.getUserType() === 'Subscriber';
    },

    getUserRole() {
        const user = this.getUser();
        return user ? user.role : null;
    },

    requireAuth() {
        if (!this.isAuthenticated()) {
            window.location.href = '/index.html';
            return false;
        }
        return true;
    },

    requireRole(role) {
        if (!this.requireAuth()) {
            return false;
        }
        const userRole = this.getUserRole();
        if (userRole !== role) {
            utils.showToast('تم رفض الوصول. صلاحيات غير كافية.', 'error');
            window.location.href = '/index.html';
            return false;
        }
        return true;
    }
};

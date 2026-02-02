const auth = {
    async login(username, password, rememberMe = false) {
        // Try admin login first
        try {
            const response = await api.post('/auth/admin/login', {
                username: username,
                password: password,
                rememberMe: rememberMe,
                deviceName: navigator.userAgent,
                operatingSystem: navigator.platform
            });

            if (response.data && response.data.accessToken) {
                localStorage.setItem('authToken', response.data.accessToken);
                localStorage.setItem('refreshToken', response.data.refreshToken);
                localStorage.setItem('userData', JSON.stringify({
                    userType: response.data.userType,
                    fullName: response.data.fullName,
                    role: response.data.role
                }));
                return true;
            }
        } catch (adminError) {
            // If admin login fails, try subscriber login
            try {
                const response = await api.post('/auth/subscriber/login', {
                    username: username,
                    password: password,
                    rememberMe: rememberMe,
                    deviceName: navigator.userAgent,
                    operatingSystem: navigator.platform
                });

                if (response.data && response.data.accessToken) {
                    localStorage.setItem('authToken', response.data.accessToken);
                    localStorage.setItem('refreshToken', response.data.refreshToken);
                    localStorage.setItem('userData', JSON.stringify({
                        userType: response.data.userType,
                        fullName: response.data.fullName,
                        hasActiveSubscription: response.data.hasActiveSubscription
                    }));
                    return true;
                }
            } catch (subscriberError) {
                // If both fail, throw the subscriber error (more likely to be the correct one)
                throw subscriberError;
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

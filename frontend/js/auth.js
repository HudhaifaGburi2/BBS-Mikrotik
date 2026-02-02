const auth = {
    // Flag to prevent redirect loops
    _isLoggingIn: false,
    _isLoggingOut: false,

    // Store auth data synchronously to localStorage
    _storeAuthData(response, userType) {
        try {
            const userData = {
                userType: response.userType || userType,
                fullName: response.fullName,
                role: response.role,
                hasActiveSubscription: response.hasActiveSubscription
            };
            
            // Store synchronously - these are blocking operations
            localStorage.setItem('authToken', response.accessToken);
            if (response.refreshToken) {
                localStorage.setItem('refreshToken', response.refreshToken);
            }
            localStorage.setItem('userData', JSON.stringify(userData));
            
            // Store login timestamp for session management
            localStorage.setItem('authTimestamp', Date.now().toString());
            
            return true;
        } catch (error) {
            console.error('Failed to store auth data:', error);
            return false;
        }
    },

    // Clear all auth data
    _clearAuthData() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userData');
        localStorage.removeItem('authTimestamp');
    },

    async login(username, password, rememberMe = false) {
        // Prevent concurrent login attempts
        if (this._isLoggingIn) {
            throw new Error('Login already in progress');
        }
        
        this._isLoggingIn = true;
        
        try {
            // Clear any existing auth data before login attempt
            this._clearAuthData();
            
            const loginPayload = {
                username: username,
                password: password,
                rememberMe: rememberMe,
                deviceName: navigator.userAgent,
                operatingSystem: navigator.platform
            };

            // Try admin login first
            try {
                const response = await this._rawPost('/auth/admin/login', loginPayload);
                
                if (response && response.accessToken) {
                    const stored = this._storeAuthData(response, 'Admin');
                    if (!stored) {
                        throw new Error('Failed to save session');
                    }
                    return { success: true, userType: 'Admin' };
                }
            } catch (adminError) {
                // Only try subscriber if admin returned 401 (wrong credentials)
                if (adminError.status === 401 || (adminError.message && !adminError.message.includes('فشل الاتصال'))) {
                    try {
                        const response = await this._rawPost('/auth/subscriber/login', loginPayload);
                        
                        if (response && response.accessToken) {
                            const stored = this._storeAuthData(response, 'Subscriber');
                            if (!stored) {
                                throw new Error('Failed to save session');
                            }
                            return { success: true, userType: 'Subscriber' };
                        }
                    } catch (subscriberError) {
                        throw subscriberError;
                    }
                } else {
                    throw adminError;
                }
            }
            
            throw new Error('استجابة غير صالحة من الخادم');
        } finally {
            this._isLoggingIn = false;
        }
    },

    // Raw POST without auth redirect handling (for login/logout)
    async _rawPost(endpoint, data) {
        const API_BASE_URL = 'http://localhost:5286/api';
        
        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                let errorMessage = 'فشل الطلب';
                const error = { status: response.status, message: errorMessage };
                
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const errorData = await response.json();
                        error.message = errorData.message || errorData.errors?.[0] || errorMessage;
                    }
                } catch (e) {}
                
                throw error;
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return { success: true };
        } catch (error) {
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                throw new Error('فشل الاتصال بالخادم. تأكد من تشغيل الخادم على http://localhost:5286');
            }
            throw error;
        }
    },

    async logout() {
        if (this._isLoggingOut) return;
        this._isLoggingOut = true;
        
        try {
            const token = this.getToken();
            if (token) {
                await this._rawPost('/auth/logout', {}).catch(() => {});
            }
        } finally {
            this._clearAuthData();
            this._isLoggingOut = false;
            window.location.replace('/index.html');
        }
    },

    isAuthenticated() {
        const token = localStorage.getItem('authToken');
        const userData = localStorage.getItem('userData');
        
        if (!token || !userData) {
            return false;
        }
        
        // Validate userData is valid JSON
        try {
            JSON.parse(userData);
            return true;
        } catch {
            this._clearAuthData();
            return false;
        }
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

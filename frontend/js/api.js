const API_BASE_URL = 'http://localhost:5286/api';

const apiCache = {
    data: new Map(),
    timeout: 5 * 60 * 1000,

    set(key, value) {
        this.data.set(key, {
            value: value,
            timestamp: Date.now()
        });
    },

    get(key) {
        const cached = this.data.get(key);
        if (!cached) return null;

        if (Date.now() - cached.timestamp > this.timeout) {
            this.data.delete(key);
            return null;
        }

        return cached.value;
    },

    clear() {
        this.data.clear();
    }
};

const api = {
    async get(endpoint) {
        const cacheKey = `GET:${endpoint}`;
        const cached = apiCache.get(cacheKey);
        
        if (cached) {
            return cached;
        }


        const token = localStorage.getItem('authToken');
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'GET',
                headers: headers
            });

            if (response.status === 401) {
                localStorage.removeItem('authToken');
                localStorage.removeItem('userData');
                window.location.href = '/index.html';
                return;
            }

            if (!response.ok) {
                let errorMessage = 'فشل الطلب';
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const error = await response.json();
                        errorMessage = error.message || error.title || errorMessage;
                    } else {
                        const text = await response.text();
                        errorMessage = text || errorMessage;
                    }
                } catch (e) {
                    console.error('خطأ في تحليل استجابة الخطأ:', e);
                }
                throw new Error(errorMessage);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                const result = await response.json();
                apiCache.set(cacheKey, result);
                return result;
            }
            return { success: true };
        } catch (error) {
            console.error('خطأ في API GET:', error);
            throw error;
        }
    },

    async post(endpoint, data) {
        apiCache.clear();
        
        const token = localStorage.getItem('authToken');
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(data)
            });

            if (response.status === 401) {
                localStorage.removeItem('authToken');
                localStorage.removeItem('userData');
                window.location.href = '/index.html';
                return;
            }

            if (!response.ok) {
                let errorMessage = 'فشل الطلب';
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const error = await response.json();
                        errorMessage = error.message || error.errors?.[0] || errorMessage;
                    } else {
                        const text = await response.text();
                        errorMessage = text || errorMessage;
                    }
                } catch (e) {
                    console.error('خطأ في تحليل استجابة الخطأ:', e);
                }
                throw new Error(errorMessage);
            }

            return await response.json();
        } catch (error) {
            console.error('خطأ في API POST:', error);
            // Network error or CORS issue
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                throw new Error('فشل الاتصال بالخادم. تأكد من تشغيل الخادم على http://localhost:5286');
            }
            throw error;
        }
    },

    async put(endpoint, data) {
        apiCache.clear();
        
        const token = localStorage.getItem('authToken');
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'PUT',
                headers: headers,
                body: JSON.stringify(data)
            });

            if (response.status === 401) {
                localStorage.removeItem('authToken');
                localStorage.removeItem('userData');
                window.location.href = '/index.html';
                return;
            }

            if (!response.ok) {
                let errorMessage = 'فشل الطلب';
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const error = await response.json();
                        errorMessage = error.message || error.title || errorMessage;
                    } else {
                        const text = await response.text();
                        errorMessage = text || errorMessage;
                    }
                } catch (e) {
                    console.error('خطأ في تحليل استجابة الخطأ:', e);
                }
                throw new Error(errorMessage);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return { success: true };
        } catch (error) {
            console.error('خطأ في API PUT:', error);
            throw error;
        }
    },

    async delete(endpoint) {
        apiCache.clear();
        
        const token = localStorage.getItem('authToken');
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'DELETE',
                headers: headers
            });

            if (response.status === 401) {
                localStorage.removeItem('authToken');
                localStorage.removeItem('userData');
                window.location.href = '/index.html';
                return;
            }

            if (!response.ok) {
                let errorMessage = 'فشل الطلب';
                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const error = await response.json();
                        errorMessage = error.message || error.errors?.[0] || errorMessage;
                    } else {
                        const text = await response.text();
                        errorMessage = text || errorMessage;
                    }
                } catch (e) {
                    console.error('خطأ في تحليل استجابة الخطأ:', e);
                }
                throw new Error(errorMessage);
            }

            return await response.json();
        } catch (error) {
            console.error('خطأ في API DELETE:', error);
            throw error;
        }
    }
};

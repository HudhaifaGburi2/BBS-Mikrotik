// Forgot password page initialization
(function() {
    document.getElementById('forgot-password-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const email = document.getElementById('email').value;

        if (!email) {
            utils.showToast('الرجاء إدخال بريدك الإلكتروني', 'error');
            return;
        }

        if (!utils.validateEmail(email)) {
            utils.showToast('الرجاء إدخال عنوان بريد إلكتروني صحيح', 'error');
            return;
        }

        try {
            utils.showLoading();
            
            await api.post('/auth/forgot-password', { email });
            
            utils.showToast('تم إرسال رابط إعادة التعيين إلى بريدك الإلكتروني', 'success');
            
            setTimeout(() => {
                window.location.href = '/index.html';
            }, 2000);
        } catch (error) {
            utils.showToast(error.message || 'فشل إرسال رابط إعادة التعيين', 'error');
        } finally {
            utils.hideLoading();
        }
    });
})();

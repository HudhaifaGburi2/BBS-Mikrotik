# Broadband Billing System - Frontend

A professional, production-ready frontend for the Broadband Billing System built with **Tailwind CSS**, **Plain HTML5**, and **Vanilla JavaScript (ES6+)**. The frontend is 100% API-driven with no backend rendering.

## 🚀 Tech Stack

- **CSS Framework**: Tailwind CSS via CDN (v3.x)
- **HTML**: Plain HTML5 with semantic markup
- **JavaScript**: Pure Vanilla JS (ES6+), no frameworks or libraries
- **API Communication**: Fetch API with async/await
- **Authentication**: JWT tokens stored in LocalStorage
- **Charts**: Chart.js v4.4.0 via CDN

## 📋 Prerequisites

- Modern web browser (Chrome, Firefox, Safari, Edge)
- Web server (for local development: Python, Node.js http-server, or any static file server)
- Backend API running at configured endpoint

## 🛠️ Installation & Setup

### 1. Clone or Extract Files

```bash
cd /path/to/BroadbandBillingSystem/frontend
```

### 2. Configure API Endpoint

Edit `js/api.js` and update the API_BASE_URL:

```javascript
const API_BASE_URL = 'https://your-api-domain.com/api';
```

For local development:
```javascript
const API_BASE_URL = 'https://localhost:7001/api';
```

### 3. Start Local Development Server

**Using Python 3:**
```bash
python -m http.server 8000
```

**Using Node.js http-server:**
```bash
npx http-server -p 8000
```

**Using PHP:**
```bash
php -S localhost:8000
```

### 4. Access the Application

Open your browser and navigate to:
```
http://localhost:8000
```

## 📁 File Structure

```
frontend/
├── index.html                  # Login page
├── admin/                      # Admin portal pages
│   ├── dashboard.html          # Admin dashboard with stats
│   ├── subscribers.html        # Subscriber management
│   ├── subscriber-form.html    # Create/Edit subscriber
│   ├── plans.html              # Plan management
│   ├── plan-form.html          # Create/Edit plan
│   ├── invoices.html           # Invoice list
│   ├── invoice-detail.html     # Invoice details
│   ├── payments.html           # Payment history
│   ├── reports.html            # Reports & analytics
│   └── online-users.html       # Live online subscribers
├── client/                     # Client portal pages
│   ├── dashboard.html          # Client dashboard
│   ├── subscription.html       # Subscription details
│   ├── payment.html            # Make payment
│   ├── invoices.html           # Client invoices
│   └── usage.html              # Usage statistics
├── js/                         # Shared JavaScript modules
│   ├── api.js                  # API wrapper with caching
│   ├── auth.js                 # Authentication management
│   ├── utils.js                # Utility functions
│   ├── components.js           # Reusable UI components
│   └── keyboard.js             # Keyboard shortcuts
└── README.md                   # This file
```

## 🔐 Authentication

### Login Credentials

Contact your system administrator for login credentials.

### Token Management

- JWT tokens are stored in `localStorage` as `authToken`
- User data is stored in `localStorage` as `userData`
- Tokens automatically expire and redirect to login on 401 responses
- Logout clears all localStorage data

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `ESC` | Close modals and mobile sidebar |
| `CTRL/CMD + S` | Save current form |
| `/` | Focus search input |
| `CTRL/CMD + K` | Focus and select search input |
| `TAB` | Navigate between form fields |

## 🎨 Features

### Admin Portal
- **Dashboard**: Real-time statistics and recent activity
- **Subscriber Management**: CRUD operations with search and filters
- **Plan Management**: Create and manage broadband plans
- **Invoice Management**: Generate, view, and manage invoices
- **Payment Tracking**: View payment history and status
- **Reports**: Generate revenue, usage, and subscriber reports with charts
- **Online Users**: Real-time monitoring of active PPPoE sessions

### Client Portal
- **Dashboard**: Account overview with usage statistics
- **Subscription**: View plan details and usage charts
- **Payment**: Secure payment processing (Mada/Visa/MasterCard)
- **Invoices**: View and pay invoices
- **Usage Statistics**: Detailed data usage reports

## 🔧 Configuration

### API Endpoints

The frontend expects the following API endpoints:

**Authentication:**
- `POST /auth/login` - User login

**Subscribers:**
- `GET /subscribers` - List subscribers
- `GET /subscribers/{id}` - Get subscriber
- `GET /subscribers/me` - Get current subscriber
- `POST /subscribers` - Create subscriber
- `PUT /subscribers/{id}` - Update subscriber
- `PUT /subscribers/{id}/suspend` - Suspend subscriber
- `DELETE /subscribers/{id}` - Delete subscriber

**Plans:**
- `GET /plans` - List plans
- `GET /plans/{id}` - Get plan
- `POST /plans` - Create plan
- `PUT /plans/{id}` - Update plan
- `DELETE /plans/{id}` - Delete plan

**Subscriptions:**
- `GET /subscriptions/me` - Get current subscription
- `POST /subscriptions/me/renew` - Renew subscription
- `PUT /subscriptions/me/auto-renew` - Toggle auto-renew

**Invoices:**
- `GET /invoices` - List invoices
- `GET /invoices/{id}` - Get invoice
- `GET /invoices/me` - Get client invoices
- `PUT /invoices/{id}/pay` - Mark as paid
- `GET /invoices/{id}/pdf` - Download PDF

**Payments:**
- `GET /payments` - List payments
- `POST /payments/initiate` - Process payment

**Reports:**
- `GET /reports/dashboard-stats` - Dashboard statistics
- `GET /reports/revenue` - Revenue report
- `GET /reports/usage` - Usage report

**MikroTik:**
- `GET /mikrotik/online-users` - Get online users
- `POST /mikrotik/disconnect/{username}` - Disconnect user

**Usage:**
- `GET /usage/daily` - Daily usage statistics

## 🚀 Deployment

### Option 1: Static Hosting (Netlify, Vercel, GitHub Pages)

1. Update `API_BASE_URL` in `js/api.js` to production URL
2. Deploy the `frontend/` directory to your hosting provider
3. Ensure CORS is configured on your backend API

**Netlify:**
```bash
netlify deploy --prod --dir=frontend
```

**Vercel:**
```bash
vercel --prod frontend
```

### Option 2: Traditional Web Server (Apache, Nginx)

**Nginx Configuration:**
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    root /var/www/broadband-billing/frontend;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

**Apache Configuration (.htaccess):**
```apache
RewriteEngine On
RewriteBase /
RewriteRule ^index\.html$ - [L]
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule . /index.html [L]
```

### Option 3: CDN Deployment

1. Build a ZIP of the frontend folder
2. Upload to AWS S3, Azure Blob Storage, or Google Cloud Storage
3. Configure CDN (CloudFront, Azure CDN, Cloud CDN)
4. Enable static website hosting

## 🧪 Testing

### Manual Testing Checklist

- [ ] Login with valid credentials
- [ ] Login with invalid credentials (error message)
- [ ] Logout and verify redirect to login
- [ ] Navigate all admin pages
- [ ] Navigate all client pages
- [ ] Create, edit, delete subscriber
- [ ] Create, edit, delete plan
- [ ] Filter and search functionality
- [ ] Pagination on large lists
- [ ] Generate reports with charts
- [ ] Process payment (test mode)
- [ ] Responsive design on mobile (320px+)
- [ ] Keyboard shortcuts work
- [ ] Toast notifications appear
- [ ] Loading spinners show during API calls
- [ ] Form validation prevents invalid submission

### Browser Compatibility

Tested and working on:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 🔒 Security

- All API calls use HTTPS in production
- JWT tokens stored securely in localStorage
- Automatic token expiration handling
- No sensitive data in code or console logs
- Client-side input validation
- XSS protection via proper escaping

## ⚡ Performance

- API response caching (5-minute TTL)
- Lazy loading of data
- Debounced search inputs (500ms)
- Paginated lists (20 items per page)
- Optimized chart rendering
- Minimal CSS/JS (Tailwind CDN, no build step)

## ♿ Accessibility

- WCAG 2.1 Level AA compliant
- Semantic HTML5 elements
- ARIA labels on interactive elements
- Keyboard navigation support
- Focus states visible
- Color contrast ratios meet standards
- Screen reader compatible

## 🐛 Troubleshooting

### Issue: Cannot login / 401 errors

**Solution**: Check that:
1. Backend API is running
2. `API_BASE_URL` in `js/api.js` is correct
3. CORS is configured on backend
4. Network tab shows request reaching API

### Issue: Charts not displaying

**Solution**: Check that:
1. Chart.js CDN is accessible
2. Data is being fetched from API
3. Console shows no errors
4. Canvas element exists in DOM

### Issue: Blank page on load

**Solution**: Check that:
1. Browser console for JavaScript errors
2. Tailwind CSS CDN is loading
3. Files are served over HTTP (not file://)
4. All JS files are accessible

### Issue: Mobile menu not working

**Solution**: Check that:
1. Screen width is below 1024px
2. JavaScript is enabled
3. Event listeners are attached
4. No console errors

## 📝 Development Notes

- **No Build Step**: This is a pure static frontend with no build process
- **No NPM Dependencies**: All dependencies loaded via CDN
- **No Frameworks**: Pure Vanilla JavaScript for maximum performance
- **API-First**: All data comes from REST API, no hardcoded values
- **Modular**: Shared JavaScript files for reusability

## 📞 Support

For technical support or bug reports, contact your system administrator or development team.

## 📄 License

Proprietary - All rights reserved

---

**Version**: 1.0.0
**Last Updated**: February 2026
**Maintained By**: Development Team

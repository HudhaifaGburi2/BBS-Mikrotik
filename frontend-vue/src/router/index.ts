import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  // Auth routes
  {
    path: '/',
    name: 'login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: { guest: true },
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('@/views/auth/ForgotPasswordView.vue'),
    meta: { guest: true },
  },
  {
    path: '/contact-admin',
    name: 'contact-admin',
    component: () => import('@/views/auth/ContactAdminView.vue'),
    meta: { guest: true },
  },

  // Admin routes
  {
    path: '/admin',
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: { requiresAuth: true, role: 'Admin' },
    children: [
      { path: '', redirect: '/admin/dashboard' },
      { path: 'dashboard', name: 'admin-dashboard', component: () => import('@/views/admin/DashboardView.vue') },
      { path: 'subscribers', name: 'admin-subscribers', component: () => import('@/views/admin/SubscribersView.vue') },
      { path: 'subscribers/new', name: 'admin-subscriber-create', component: () => import('@/views/admin/SubscriberFormView.vue') },
      { path: 'subscribers/:id', name: 'admin-subscriber-detail', component: () => import('@/views/admin/SubscriberDetailView.vue') },
      { path: 'subscribers/:id/edit', name: 'admin-subscriber-edit', component: () => import('@/views/admin/SubscriberFormView.vue') },
      { path: 'plans', name: 'admin-plans', component: () => import('@/views/admin/PlansView.vue') },
      { path: 'plans/new', name: 'admin-plan-create', component: () => import('@/views/admin/PlanFormView.vue') },
      { path: 'plans/:id/edit', name: 'admin-plan-edit', component: () => import('@/views/admin/PlanFormView.vue') },
      { path: 'invoices', name: 'admin-invoices', component: () => import('@/views/admin/InvoicesView.vue') },
      { path: 'invoices/:id', name: 'admin-invoice-detail', component: () => import('@/views/admin/InvoiceDetailView.vue') },
      { path: 'payments', name: 'admin-payments', component: () => import('@/views/admin/PaymentsView.vue') },
      { path: 'online-users', name: 'admin-online-users', component: () => import('@/views/admin/OnlineUsersView.vue') },
      { path: 'reports', name: 'admin-reports', component: () => import('@/views/admin/ReportsView.vue') },
      { path: 'users', name: 'admin-users', component: () => import('@/views/admin/UsersView.vue') },
      { path: 'users/new', name: 'admin-user-create', component: () => import('@/views/admin/UserFormView.vue') },
      { path: 'users/:id/edit', name: 'admin-user-edit', component: () => import('@/views/admin/UserFormView.vue') },
      { path: 'mikrotik/users', name: 'admin-mikrotik-users', component: () => import('@/views/admin/MikroTikUsersView.vue') },
      { path: 'mikrotik/profiles', name: 'admin-mikrotik-profiles', component: () => import('@/views/admin/MikroTikProfilesView.vue') },
    ],
  },

  // Client routes
  {
    path: '/client',
    component: () => import('@/layouts/ClientLayout.vue'),
    meta: { requiresAuth: true, role: 'Subscriber' },
    children: [
      { path: '', redirect: '/client/dashboard' },
      { path: 'dashboard', name: 'client-dashboard', component: () => import('@/views/client/DashboardView.vue') },
      { path: 'subscription', name: 'client-subscription', component: () => import('@/views/client/SubscriptionView.vue') },
      { path: 'invoices', name: 'client-invoices', component: () => import('@/views/client/InvoicesView.vue') },
      { path: 'plans', name: 'client-plans', component: () => import('@/views/client/PlansView.vue') },
      { path: 'payment', name: 'client-payment', component: () => import('@/views/client/PaymentView.vue') },
      { path: 'usage', name: 'client-usage', component: () => import('@/views/client/UsageView.vue') },
    ],
  },

  // Catch-all
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  
  // Ensure auth store is initialized from localStorage
  auth.initFromStorage()

  console.log('[Router] Navigating to:', to.path, 'isAuthenticated:', auth.isAuthenticated)

  // If route requires auth
  if (to.meta.requiresAuth) {
    // Check if user is authenticated (from localStorage)
    if (!auth.isAuthenticated) {
      console.log('[Router] Not authenticated, trying checkSession...')
      // No persisted state, try to restore session from API
      const restored = await auth.checkSession()
      if (!restored) {
        console.log('[Router] checkSession failed, redirecting to login')
        return { name: 'login', query: { redirect: to.fullPath } }
      }
    }
    console.log('[Router] User authenticated:', auth.userType)
  }

  // If route is guest-only and user is authenticated
  if (to.meta.guest && auth.isAuthenticated) {
    console.log('[Router] Guest route but user authenticated, redirecting to dashboard')
    return auth.isAdmin ? '/admin/dashboard' : '/client/dashboard'
  }

  // Role-based access control
  if (to.meta.role) {
    if (to.meta.role === 'Admin' && !auth.isAdmin) {
      console.log('[Router] Admin route but user is not admin')
      return '/client/dashboard'
    }
    if (to.meta.role === 'Subscriber' && !auth.isSubscriber) {
      console.log('[Router] Subscriber route but user is not subscriber')
      return '/admin/dashboard'
    }
  }
})

export default router

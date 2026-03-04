// ============ API Response Types ============

export interface ApiResponse<T> {
  success: boolean
  message: string
  data?: T
  errors: string[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// ============ Auth Types ============

export interface LoginRequest {
  username: string
  password: string
  rememberMe: boolean
  deviceName: string
  operatingSystem: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  userType: UserType
  fullName: string
  role: string
  hasActiveSubscription: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export type UserType = 'Admin' | 'Subscriber'

export interface UserData {
  userType: UserType
  fullName: string
  role: string
  hasActiveSubscription: boolean
}

// ============ System User Types ============

export type SystemUserRole = 'Admin' | 'Client'

export interface SystemUser {
  id: string
  username: string
  email: string
  fullName: string
  role: SystemUserRole
  isActive: boolean
  subscriberId: string | null
  subscriberName: string | null
  lastLoginAt: string | null
  createdAt: string
  updatedAt: string | null
}

export interface CreateSystemUserCommand {
  username: string
  email: string
  fullName: string
  password: string
  confirmPassword: string
  role: SystemUserRole
  subscriberId: string | null
}

export interface UpdateSystemUserCommand {
  email: string
  fullName: string
  role: SystemUserRole
  isActive: boolean
}

export interface ResetPasswordCommand {
  newPassword: string
  confirmPassword: string
}

// ============ Subscriber Types ============

export interface Subscriber {
  id: string
  fullName: string
  email: string
  phoneNumber: string
  address: string
  nationalId: string | null
  isActive: boolean
  macAddress: string | null
  ipAddress: string | null
  mikroTikUsername: string | null
  systemUserId: string | null
  createdAt: string
  updatedAt: string | null
  subscriptions?: SubscriberSubscription[]
  pppoeAccounts?: PppoeAccount[]
}

export interface SubscriberSubscription {
  id: string
  planId: string
  planName: string
  status: SubscriptionStatus
  startDate: string
  endDate: string
  activatedAt: string | null
  suspendedAt: string | null
}

export interface PppoeAccount {
  id: string
  username: string
  profileName: string
  isEnabled: boolean
  isSyncedWithMikroTik: boolean
  lastSyncDate: string | null
  validationStatus: string
  isOnline: boolean
  lastConnectedAt: string | null
}

export interface CreateSubscriberCommand {
  fullName: string
  email: string
  phoneNumber: string
  nationalId?: string | null
  address: string
  macAddress?: string | null
  ipAddress?: string | null
  planId?: string | null
  startDate?: string | null
  autoRenew?: boolean
  pppUsername?: string | null
  pppPassword?: string | null
  autoCreateMikroTik?: boolean
  createSystemAccount?: boolean
  systemUsername?: string | null
  systemPassword?: string | null
}

export interface UpdateSubscriberCommand {
  subscriberId?: string
  fullName: string
  email: string
  phoneNumber: string
  nationalId: string | null
  address: string
  macAddress: string | null
  ipAddress: string | null
}

// ============ Subscription Types ============

export type SubscriptionStatus = 'Active' | 'Suspended' | 'Cancelled' | 'Expired' | 'Pending'

export interface Subscription {
  id: string
  subscriberId: string
  planId: string
  planName: string
  startDate: string
  endDate: string
  status: string
  activatedAt: string | null
  suspendedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  remainingDays: number
}

export interface CreateSubscriptionCommand {
  subscriberId: string
  planId: string
  startDate: string
}

// ============ Plan Types ============

export interface Plan {
  id: string
  name: string
  description: string
  price: number
  currency: string
  speedMbps: number
  dataLimitGB: number
  billingCycleDays: number
  billingCycleHours: number | null
  isActive: boolean
  mikroTikProfileName: string
}

export interface CreatePlanCommand {
  name: string
  description: string
  price: number
  currency: string
  speedMbps: number
  dataLimitGB: number
  billingCycleDays: number
  billingCycleHours: number | null
  mikroTikProfileName: string
}

export interface UpdatePlanDto {
  name: string
  description: string
  price: number
  currency: string
  speedMbps: number
  dataLimitGB: number
  billingCycleHours: number | null
}

// ============ Invoice Types ============

export interface Invoice {
  id: string
  invoiceNumber: string
  subscriberId: string
  subscriberName: string
  subscriptionId: string | null
  issueDate: string
  dueDate: string
  subtotal: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
  paidAmount: number
  remainingAmount: number
  status: string
  notes: string | null
  daysOverdue: number
}

export interface GenerateInvoiceCommand {
  subscriberId: string
  subscriptionId: string | null
  issueDate: string
  dueDays: number
  subtotal: number
  taxAmount: number
  discountAmount: number
  notes: string | null
}

// ============ Payment Types ============

export interface Payment {
  id: string
  invoiceId: string
  invoiceNumber: string
  subscriberId: string
  subscriberName: string
  paymentReference: string
  amount: number
  method: string
  status: string
  paymentDate: string
  transactionId: string | null
  notes: string | null
}

export interface ProcessPaymentCommand {
  invoiceId: string
  amount: number
  method: string
  paymentDate: string | null
  transactionId: string | null
  notes: string | null
}

// ============ Online Users / MikroTik Types ============

export interface ActiveSession {
  username: string
  callerId: string
  service: string
  address: string
  uptime: string
  bytesIn: number
  bytesOut: number
}

export interface MikroTikConnectionRequest {
  host: string
  port: number
  username: string
  password: string
}

// ============ Dashboard Types ============

export interface DashboardStats {
  totalSubscribers: number
  activeSubscriptions: number
  monthlyRevenue: number
  overdueInvoices: number
}

export interface RecentActivity {
  date: string
  activity: string
  user: string
  details: string
}

// ============ Report Types ============

export interface RevenueReport {
  month: string
  revenue: number
  payments: number
  invoices: number
}

// ============ Table Column Definition ============

export interface TableColumn<T = Record<string, unknown>> {
  label: string
  field: string
  type?: 'text' | 'date' | 'datetime' | 'currency' | 'custom'
  render?: (row: T) => string
}

// ============ Toast Types ============

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface ToastMessage {
  id: number
  message: string
  type: ToastType
}

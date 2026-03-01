<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { usePaymentsStore } from '@/stores/payments'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const store = usePaymentsStore()
const toast = useToastStore()
const { formatCurrency, formatDate } = useFormatters()

const activeTab = ref<'payments' | 'gateways'>('payments')
const showRecordForm = ref(false)

const recordForm = ref({
  invoiceId: '',
  amount: 0,
  method: 'Cash',
  paymentDate: new Date().toISOString().split('T')[0],
  transactionId: '',
  notes: '',
})

const paymentMethods = [
  { value: 'Cash', label: 'نقدي' },
  { value: 'BankTransfer', label: 'تحويل بنكي' },
  { value: 'Mada', label: 'مدى' },
  { value: 'Visa', label: 'Visa' },
  { value: 'MasterCard', label: 'MasterCard' },
]

const gateways = ref([
  {
    id: 'mada',
    name: 'مدى',
    logo: 'M',
    color: '#00A651',
    enabled: false,
    merchantId: '',
    terminalId: '',
    secretKey: '',
    apiUrl: 'https://api.mada.com.sa/v1',
    mode: 'test' as 'test' | 'live',
  },
  {
    id: 'visa',
    name: 'Visa',
    logo: 'V',
    color: '#1A1F71',
    enabled: false,
    merchantId: '',
    terminalId: '',
    secretKey: '',
    apiUrl: 'https://api.visa.com/v1',
    mode: 'test' as 'test' | 'live',
  },
  {
    id: 'mastercard',
    name: 'MasterCard',
    logo: 'M',
    color: '#EB001B',
    enabled: false,
    merchantId: '',
    terminalId: '',
    secretKey: '',
    apiUrl: 'https://api.mastercard.com/v1',
    mode: 'test' as 'test' | 'live',
  },
])

const totalPayments = computed(() => store.payments.length)
const totalAmount = computed(() => store.payments.reduce((sum, p) => sum + (p.amount || 0), 0))

async function recordPayment() {
  if (!recordForm.value.invoiceId || recordForm.value.amount <= 0) {
    toast.error('الرجاء إدخال رقم الفاتورة والمبلغ')
    return
  }
  try {
    await store.processPayment({
      invoiceId: recordForm.value.invoiceId,
      amount: recordForm.value.amount,
      method: recordForm.value.method,
      paymentDate: recordForm.value.paymentDate || null,
      transactionId: recordForm.value.transactionId || null,
      notes: recordForm.value.notes || null,
    })
    toast.success('تم تسجيل الدفعة بنجاح')
    showRecordForm.value = false
    recordForm.value = { invoiceId: '', amount: 0, method: 'Cash', paymentDate: new Date().toISOString().split('T')[0], transactionId: '', notes: '' }
    store.fetchAll()
  } catch {
    toast.error('فشل تسجيل الدفعة')
  }
}

function saveGatewayConfig() {
  localStorage.setItem('Dushi_payment_gateways', JSON.stringify(gateways.value))
  toast.success('تم حفظ إعدادات بوابات الدفع')
}

function loadGatewayConfig() {
  const saved = localStorage.getItem('Dushi_payment_gateways')
  if (saved) {
    try {
      gateways.value = JSON.parse(saved)
    } catch { /* ignore */ }
  }
}

function getMethodLabel(method: string): string {
  return paymentMethods.find(m => m.value === method)?.label || method
}

onMounted(() => {
  store.fetchAll()
  loadGatewayConfig()
})
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">المدفوعات</h1>
      <div class="flex gap-3">
        <button
          :class="['px-4 py-2 rounded-lg text-sm transition-colors', activeTab === 'payments' ? 'bg-coastal-blue text-white' : 'bg-white border border-sandy-brown/30 text-charcoal hover:bg-soft-beige-light']"
          @click="activeTab = 'payments'"
        >المدفوعات</button>
        <button
          :class="['px-4 py-2 rounded-lg text-sm transition-colors', activeTab === 'gateways' ? 'bg-coastal-blue text-white' : 'bg-white border border-sandy-brown/30 text-charcoal hover:bg-soft-beige-light']"
          @click="activeTab = 'gateways'"
        >بوابات الدفع</button>
      </div>
    </div>

    <!-- ============ PAYMENTS TAB ============ -->
    <div v-if="activeTab === 'payments'">
      <!-- Stats -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige">
          <p class="text-sm text-light-gray">إجمالي المدفوعات</p>
          <p class="text-2xl font-bold text-coastal-blue">{{ totalPayments }}</p>
        </div>
        <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige">
          <p class="text-sm text-light-gray">إجمالي المبالغ</p>
          <p class="text-2xl font-bold text-jazan-green">{{ formatCurrency(totalAmount) }}</p>
        </div>
        <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige flex items-center justify-center">
          <button
            class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors shadow-md text-sm"
            @click="showRecordForm = !showRecordForm"
          >
            {{ showRecordForm ? 'إلغاء' : '+ تسجيل دفعة' }}
          </button>
        </div>
      </div>

      <!-- Record Payment Form -->
      <div v-if="showRecordForm" class="bg-white rounded-xl shadow-md p-6 mb-6 border border-jazan-green">
        <h3 class="text-lg font-semibold text-jazan-green mb-4">تسجيل دفعة جديدة</h3>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">معرف الفاتورة</label>
            <input v-model="recordForm.invoiceId" type="text" placeholder="GUID الفاتورة" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">المبلغ</label>
            <input v-model.number="recordForm.amount" type="number" min="0" step="0.01" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">طريقة الدفع</label>
            <select v-model="recordForm.method" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent">
              <option v-for="m in paymentMethods" :key="m.value" :value="m.value">{{ m.label }}</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">تاريخ الدفع</label>
            <input v-model="recordForm.paymentDate" type="date" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">رقم العملية</label>
            <input v-model="recordForm.transactionId" type="text" placeholder="اختياري" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">ملاحظات</label>
            <input v-model="recordForm.notes" type="text" placeholder="اختياري" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          </div>
        </div>
        <div class="mt-4 flex justify-end">
          <button class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors text-sm" @click="recordPayment">تسجيل الدفعة</button>
        </div>
      </div>

      <!-- Payments Table -->
      <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
        <div class="overflow-x-auto">
          <table v-if="store.payments.length" class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المرجع</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المشترك</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الفاتورة</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المبلغ</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الطريقة</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">التاريخ</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="p in store.payments" :key="p.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ p.paymentReference }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ p.subscriberName }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ p.invoiceNumber }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-charcoal font-semibold">{{ formatCurrency(p.amount) }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ getMethodLabel(p.method) }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ formatDate(p.paymentDate) }}</td>
                <td class="px-6 py-4 whitespace-nowrap"><StatusBadge :status="p.status" /></td>
              </tr>
            </tbody>
          </table>
          <div v-else class="text-center py-12 text-light-gray">لا توجد مدفوعات</div>
        </div>
      </div>
    </div>

    <!-- ============ GATEWAYS TAB ============ -->
    <div v-if="activeTab === 'gateways'">
      <div class="space-y-6">
        <div v-for="gw in gateways" :key="gw.id" class="bg-white rounded-xl shadow-md border border-soft-beige overflow-hidden">
          <!-- Gateway Header -->
          <div class="flex items-center justify-between p-6 border-b border-gray-100">
            <div class="flex items-center gap-4">
              <div class="h-12 w-12 rounded-xl flex items-center justify-center text-white font-bold text-lg" :style="{ backgroundColor: gw.color }">
                {{ gw.logo }}
              </div>
              <div>
                <h3 class="text-lg font-semibold text-charcoal">{{ gw.name }}</h3>
                <p class="text-sm text-light-gray">بوابة الدفع الإلكتروني</p>
              </div>
            </div>
            <div class="flex items-center gap-3">
              <span :class="['text-xs font-medium px-2 py-1 rounded-full', gw.mode === 'live' ? 'bg-jazan-green/10 text-jazan-green' : 'bg-warning-yellow/10 text-golden-sand-dark']">
                {{ gw.mode === 'live' ? 'إنتاج' : 'تجريبي' }}
              </span>
              <label class="relative inline-flex items-center cursor-pointer">
                <input v-model="gw.enabled" type="checkbox" class="sr-only peer" />
                <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-jazan-green rounded-full peer peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-jazan-green"></div>
              </label>
            </div>
          </div>

          <!-- Gateway Config -->
          <div v-if="gw.enabled" class="p-6 space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-charcoal mb-1">معرف التاجر (Merchant ID)</label>
                <input v-model="gw.merchantId" type="text" placeholder="أدخل معرف التاجر" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent font-mono" />
              </div>
              <div>
                <label class="block text-sm font-medium text-charcoal mb-1">معرف الطرفية (Terminal ID)</label>
                <input v-model="gw.terminalId" type="text" placeholder="أدخل معرف الطرفية" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent font-mono" />
              </div>
              <div>
                <label class="block text-sm font-medium text-charcoal mb-1">المفتاح السري (Secret Key)</label>
                <input v-model="gw.secretKey" type="password" placeholder="أدخل المفتاح السري" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent font-mono" />
              </div>
              <div>
                <label class="block text-sm font-medium text-charcoal mb-1">رابط API</label>
                <input v-model="gw.apiUrl" type="url" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent font-mono" />
              </div>
            </div>
            <div class="flex items-center gap-4">
              <label class="block text-sm font-medium text-charcoal">الوضع:</label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input v-model="gw.mode" type="radio" value="test" class="text-coastal-blue focus:ring-coastal-blue" />
                <span class="text-sm text-charcoal">تجريبي</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input v-model="gw.mode" type="radio" value="live" class="text-jazan-green focus:ring-jazan-green" />
                <span class="text-sm text-charcoal">إنتاج</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Save Button -->
        <div class="flex justify-end">
          <button class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors shadow-md" @click="saveGatewayConfig">
            حفظ إعدادات بوابات الدفع
          </button>
        </div>

        <!-- Info Box -->
        <div class="bg-coastal-blue/5 border border-coastal-blue/20 rounded-xl p-4">
          <h4 class="text-sm font-semibold text-coastal-blue mb-2">ملاحظات مهمة</h4>
          <ul class="text-sm text-light-gray space-y-1 list-disc list-inside">
            <li>يتم حفظ إعدادات بوابات الدفع محلياً. في بيئة الإنتاج يجب حفظها في الخادم.</li>
            <li>استخدم الوضع التجريبي أولاً للتأكد من صحة الإعدادات قبل التحويل للإنتاج.</li>
            <li>المفاتيح السرية يجب أن تبقى سرية ولا يتم مشاركتها.</li>
            <li>عند تفعيل بوابة الدفع، سيتمكن العملاء من الدفع عبرها من لوحة التحكم الخاصة بهم.</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

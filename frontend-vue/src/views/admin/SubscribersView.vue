<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSubscribersStore } from '@/stores/subscribers'
import { useToastStore } from '@/stores/toast'
import { useDebounceFn } from '@vueuse/core'
import AppLoader from '@/components/AppLoader.vue'
import AppPagination from '@/components/AppPagination.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const store = useSubscribersStore()
const toast = useToastStore()
const router = useRouter()

const search = ref('')
const statusFilter = ref('')
const confirmVisible = ref(false)
const pendingSuspendId = ref('')

const debouncedSearch = useDebounceFn(() => {
  store.fetchSubscribers(1, 20, search.value, statusFilter.value === 'Active' ? true : undefined)
}, 500)

function onSearchInput() {
  debouncedSearch()
}

function onStatusChange() {
  store.fetchSubscribers(1, 20, search.value, statusFilter.value === 'Active' ? true : undefined)
}

function goToPage(page: number) {
  store.fetchSubscribers(page, 20, search.value, statusFilter.value === 'Active' ? true : undefined)
}

function confirmSuspend(id: string) {
  pendingSuspendId.value = id
  confirmVisible.value = true
}

async function handleSuspend() {
  confirmVisible.value = false
  try {
    await store.suspendSubscriber(pendingSuspendId.value)
    toast.success('تم إيقاف المشترك بنجاح')
    store.fetchSubscribers(store.currentPage, 20, search.value)
  } catch {
    toast.error('فشل إيقاف المشترك')
  }
}

onMounted(() => store.fetchSubscribers())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <ConfirmDialog
    :visible="confirmVisible"
    title="إيقاف المشترك"
    message="هل أنت متأكد من إيقاف هذا المشترك؟"
    @confirm="handleSuspend"
    @cancel="confirmVisible = false"
  />

  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">المشتركون</h1>
      <button
        class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors shadow-md"
        @click="router.push('/admin/subscribers/new')"
      >
        + إضافة مشترك
      </button>
    </div>

    <div class="bg-white rounded-xl shadow-md border border-soft-beige">
      <div class="p-4 border-b border-soft-beige flex flex-col sm:flex-row gap-4">
        <input
          v-model="search"
          type="text"
          placeholder="بحث بالاسم أو البريد..."
          class="flex-1 px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"
          @input="onSearchInput"
        />
        <select
          v-model="statusFilter"
          class="px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"
          @change="onStatusChange"
        >
          <option value="">جميع الحالات</option>
          <option value="Active">نشط</option>
          <option value="Suspended">موقوف</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table v-if="store.subscribers.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الاسم</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">البريد</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الهاتف</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="sub in store.subscribers" :key="sub.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-charcoal font-medium">{{ sub.fullName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ sub.email }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ sub.phoneNumber }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <StatusBadge :status="sub.isActive ? 'Active' : 'Suspended'" />
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <div class="flex gap-2">
                  <button class="text-coastal-blue hover:text-coastal-blue-dark font-medium" @click="router.push(`/admin/subscribers/${sub.id}`)">
                    عرض
                  </button>
                  <button class="text-jazan-green hover:text-jazan-green-dark font-medium" @click="router.push(`/admin/subscribers/${sub.id}/edit`)">
                    تعديل
                  </button>
                  <button v-if="sub.isActive" class="text-golden-sand-dark hover:text-golden-sand font-medium" @click="confirmSuspend(sub.id)">
                    إيقاف
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="text-center py-12 text-light-gray">لا توجد بيانات متاحة</div>
      </div>

      <AppPagination :current-page="store.currentPage" :total-pages="store.totalPages" @page-change="goToPage" />
    </div>
  </div>
</template>

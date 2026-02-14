<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePlansStore } from '@/stores/plans'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const store = usePlansStore()
const toast = useToastStore()
const router = useRouter()
const { formatCurrency } = useFormatters()

const showAll = ref(false)
const confirmVisible = ref(false)
const pendingDeleteId = ref('')

function toggleFilter() {
  showAll.value = !showAll.value
  store.fetchPlans(!showAll.value)
}

function confirmDelete(id: string) {
  pendingDeleteId.value = id
  confirmVisible.value = true
}

async function handleDelete() {
  confirmVisible.value = false
  try {
    await store.deletePlan(pendingDeleteId.value)
    toast.success('تم إلغاء تفعيل الباقة بنجاح')
    store.fetchPlans(!showAll.value)
  } catch {
    toast.error('فشل إلغاء تفعيل الباقة')
  }
}

onMounted(() => store.fetchPlans())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <ConfirmDialog
    :visible="confirmVisible"
    title="إلغاء تفعيل الباقة"
    message="هل أنت متأكد من إلغاء تفعيل هذه الباقة؟"
    @confirm="handleDelete"
    @cancel="confirmVisible = false"
  />

  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">الباقات</h1>
      <div class="flex gap-3">
        <button
          class="px-4 py-2 border border-sandy-brown/30 rounded-lg hover:bg-soft-beige-light text-sm text-charcoal transition-colors"
          @click="toggleFilter"
        >
          {{ showAll ? 'النشطة فقط' : 'عرض الكل' }}
        </button>
        <button
          class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors shadow-md"
          @click="router.push('/admin/plans/new')"
        >
          + إضافة باقة
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="plan in store.plans" :key="plan.id" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-coastal-blue">{{ plan.name }}</h3>
          <StatusBadge :status="plan.isActive ? 'Active' : 'Expired'" />
        </div>
        <p class="text-light-gray text-sm mb-4">{{ plan.description }}</p>
        <div class="space-y-2 text-sm text-charcoal">
          <div class="flex justify-between"><span>السعر:</span><span class="font-semibold">{{ formatCurrency(plan.price) }}</span></div>
          <div class="flex justify-between"><span>السرعة:</span><span>{{ plan.speedMbps }} Mbps</span></div>
          <div class="flex justify-between"><span>حد البيانات:</span><span>{{ plan.dataLimitGB }} GB</span></div>
          <div class="flex justify-between"><span>دورة الفوترة:</span><span>{{ plan.billingCycleDays }} يوم</span></div>
        </div>
        <div class="flex gap-2 mt-4 pt-4 border-t border-gray-200">
          <button class="flex-1 px-3 py-2 text-sm bg-coastal-blue/10 text-coastal-blue rounded-lg hover:bg-coastal-blue/20 transition-colors" @click="router.push(`/admin/plans/${plan.id}/edit`)">تعديل</button>
          <button v-if="plan.isActive" class="flex-1 px-3 py-2 text-sm bg-red-coral/10 text-red-coral rounded-lg hover:bg-red-coral/20 transition-colors" @click="confirmDelete(plan.id)">إلغاء التفعيل</button>
        </div>
      </div>
    </div>

    <div v-if="!store.plans.length && !store.isLoading" class="text-center py-12 text-light-gray">
      لا توجد باقات متاحة
    </div>
  </div>
</template>

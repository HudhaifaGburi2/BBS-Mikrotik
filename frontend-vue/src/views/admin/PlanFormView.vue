<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePlansStore } from '@/stores/plans'
import { useToastStore } from '@/stores/toast'
import { useValidation } from '@/composables/useValidation'
import FormField from '@/components/FormField.vue'
import AppLoader from '@/components/AppLoader.vue'

const route = useRoute()
const router = useRouter()
const store = usePlansStore()
const toast = useToastStore()
const { errors, sanitize, clearErrors } = useValidation()

const isEdit = computed(() => !!route.params.id)
const isLoading = ref(false)

const form = ref({
  name: '',
  description: '',
  price: 0,
  currency: 'USD',
  speedMbps: 0,
  dataLimitGB: 0,
  billingCycleDays: 30,
  mikroTikProfileName: '',
})

function validate(): boolean {
  clearErrors()
  if (!form.value.name.trim()) errors.value.name = 'اسم الباقة مطلوب'
  if (form.value.price <= 0) errors.value.price = 'السعر يجب أن يكون أكبر من 0'
  if (form.value.speedMbps <= 0) errors.value.speedMbps = 'السرعة يجب أن تكون أكبر من 0'
  if (form.value.billingCycleDays <= 0) errors.value.billingCycleDays = 'دورة الفوترة يجب أن تكون يوم واحد على الأقل'
  if (!isEdit.value && !form.value.mikroTikProfileName.trim()) errors.value.mikroTikProfileName = 'اسم بروفايل MikroTik مطلوب'
  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validate()) {
    toast.error('الرجاء تصحيح الأخطاء في النموذج')
    return
  }

  isLoading.value = true
  try {
    if (isEdit.value) {
      await store.updatePlan(route.params.id as string, {
        name: sanitize(form.value.name),
        description: sanitize(form.value.description),
        price: form.value.price,
        currency: form.value.currency,
        speedMbps: form.value.speedMbps,
        dataLimitGB: form.value.dataLimitGB,
      })
      toast.success('تم تحديث الباقة بنجاح')
    } else {
      await store.createPlan({
        name: sanitize(form.value.name),
        description: sanitize(form.value.description),
        price: form.value.price,
        currency: form.value.currency,
        speedMbps: form.value.speedMbps,
        dataLimitGB: form.value.dataLimitGB,
        billingCycleDays: form.value.billingCycleDays,
        mikroTikProfileName: sanitize(form.value.mikroTikProfileName),
      })
      toast.success('تم إنشاء الباقة بنجاح')
    }
    router.push('/admin/plans')
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'فشل حفظ الباقة'
    toast.error(message)
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  if (isEdit.value) {
    isLoading.value = true
    try {
      await store.fetchPlan(route.params.id as string)
      const plan = store.currentPlan
      if (plan) {
        form.value.name = plan.name
        form.value.description = plan.description
        form.value.price = plan.price
        form.value.currency = plan.currency
        form.value.speedMbps = plan.speedMbps
        form.value.dataLimitGB = plan.dataLimitGB
        form.value.billingCycleDays = plan.billingCycleDays
        form.value.mikroTikProfileName = plan.mikroTikProfileName
      }
    } finally {
      isLoading.value = false
    }
  }
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="max-w-3xl mx-auto">
    <div class="bg-white rounded-xl shadow-md p-8 border border-soft-beige">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-coastal-blue">{{ isEdit ? 'تعديل الباقة' : 'إضافة باقة' }}</h2>
        <RouterLink to="/admin/plans" class="text-light-gray hover:text-charcoal">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </RouterLink>
      </div>

      <form class="space-y-6" @submit.prevent="handleSubmit">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="اسم الباقة" :error="errors.name" required>
            <input v-model="form.name" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="السعر" :error="errors.price" required>
            <input v-model.number="form.price" type="number" min="0" step="0.01" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="السرعة (Mbps)" :error="errors.speedMbps" required>
            <input v-model.number="form.speedMbps" type="number" min="1" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="حد البيانات (GB)">
            <input v-model.number="form.dataLimitGB" type="number" min="0" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="دورة الفوترة (أيام)" :error="errors.billingCycleDays" required>
            <input v-model.number="form.billingCycleDays" type="number" min="1" max="365" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="اسم بروفايل MikroTik" :error="errors.mikroTikProfileName" :required="!isEdit">
            <input v-model="form.mikroTikProfileName" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
        </div>

        <FormField label="الوصف">
          <textarea v-model="form.description" rows="3" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"></textarea>
        </FormField>

        <div class="flex items-center justify-end gap-4 pt-6 border-t border-gray-200">
          <RouterLink to="/admin/plans" class="px-6 py-2 bg-pale-olive/30 text-charcoal rounded-lg hover:bg-pale-olive/50 transition-colors">إلغاء</RouterLink>
          <button type="submit" :disabled="isLoading" class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors disabled:opacity-50 shadow-md">حفظ الباقة</button>
        </div>
      </form>
    </div>
  </div>
</template>

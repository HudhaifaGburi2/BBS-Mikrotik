export function useFormatters() {
  function formatCurrency(amount: number, currency = 'SAR'): string {
    return new Intl.NumberFormat('ar-SA', {
      style: 'currency',
      currency,
    }).format(amount)
  }

  function formatDate(dateString: string | null | undefined): string {
    if (!dateString) return '-'
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(new Date(dateString))
  }

  function formatDateTime(dateString: string | null | undefined): string {
    if (!dateString) return '-'
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(dateString))
  }

  function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
  }

  return { formatCurrency, formatDate, formatDateTime, formatBytes }
}

'use client'

import Link from 'next/link'

import { AdminNav } from '@/components/admin/AdminNav'
import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/features/auth/store'
import { useCurrentUser } from '@/hooks/useCurrentUser'

export default function AdminLayout({ children }: LayoutProps<'/admin'>) {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const user = useCurrentUser()

  if (!isAuthenticated || user?.role !== 'Admin') {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
        <p className="text-muted-foreground">Bu sayfayı görüntülemek için yönetici olarak giriş yapmalısınız.</p>
        <Button render={<Link href="/login" />} nativeButton={false}>
          Giriş Yap
        </Button>
      </div>
    )
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-8 md:flex-row md:gap-8">
      <AdminNav />
      <div className="min-w-0 flex-1">{children}</div>
    </div>
  )
}

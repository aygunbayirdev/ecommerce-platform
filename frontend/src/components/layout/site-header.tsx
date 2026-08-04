'use client'

import { LogOut, ShoppingCart, User } from 'lucide-react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'

import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useAuthStore } from '@/features/auth/store'
import { useCurrentUser } from '@/hooks/useCurrentUser'

export function SiteHeader() {
  const router = useRouter()
  const user = useCurrentUser()
  const clear = useAuthStore((state) => state.clear)

  function handleLogout() {
    clear()
    router.push('/')
  }

  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Link href="/" className="text-lg font-bold tracking-tight">
          ECommercePlatform
        </Link>

        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="icon"
            aria-label="Sepetim"
            nativeButton={false}
            render={<Link href="/cart" />}
          >
            <ShoppingCart className="size-5" />
          </Button>

          {user ? (
            <DropdownMenu>
              <DropdownMenuTrigger
                render={<Button variant="ghost" size="icon" aria-label="Hesabım" />}
              >
                <User className="size-5" />
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem render={<Link href="/orders" />}>Siparişlerim</DropdownMenuItem>
                <DropdownMenuItem render={<Link href="/account/addresses" />}>
                  Adreslerim
                </DropdownMenuItem>
                {user.role === 'Admin' && (
                  <DropdownMenuItem render={<Link href="/admin" />}>Admin Panel</DropdownMenuItem>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout} variant="destructive">
                  <LogOut className="size-4" />
                  Çıkış Yap
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Button variant="outline" nativeButton={false} render={<Link href="/login" />}>
              Giriş Yap
            </Button>
          )}
        </div>
      </div>
    </header>
  )
}

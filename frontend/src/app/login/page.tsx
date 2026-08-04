'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { type FormEvent, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useLogin } from '@/features/auth/api/useLogin'
import { getApiErrorMessage } from '@/lib/errors'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const router = useRouter()
  const loginMutation = useLogin()

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    loginMutation.mutate(
      { email, password },
      { onSuccess: () => router.push('/') },
    )
  }

  return (
    <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-sm items-center justify-center px-4">
      <form onSubmit={handleSubmit} className="w-full space-y-4 rounded-lg border p-6">
        <h1 className="text-lg font-semibold">Giriş Yap</h1>

        <div className="space-y-1.5">
          <Label htmlFor="email">E-posta</Label>
          <Input
            id="email"
            type="email"
            required
            autoComplete="username"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password">Şifre</Label>
          <Input
            id="password"
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {loginMutation.isError && (
          <p className="text-sm text-destructive">
            {getApiErrorMessage(loginMutation.error, 'Giriş yapılamadı. Lütfen tekrar deneyin.')}
          </p>
        )}

        <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
          {loginMutation.isPending ? 'Giriş yapılıyor...' : 'Giriş Yap'}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          Hesabınız yok mu?{' '}
          <Link href="/register" className="font-medium text-foreground underline-offset-4 hover:underline">
            Kayıt olun
          </Link>
        </p>
      </form>
    </div>
  )
}

'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { type FormEvent, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useLogin } from '@/features/auth/api/useLogin'
import { useRegister } from '@/features/auth/api/useRegister'
import { getApiErrorMessage } from '@/lib/errors'

export default function RegisterPage() {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [password, setPassword] = useState('')
  const router = useRouter()
  const registerMutation = useRegister()
  const loginMutation = useLogin()

  const isPending = registerMutation.isPending || loginMutation.isPending

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    registerMutation.mutate(
      { email, password, firstName, lastName, phoneNumber: phoneNumber || undefined },
      {
        // Registration only returns the new user's id — log in right away with the
        // same credentials instead of sending the customer back through a second form.
        onSuccess: () => {
          loginMutation.mutate({ email, password }, { onSuccess: () => router.push('/') })
        },
      },
    )
  }

  return (
    <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-sm items-center justify-center px-4 py-8">
      <form onSubmit={handleSubmit} className="w-full space-y-4 rounded-lg border p-6">
        <h1 className="text-lg font-semibold">Hesap Oluştur</h1>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <Label htmlFor="firstName">Ad</Label>
            <Input
              id="firstName"
              required
              value={firstName}
              onChange={(event) => setFirstName(event.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="lastName">Soyad</Label>
            <Input
              id="lastName"
              required
              value={lastName}
              onChange={(event) => setLastName(event.target.value)}
            />
          </div>
        </div>

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
          <Label htmlFor="phoneNumber">Telefon (opsiyonel)</Label>
          <Input
            id="phoneNumber"
            type="tel"
            value={phoneNumber}
            onChange={(event) => setPhoneNumber(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password">Şifre</Label>
          <Input
            id="password"
            type="password"
            required
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {registerMutation.isError && (
          <p className="text-sm text-destructive">
            {getApiErrorMessage(registerMutation.error, 'Kayıt oluşturulamadı. Lütfen tekrar deneyin.')}
          </p>
        )}

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending ? 'Hesap oluşturuluyor...' : 'Hesap Oluştur'}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          Zaten hesabınız var mı?{' '}
          <Link href="/login" className="font-medium text-foreground underline-offset-4 hover:underline">
            Giriş yapın
          </Link>
        </p>
      </form>
    </div>
  )
}

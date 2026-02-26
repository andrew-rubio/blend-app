import Link from 'next/link'

export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center text-center">
      <h2 className="text-2xl font-semibold text-gray-900">Page not found</h2>
      <p className="mt-2 text-gray-500">The page you are looking for does not exist.</p>
      <Link href="/" className="mt-6 rounded-lg bg-orange-500 px-6 py-2 text-white hover:bg-orange-600">
        Go home
      </Link>
    </div>
  )
}

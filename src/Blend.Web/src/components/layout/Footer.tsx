import Link from 'next/link'

export function Footer() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="border-t border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-950">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
          <div className="flex items-center gap-2">
            <span className="text-lg font-bold text-primary-600">Blend</span>
            <span className="text-sm text-gray-500 dark:text-gray-400">
              &copy; {currentYear}
            </span>
          </div>

          <nav className="flex items-center gap-6">
            <Link
              href="/about"
              className="text-sm text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
            >
              About
            </Link>
            <Link
              href="/privacy"
              className="text-sm text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
            >
              Privacy
            </Link>
            <Link
              href="/terms"
              className="text-sm text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
            >
              Terms
            </Link>
          </nav>
        </div>
      </div>
    </footer>
  )
}

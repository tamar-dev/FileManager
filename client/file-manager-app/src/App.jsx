import { Toaster } from "@/components/ui/toaster"
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClientInstance } from '@/lib/query-client'
import {
  BrowserRouter as Router,
  Route,
  Routes,
  Navigate
} from 'react-router-dom'

import PageNotFound from './lib/PageNotFound'
import ScrollToTop from './components/ScrollToTop'
import { ThemeProvider } from '@/lib/ThemeContext'
import Layout from '@/components/Layout'

import { AuthProvider, useAuth } from '@/lib/AuthContext'
import UserNotRegisteredError from '@/components/UserNotRegisteredError'
import ProtectedRoute from '@/components/ProtectedRoute'

import Dashboard from '@/pages/Dashboard'
import SearchPage from '@/pages/Search'
import VirtualFolders from '@/pages/VirtualFolders'
import Duplicates from '@/pages/Duplicates'
import Photos from '@/pages/Photos'
import Recent from '@/pages/Recent'
import Favorites from '@/pages/Favorites'
import IndexStatus from '@/pages/IndexStatus'
import Settings from '@/pages/Settings'
import VirtualExplorer from '@/pages/VirtualExplorer'

import Login from '@/pages/Login'
import Register from '@/pages/Register'
import ForgotPassword from '@/pages/ForgotPassword'
import ResetPassword from '@/pages/ResetPassword'

const authEnabled =
  import.meta.env.VITE_AUTH_ENABLED === 'true'


const AppPages = () => (
  <>
    <Route path="/" element={<Dashboard />} />
    <Route path="/virtual-explorer" element={<VirtualExplorer />} />
    <Route path="/search" element={<SearchPage />} />
    <Route path="/virtual-folders" element={<VirtualFolders />} />
    <Route path="/duplicates" element={<Duplicates />} />
    <Route path="/photos" element={<Photos />} />
    <Route path="/recent" element={<Recent />} />
    <Route path="/favorites" element={<Favorites />} />
    <Route path="/index-status" element={<IndexStatus />} />
    <Route path="/settings" element={<Settings />} />
  </>
)


const LocalRoutes = () => (
  <Routes>
    <Route element={<Layout />}>
      <Route path="/" element={<Dashboard />} />
      <Route path="/virtual-explorer" element={<VirtualExplorer />} />
      <Route path="/search" element={<SearchPage />} />
      <Route path="/virtual-folders" element={<VirtualFolders />} />
      <Route path="/duplicates" element={<Duplicates />} />
      <Route path="/photos" element={<Photos />} />
      <Route path="/recent" element={<Recent />} />
      <Route path="/favorites" element={<Favorites />} />
      <Route path="/index-status" element={<IndexStatus />} />
      <Route path="/settings" element={<Settings />} />
    </Route>

    <Route path="*" element={<PageNotFound />} />
  </Routes>
)


const AuthenticatedRoutes = () => {
  const {
    isLoadingAuth,
    isLoadingPublicSettings,
    authError,
    navigateToLogin
  } = useAuth()

  if (isLoadingPublicSettings || isLoadingAuth) {
    return (
      <div className="fixed inset-0 flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-slate-200 border-t-slate-800 rounded-full animate-spin" />
      </div>
    )
  }

  if (authError?.type === 'user_not_registered') {
    return <UserNotRegisteredError />
  }

  if (authError?.type === 'auth_required') {
    navigateToLogin()
    return null
  }

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/reset-password" element={<ResetPassword />} />

      <Route
        element={
          <ProtectedRoute
            unauthenticatedElement={
              <Navigate to="/login" replace />
            }
          />
        }
      >
        <Route element={<Layout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/virtual-explorer" element={<VirtualExplorer />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/virtual-folders" element={<VirtualFolders />} />
          <Route path="/duplicates" element={<Duplicates />} />
          <Route path="/photos" element={<Photos />} />
          <Route path="/recent" element={<Recent />} />
          <Route path="/favorites" element={<Favorites />} />
          <Route path="/index-status" element={<IndexStatus />} />
          <Route path="/settings" element={<Settings />} />
        </Route>
      </Route>

      <Route path="*" element={<PageNotFound />} />
    </Routes>
  )
}


function AppShell() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClientInstance}>
        <Router>
          <ScrollToTop />

          {authEnabled
            ? <AuthenticatedRoutes />
            : <LocalRoutes />
          }
        </Router>

        <Toaster />
      </QueryClientProvider>
    </ThemeProvider>
  )
}


function App() {
  if (authEnabled) {
    return (
      <AuthProvider>
        <AppShell />
      </AuthProvider>
    )
  }

  return <AppShell />
}

export default App
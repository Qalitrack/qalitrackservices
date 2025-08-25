import Link from 'next/link'
import { Button } from '@/components/ui/button'

export default function HomePage() {
  return (
    <main className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center max-w-4xl mx-auto">
          {/* Hero Section */}
          <div className="mb-16">
            <h1 className="text-6xl font-bold text-gray-900 mb-6">
              🎯 <span className="text-primary">Qasana</span>
            </h1>
            <p className="text-xl text-gray-600 mb-8 leading-relaxed">
              The modern project management platform that helps teams organize, 
              track, and manage their work with clarity and efficiency.
            </p>
            <div className="flex gap-4 justify-center">
              <Button size="lg" asChild>
                <Link href="/dashboard">Get Started</Link>
              </Button>
              <Button variant="outline" size="lg" asChild>
                <Link href="/login">Sign In</Link>
              </Button>
            </div>
          </div>

          {/* Features Grid */}
          <div className="grid md:grid-cols-3 gap-8 mb-16">
            <div className="bg-white p-8 rounded-xl shadow-sm border">
              <div className="text-3xl mb-4">📋</div>
              <h3 className="text-xl font-semibold mb-3">Project Management</h3>
              <p className="text-gray-600">
                Organize your projects with customizable boards, lists, and cards. 
                Track progress and collaborate seamlessly.
              </p>
            </div>
            
            <div className="bg-white p-8 rounded-xl shadow-sm border">
              <div className="text-3xl mb-4">👥</div>
              <h3 className="text-xl font-semibold mb-3">Team Collaboration</h3>
              <p className="text-gray-600">
                Work together in real-time with comments, mentions, and 
                notifications. Keep everyone in sync.
              </p>
            </div>
            
            <div className="bg-white p-8 rounded-xl shadow-sm border">
              <div className="text-3xl mb-4">📊</div>
              <h3 className="text-xl font-semibold mb-3">Progress Tracking</h3>
              <p className="text-gray-600">
                Monitor deadlines, track milestones, and visualize progress 
                with intuitive dashboards and reports.
              </p>
            </div>
          </div>

          {/* Wireframes Preview */}
          <div className="bg-white rounded-xl shadow-lg border p-8">
            <h2 className="text-3xl font-bold mb-6">Explore Our Design</h2>
            <p className="text-gray-600 mb-8">
              Take a look at our carefully crafted wireframes that showcase the user experience.
            </p>
            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
              <Link 
                href="/wireframes/index.html" 
                target="_blank"
                className="p-4 border rounded-lg hover:shadow-md transition-shadow bg-gray-50"
              >
                <div className="text-2xl mb-2">📊</div>
                <h4 className="font-medium">Dashboard</h4>
                <p className="text-sm text-gray-600">Overview and stats</p>
              </Link>
              
              <Link 
                href="/wireframes/project-board.html" 
                target="_blank"
                className="p-4 border rounded-lg hover:shadow-md transition-shadow bg-gray-50"
              >
                <div className="text-2xl mb-2">📋</div>
                <h4 className="font-medium">Project Board</h4>
                <p className="text-sm text-gray-600">Kanban workflow</p>
              </Link>
              
              <Link 
                href="/wireframes/task-detail.html" 
                target="_blank"
                className="p-4 border rounded-lg hover:shadow-md transition-shadow bg-gray-50"
              >
                <div className="text-2xl mb-2">📝</div>
                <h4 className="font-medium">Task Detail</h4>
                <p className="text-sm text-gray-600">Task management</p>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}
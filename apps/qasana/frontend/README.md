# Qasana Frontend

The frontend application for Qasana - a modern project management platform built with Next.js 14.

## Overview

A responsive, modern web application that provides an intuitive interface for project management, task tracking, and team collaboration.

## Technology Stack

### Core Framework
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type safety and developer experience
- **React 18** - User interface library

### Styling & UI
- **Tailwind CSS** - Utility-first CSS framework
- **Radix UI** - Unstyled, accessible UI components
- **Lucide React** - Beautiful icon library
- **class-variance-authority** - Component variant management
- **Framer Motion** - Smooth animations

### State Management
- **Redux Toolkit** - Predictable state management
- **React Query** - Server state management and caching
- **React Hook Form** - Form state management
- **Zod** - Schema validation

### Features & Functionality
- **Axios** - HTTP client for API communication
- **React Hot Toast** - Beautiful toast notifications
- **DND Kit** - Drag and drop functionality
- **date-fns** - Date manipulation utilities

## Project Structure

```
src/
├── app/                 # Next.js 14 App Router
│   ├── globals.css     # Global styles and CSS variables
│   ├── layout.tsx      # Root layout component
│   └── page.tsx        # Home page
├── components/         # React components
│   ├── ui/            # Base UI components (buttons, inputs, etc.)
│   ├── layout/        # Layout components (header, sidebar, etc.)
│   └── features/      # Feature-specific components
├── lib/               # Utility libraries and configurations
│   └── utils.ts       # Common utility functions
├── store/             # Redux store configuration
├── hooks/             # Custom React hooks
├── types/             # TypeScript type definitions
└── utils/             # Helper functions and utilities
```

## Features

### 🏠 Landing Page
- Modern, responsive design
- Feature showcase
- Direct access to wireframes
- Call-to-action buttons

### 🎨 Design System
- Consistent color palette with light/dark mode support
- Reusable UI components built on Radix UI primitives
- Responsive design patterns
- Accessible components by default

### 📱 Responsive Design
- Mobile-first approach
- Tablet and desktop optimized layouts
- Touch-friendly interactions
- Progressive enhancement

### 🔧 Developer Experience
- TypeScript for type safety
- ESLint for code quality
- Hot reloading in development
- Optimized build process

## Getting Started

### Prerequisites
- Node.js 18 or higher
- pnpm (preferred package manager)

### Installation

1. **Install dependencies**:
   ```bash
   pnpm install
   ```

2. **Start development server**:
   ```bash
   pnpm dev
   ```

3. **Open in browser**:
   Navigate to [http://localhost:3000](http://localhost:3000)

### Available Scripts

- `pnpm dev` - Start development server
- `pnpm build` - Build for production
- `pnpm start` - Start production server
- `pnpm lint` - Run ESLint
- `pnpm type-check` - Run TypeScript compiler checks

## Environment Variables

Create a `.env.local` file in the root directory:

```bash
NEXT_PUBLIC_API_URL=http://localhost:7000
NEXT_PUBLIC_WS_URL=ws://localhost:7000
```

## Wireframes Integration

The application includes integrated access to HTML wireframes that showcase the complete user experience:

- **Dashboard** - Project overview and statistics
- **Workspace** - Team and project management
- **Project Board** - Kanban-style task management
- **Task Detail** - Comprehensive task editing
- **Team Management** - Member and role management
- **Settings** - User preferences and configuration

Access wireframes via the homepage or directly at `/wireframes/`

## API Integration

The frontend is designed to integrate with the Qasana backend microservices:

- **Workspace Service** - Team and workspace management
- **Project Service** - Project operations
- **Task Service** - Task management
- **Comment Service** - Comments and activity feeds
- **Notification Service** - Real-time updates

## Future Enhancements

### Planned Features
- [ ] Authentication and user management
- [ ] Real-time collaboration via WebSockets
- [ ] Advanced task management (drag & drop, filtering)
- [ ] File attachments and media handling
- [ ] Advanced search and filtering
- [ ] Mobile application (React Native)
- [ ] Offline support with service workers
- [ ] Advanced reporting and analytics

### Performance Optimizations
- [ ] Image optimization and lazy loading
- [ ] Code splitting and lazy component loading
- [ ] Service worker for caching
- [ ] Bundle analysis and optimization

## Contributing

1. Follow the existing code style and conventions
2. Use TypeScript for all new components
3. Ensure responsive design on all screen sizes
4. Write meaningful commit messages
5. Test thoroughly before submitting changes

## Design Philosophy

The Qasana frontend follows these principles:

- **User-Centric**: Intuitive interfaces that reduce cognitive load
- **Accessible**: WCAG compliant components for all users
- **Performant**: Fast loading times and smooth interactions
- **Consistent**: Cohesive design language throughout the application
- **Scalable**: Architecture that supports future growth and features
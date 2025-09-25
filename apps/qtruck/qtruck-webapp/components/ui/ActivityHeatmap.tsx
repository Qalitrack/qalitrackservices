'use client'

import { useState, useEffect } from 'react'

interface HeatmapData {
  [date: string]: number // date in YYYY-MM-DD format -> activity count
}

interface ActivityHeatmapProps {
  data: HeatmapData
  year?: number
  className?: string
}

export default function ActivityHeatmap({ data, year, className = '' }: ActivityHeatmapProps) {
  const [currentYear, setCurrentYear] = useState(year || new Date().getFullYear())
  const [hoveredDate, setHoveredDate] = useState<string | null>(null)
  const [hoveredCount, setHoveredCount] = useState<number>(0)

  useEffect(() => {
    if (year) {
      setCurrentYear(year)
    }
  }, [year])

  // Generate all dates for the year
  const generateYearDates = (year: number) => {
    const dates = []
    const start = new Date(year, 0, 1) // January 1st
    const end = new Date(year, 11, 31) // December 31st
    
    let current = new Date(start)
    while (current <= end) {
      dates.push(new Date(current))
      current.setDate(current.getDate() + 1)
    }
    return dates
  }

  // Get activity level for color intensity
  const getActivityLevel = (count: number) => {
    if (count === 0) return 0
    if (count <= 2) return 1
    if (count <= 5) return 2
    if (count <= 10) return 3
    return 4
  }

  // Get color class based on activity level
  const getColorClass = (level: number) => {
    const colors = [
      'bg-gray-100', // No activity
      'bg-green-200', // Low activity
      'bg-green-400', // Medium activity
      'bg-green-600', // High activity
      'bg-green-800', // Very high activity
    ]
    return colors[level]
  }

  // Get weeks for the year (starting Monday)
  const getWeeksForYear = (year: number) => {
    const dates = generateYearDates(year)
    const weeks = []
    let currentWeek = []

    // Find the first Monday of the year or the first day
    let startDate = new Date(year, 0, 1)
    const firstDayOfWeek = startDate.getDay()
    
    // Adjust to start from Monday (1) instead of Sunday (0)
    const daysToFirstMonday = firstDayOfWeek === 0 ? 1 : (8 - firstDayOfWeek)
    if (daysToFirstMonday < 7) {
      startDate.setDate(startDate.getDate() + daysToFirstMonday)
    }

    // Add empty cells for days before the first Monday
    for (let i = 0; i < daysToFirstMonday && daysToFirstMonday < 7; i++) {
      currentWeek.push(null)
    }

    dates.forEach(date => {
      const dayOfWeek = date.getDay()
      const adjustedDay = dayOfWeek === 0 ? 6 : dayOfWeek - 1 // Convert Sunday(0) to 6, Monday(1) to 0, etc.

      if (adjustedDay === 0 && currentWeek.length > 0) {
        // Start new week on Monday
        weeks.push([...currentWeek])
        currentWeek = []
      }

      currentWeek.push(date)

      if (date.getTime() === dates[dates.length - 1].getTime()) {
        // Add the last week
        weeks.push([...currentWeek])
      }
    })

    return weeks
  }

  const weeks = getWeeksForYear(currentYear)
  const months = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
  ]

  const handleMouseEnter = (date: Date | null) => {
    if (date) {
      const dateStr = date.toISOString().split('T')[0]
      setHoveredDate(dateStr)
      setHoveredCount(data[dateStr] || 0)
    }
  }

  const handleMouseLeave = () => {
    setHoveredDate(null)
    setHoveredCount(0)
  }

  return (
    <div className={`activity-heatmap ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">Activity in {currentYear}</h3>
          {hoveredDate && (
            <p className="text-sm text-gray-600">
              {hoveredCount} {hoveredCount === 1 ? 'activity' : 'activities'} on{' '}
              {new Date(hoveredDate).toLocaleDateString('en-US', { 
                weekday: 'long', 
                year: 'numeric', 
                month: 'long', 
                day: 'numeric' 
              })}
            </p>
          )}
        </div>
        
        {/* Year selector */}
        <div className="flex items-center space-x-2">
          <button
            onClick={() => setCurrentYear(currentYear - 1)}
            className="p-1 hover:bg-gray-100 rounded"
            title="Previous year"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <span className="text-sm font-medium min-w-[3rem] text-center">{currentYear}</span>
          <button
            onClick={() => setCurrentYear(currentYear + 1)}
            className="p-1 hover:bg-gray-100 rounded"
            title="Next year"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
      </div>

      {/* Heatmap Grid */}
      <div className="relative">
        {/* Month labels */}
        <div className="flex text-xs text-gray-500 mb-1 ml-8">
          {months.map((month, index) => (
            <div key={month} className="flex-1 text-center">
              {index % 3 === 0 ? month : ''}
            </div>
          ))}
        </div>

        <div className="flex">
          {/* Day labels */}
          <div className="flex flex-col text-xs text-gray-500 mr-2 mt-1">
            <div className="h-3"></div> {/* Spacer for Mon */}
            <div className="h-3 flex items-center">Tue</div>
            <div className="h-3"></div> {/* Spacer for Wed */}
            <div className="h-3 flex items-center">Thu</div>
            <div className="h-3"></div> {/* Spacer for Fri */}
            <div className="h-3 flex items-center">Sat</div>
            <div className="h-3"></div> {/* Spacer for Sun */}
          </div>

          {/* Heatmap cells */}
          <div className="flex flex-col space-y-1">
            {[0, 1, 2, 3, 4, 5, 6].map(dayOfWeek => (
              <div key={dayOfWeek} className="flex space-x-1">
                {weeks.map((week, weekIndex) => {
                  const date = week[dayOfWeek]
                  if (!date) {
                    return (
                      <div
                        key={`${weekIndex}-${dayOfWeek}`}
                        className="w-3 h-3"
                      />
                    )
                  }

                  const dateStr = date.toISOString().split('T')[0]
                  const count = data[dateStr] || 0
                  const level = getActivityLevel(count)

                  return (
                    <div
                      key={`${weekIndex}-${dayOfWeek}`}
                      className={`w-3 h-3 rounded-sm cursor-pointer border border-gray-200 ${getColorClass(level)} hover:ring-2 hover:ring-blue-300 transition-all`}
                      onMouseEnter={() => handleMouseEnter(date)}
                      onMouseLeave={handleMouseLeave}
                      title={`${count} ${count === 1 ? 'activity' : 'activities'} on ${date.toLocaleDateString()}`}
                    />
                  )
                })}
              </div>
            ))}
          </div>
        </div>

        {/* Legend */}
        <div className="flex items-center justify-between mt-4 text-xs text-gray-500">
          <span>Less</span>
          <div className="flex space-x-1">
            {[0, 1, 2, 3, 4].map(level => (
              <div
                key={level}
                className={`w-3 h-3 rounded-sm border border-gray-200 ${getColorClass(level)}`}
              />
            ))}
          </div>
          <span>More</span>
        </div>
      </div>

      {/* Statistics */}
      <div className="mt-4 grid grid-cols-3 gap-4 text-center">
        <div>
          <div className="text-lg font-semibold text-gray-900">
            {Object.values(data).reduce((sum, count) => sum + count, 0)}
          </div>
          <div className="text-xs text-gray-500">Total Activities</div>
        </div>
        <div>
          <div className="text-lg font-semibold text-gray-900">
            {Object.keys(data).length}
          </div>
          <div className="text-xs text-gray-500">Active Days</div>
        </div>
        <div>
          <div className="text-lg font-semibold text-gray-900">
            {Math.max(...Object.values(data), 0)}
          </div>
          <div className="text-xs text-gray-500">Best Day</div>
        </div>
      </div>
    </div>
  )
}
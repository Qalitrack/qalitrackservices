// Utility function to extract base email from email with alias
export function getBaseEmail(email: string): string {
  if (!email) return email
  
  // Check if email contains alias (+)
  if (email.includes('@') && email.split('@')[0].includes('+')) {
    const [localPart, domain] = email.split('@')
    const basePart = localPart.split('+')[0]
    return `${basePart}@${domain}`
  }
  
  return email
}

// Utility function to get user type from email alias
export function getUserTypeFromEmail(email: string): string | null {
  if (!email || !email.includes('@')) return null
  
  const localPart = email.split('@')[0]
  if (!localPart.includes('+')) return null
  
  const aliasPart = localPart.split('+')[1]
  return aliasPart || null
}

// Format user display name with role
export function formatUserDisplay(user: { first_name?: string, last_name?: string, email: string, user_type?: string }): string {
  const name = [user.first_name, user.last_name].filter(Boolean).join(' ')
  const baseEmail = getBaseEmail(user.email)
  
  if (name) {
    return `${name} (${baseEmail})`
  }
  
  return baseEmail
}
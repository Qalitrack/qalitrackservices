// Get runtime config directly from server environment (no HTTP request needed)
const getRuntimeConfig = () => {
  const apiUrl = process.env.API_URL || '/api';
  const baseDomain = apiUrl.replace(/\/api$/, '') || '';
  
  return {
    apiUrl: baseDomain,
    siteUrl: process.env.SITE_URL || process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000',
    appName: process.env.APP_NAME || process.env.NEXT_PUBLIC_APP_NAME || 'QTruck'
  };
};

// Get the proper URL for Open Graph images (using runtime config)
export const getOGImageUrl = (imagePath = '/og-image.svg') => {
  const config = getRuntimeConfig();
  const baseUrl = config.siteUrl;
  
  // Ensure we have a properly formatted URL
  if (imagePath.startsWith('http')) {
    return imagePath; // Already a full URL
  }
  
  // Remove leading slash if present to avoid double slashes
  const cleanPath = imagePath.startsWith('/') ? imagePath.slice(1) : imagePath;
  
  // Ensure baseUrl doesn't end with slash
  const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  
  return `${cleanBaseUrl}/${cleanPath}`;
};

// Get the proper site URL (using runtime config)
export const getSiteUrl = (path = '') => {
  const config = getRuntimeConfig();
  const baseUrl = config.siteUrl;
  
  if (path.startsWith('http')) {
    return path; // Already a full URL
  }
  
  // Remove leading slash if present to avoid double slashes
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  
  // Ensure baseUrl doesn't end with slash
  const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  
  return cleanPath ? `${cleanBaseUrl}/${cleanPath}` : cleanBaseUrl;
};

// Create consistent metadata object (using runtime environment variables)
export const createMetadata = ({
  title,
  description,
  keywords,
  path = '',
  image = '/og-image.svg'
}) => {
  const fullUrl = getSiteUrl(path);
  const imageUrl = getOGImageUrl(image);
  
  return {
    title,
    description,
    keywords,
    openGraph: {
      title,
      description,
      url: fullUrl,
      siteName: 'QTruck - Fleet Management System',
      images: [
        {
          url: imageUrl,
          width: 1200,
          height: 630,
          alt: 'QTruck Fleet Management System Logo',
        }
      ],
      locale: 'en_US',
      type: 'website',
    },
    twitter: {
      card: 'summary_large_image',
      title,
      description,
      images: [imageUrl],
      creator: '@qtruck',
    },
    robots: 'index, follow',
  };
};
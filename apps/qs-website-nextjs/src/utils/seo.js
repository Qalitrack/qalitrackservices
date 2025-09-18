import config from './config';

// Get the proper URL for Open Graph images
export const getOGImageUrl = (imagePath = '/og-image.svg') => {
  // Use ASSETS_URL from config (which can be overridden by Docker volumes)
  const baseUrl = config.ASSETS_URL || config.SITE_URL;
  
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

// Get the proper site URL
export const getSiteUrl = (path = '') => {
  const baseUrl = config.SITE_URL;
  
  if (path.startsWith('http')) {
    return path; // Already a full URL
  }
  
  // Remove leading slash if present to avoid double slashes
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  
  // Ensure baseUrl doesn't end with slash
  const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  
  return cleanPath ? `${cleanBaseUrl}/${cleanPath}` : cleanBaseUrl;
};

// Create consistent metadata object
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
      siteName: 'Qalibrated Systems Limited',
      images: [
        {
          url: imageUrl,
          width: 1200,
          height: 630,
          alt: 'Qalibrated Systems Limited Logo',
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
      creator: '@qalibrated',
    },
    robots: 'index, follow',
  };
};
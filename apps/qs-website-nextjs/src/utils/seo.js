import config from './config';

// Get the proper URL for Open Graph images (runtime replaceable)
export const getOGImageUrl = (imagePath = '/og-image.svg') => {
  // Use placeholder that will be replaced at runtime via entrypoint
  const baseUrl = '__NEXT_PUBLIC_ASSETS_URL__';
  
  // Ensure we have a properly formatted URL
  if (imagePath.startsWith('http')) {
    return imagePath; // Already a full URL
  }
  
  // Remove leading slash if present to avoid double slashes
  const cleanPath = imagePath.startsWith('/') ? imagePath.slice(1) : imagePath;
  
  return `${baseUrl}/${cleanPath}`;
};

// Get the proper site URL (runtime replaceable)
export const getSiteUrl = (path = '') => {
  // Use placeholder that will be replaced at runtime via entrypoint
  const baseUrl = '__NEXT_PUBLIC_SITE_URL__';
  
  if (path.startsWith('http')) {
    return path; // Already a full URL
  }
  
  // Remove leading slash if present to avoid double slashes
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  
  return cleanPath ? `${baseUrl}/${cleanPath}` : baseUrl;
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
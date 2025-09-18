import Head from 'next/head';
import { getSiteUrl, getOGImageUrl } from '@/utils/seo';

const SEO = ({
  title = "Qalibrated Systems Limited - Professional Weighing & Automation Solutions",
  description = "Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.",
  keywords = "weighing systems, calibration services, automation solutions, weighbridges, industrial scales, building management, transport systems, Kenya, Nairobi",
  image = "/logo-social.png",
  url = "",
  type = "website",
  siteName = "Qalibrated Systems Limited"
}) => {
  const fullTitle = title.includes('Qalibrated') ? title : `${title} | Qalibrated Systems Limited`;
  const fullUrl = url.startsWith('http') ? url : getSiteUrl(url);
  const fullImageUrl = image.startsWith('http') ? image : getOGImageUrl(image);

  return (
    <Head>
      {/* Basic Meta Tags */}
      <title>{fullTitle}</title>
      <meta name="description" content={description} />
      <meta name="keywords" content={keywords} />
      <meta name="author" content="Qalibrated Systems Limited" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <meta name="robots" content="index, follow" />
      <meta name="language" content="English" />
      <meta name="revisit-after" content="7 days" />
      
      {/* Canonical URL */}
      <link rel="canonical" href={fullUrl} />
      
      {/* Open Graph Meta Tags for Facebook, LinkedIn, etc. */}
      <meta property="og:title" content={fullTitle} />
      <meta property="og:description" content={description} />
      <meta property="og:image" content={fullImageUrl} />
      <meta property="og:image:alt" content="Qalibrated Systems Limited Logo" />
      <meta property="og:image:width" content="1200" />
      <meta property="og:image:height" content="630" />
      <meta property="og:url" content={fullUrl} />
      <meta property="og:type" content={type} />
      <meta property="og:site_name" content={siteName} />
      <meta property="og:locale" content="en_US" />
      
      {/* Twitter Card Meta Tags */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={fullTitle} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={fullImageUrl} />
      <meta name="twitter:image:alt" content="Qalibrated Systems Limited Logo" />
      <meta name="twitter:site" content="@qalibrated" />
      <meta name="twitter:creator" content="@qalibrated" />
      
      {/* Additional Meta Tags */}
      <meta name="theme-color" content="#f59e0b" />
      <meta name="msapplication-TileColor" content="#f59e0b" />
      <meta name="application-name" content="Qalibrated Systems Limited" />
      
      {/* Favicon and Icons */}
      <link rel="icon" type="image/x-icon" href="/favicon.ico" />
      <link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
      <link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
      <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
      <link rel="manifest" href="/site.webmanifest" />
      
      {/* Business/Organization Schema */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            "@context": "https://schema.org",
            "@type": "Organization",
            "name": "Qalibrated Systems Limited",
            "description": description,
            "url": getSiteUrl(),
            "logo": getOGImageUrl("/logo-social.png"),
            "image": getOGImageUrl("/logo-social.png"),
            "address": {
              "@type": "PostalAddress",
              "streetAddress": "QSL centre 1st Floor",
              "addressLocality": "Nairobi",
              "addressCountry": "Kenya",
              "postalCode": "00100"
            },
            "contactPoint": {
              "@type": "ContactPoint",
              "telephone": "+254714999996",
              "contactType": "Customer Service",
              "email": "info@qalibrated.co.ke"
            },
            "sameAs": [
              "https://www.linkedin.com/company/qalibrated-systems",
              "https://facebook.com/qalibrated",
              "https://twitter.com/qalibrated"
            ]
          })
        }}
      />
    </Head>
  );
};

export default SEO;
"""
Custom schema preprocessing hooks for drf-spectacular.
This file handles automatic tagging of API endpoints based on URL patterns.
"""


def custom_preprocessing_hook(endpoints):
    """
    Automatically tag API endpoints based on their URL patterns.
    This replaces manual tagging and ensures each domain app gets its proper tag.
    """
    # URL pattern to tag mapping
    url_tag_mapping = {
        'auth/': 'Authentication',
        'users/': 'Users', 
        'drivers/': 'Drivers',
        'api/admin/': 'Administration',
        'feedback/': 'Feedback',
        'fleet/': 'Fleet',
        'trips/': 'Trips',  # Note: trips are under fleet in URLs
        'settings/': 'Settings',
    }
    
    # Process each endpoint
    for path, path_regex, method, callback in endpoints:
        # Remove leading slash and get the first path segment
        clean_path = path.lstrip('/')
        
        # Find matching tag based on URL pattern
        tag = None
        for url_pattern, tag_name in url_tag_mapping.items():
            if clean_path.startswith(url_pattern):
                tag = tag_name
                break
        
        # If no specific tag found, try to infer from path structure
        if not tag:
            # Handle fleet/trips pattern specifically
            if 'fleet' in clean_path and 'trip' in clean_path:
                tag = 'Trips'
            # Handle admin patterns
            elif 'admin' in clean_path:
                tag = 'Administration'
            # Default fallback - use first path segment
            else:
                first_segment = clean_path.split('/')[0] if '/' in clean_path else clean_path
                tag = first_segment.capitalize()
        
        # Apply the tag to the callback's schema
        if hasattr(callback, 'view_class'):
            view_class = callback.view_class
            # Set tags on the view class if not already set
            if not hasattr(view_class, '_spectacular_annotation'):
                view_class._spectacular_annotation = {}
            if 'tags' not in view_class._spectacular_annotation:
                view_class._spectacular_annotation['tags'] = [tag]
        
        # Also set on the callback function itself for function-based views
        if hasattr(callback, '__self__') and hasattr(callback.__self__, '__class__'):
            callback_class = callback.__self__.__class__
            if not hasattr(callback_class, '_spectacular_annotation'):
                callback_class._spectacular_annotation = {}
            if 'tags' not in callback_class._spectacular_annotation:
                callback_class._spectacular_annotation['tags'] = [tag]
    
    return endpoints
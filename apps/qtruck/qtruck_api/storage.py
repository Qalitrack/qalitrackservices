"""
MinIO storage backend for Django
"""
import uuid
import mimetypes
from datetime import datetime, timedelta
from urllib.parse import urljoin

from django.conf import settings
from django.core.files.storage import Storage
from django.core.files.base import ContentFile
from minio import Minio
from minio.error import S3Error


class MinIOStorage(Storage):
    """
    MinIO storage backend for Django
    """

    def __init__(self, bucket_name=None):
        self.endpoint = getattr(settings, 'MINIO_ENDPOINT', 'localhost:9000')
        self.access_key = getattr(settings, 'MINIO_ACCESS_KEY', 'minioadmin')
        self.secret_key = getattr(settings, 'MINIO_SECRET_KEY', 'minioadmin123')
        self.secure = getattr(settings, 'MINIO_SECURE', False)
        self.bucket_name = bucket_name or getattr(settings, 'MINIO_BUCKET_NAME', 'qtruck-media')
        self.public_url = getattr(settings, 'MINIO_PUBLIC_URL', None)
        self._client = None

    @property
    def client(self):
        """Lazily initialize MinIO client"""
        if self._client is None:
            self._client = Minio(
                endpoint=self.endpoint,
                access_key=self.access_key,
                secret_key=self.secret_key,
                secure=self.secure
            )
        return self._client

    def _ensure_bucket_exists(self):
        """Create bucket if it doesn't exist"""
        try:
            if not self.client.bucket_exists(self.bucket_name):
                self.client.make_bucket(self.bucket_name)
        except S3Error as e:
            raise Exception(f"Error creating bucket: {e}")

    def _generate_object_name(self, name):
        """Generate unique object name"""
        ext = name.split('.')[-1] if '.' in name else ''
        filename = f"{uuid.uuid4().hex}"
        if ext:
            filename = f"{filename}.{ext}"

        # Add date prefix for organization
        date_prefix = datetime.now().strftime('%Y/%m/%d')
        return f"{date_prefix}/{filename}"

    def _open(self, name, mode='rb'):
        """Open file from MinIO"""
        try:
            response = self.client.get_object(self.bucket_name, name)
            return ContentFile(response.read())
        except S3Error as e:
            raise Exception(f"Error opening file {name}: {e}")

    def _save(self, name, content):
        """Save file to MinIO"""
        object_name = self._generate_object_name(name)

        try:
            # Ensure bucket exists when actually saving
            self._ensure_bucket_exists()

            # Reset file pointer if it's been read
            if hasattr(content, 'seek'):
                content.seek(0)

            # Get content type
            content_type, _ = mimetypes.guess_type(name)
            if not content_type:
                content_type = 'application/octet-stream'

            # Upload file
            self.client.put_object(
                bucket_name=self.bucket_name,
                object_name=object_name,
                data=content,
                length=-1,
                part_size=10*1024*1024,  # 10MB
                content_type=content_type
            )

            return object_name
        except S3Error as e:
            raise Exception(f"Error saving file {name}: {e}")

    def delete(self, name):
        """Delete file from MinIO"""
        try:
            self.client.remove_object(self.bucket_name, name)
        except S3Error as e:
            raise Exception(f"Error deleting file {name}: {e}")

    def exists(self, name):
        """Check if file exists in MinIO"""
        try:
            self.client.stat_object(self.bucket_name, name)
            return True
        except S3Error:
            return False

    def size(self, name):
        """Get file size from MinIO"""
        try:
            stat = self.client.stat_object(self.bucket_name, name)
            return stat.size
        except S3Error:
            return 0

    def url(self, name):
        """Generate URL for file"""
        if self.public_url:
            return urljoin(self.public_url, name)

        # Generate presigned URL valid for 7 days
        try:
            url = self.client.presigned_get_url(
                bucket_name=self.bucket_name,
                object_name=name,
                expires=timedelta(days=7)
            )
            return url
        except S3Error as e:
            raise Exception(f"Error generating URL for {name}: {e}")

    def get_valid_name(self, name):
        """Return a sanitized filename"""
        return name.replace('\\', '/').replace('//', '/')

    def get_available_name(self, name, max_length=None):
        """Return an available filename"""
        return self._generate_object_name(name)


def get_minio_storage(bucket_name='qtruck-media'):
    """
    Get a MinIO storage instance for a specific bucket
    """
    class BucketedMinIOStorage(MinIOStorage):
        def __init__(self):
            super().__init__(bucket_name)
            # Don't ensure bucket exists here - do it lazily when needed

    return BucketedMinIOStorage()


# Specific storage classes for different file types
class TripImageStorage(MinIOStorage):
    def __init__(self):
        super().__init__('qtruck-trip-images')

class ExpenseReceiptStorage(MinIOStorage):
    def __init__(self):
        super().__init__('qtruck-expense-receipts')

class MaterialPhotoStorage(MinIOStorage):
    def __init__(self):
        super().__init__('qtruck-material-photos')

class DriverProfileStorage(MinIOStorage):
    def __init__(self):
        super().__init__('qtruck-driver-profiles')

class ReceiptStorage(MinIOStorage):
    def __init__(self):
        super().__init__('qtruck-receipts')
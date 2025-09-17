# QTruck API Dockerfile
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        postgresql-client \
        build-essential \
        libpq-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy project files
COPY . .

# Create non-root user for security
RUN addgroup --system --gid 1001 django \
    && adduser --system --uid 1001 --gid 1001 django

# Change ownership of the app directory
RUN chown -R django:django /app
USER django

# Expose port
EXPOSE 8000

# Set environment variables
ENV PYTHONUNBUFFERED=1
ENV DJANGO_SETTINGS_MODULE=qtruck_api.settings

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD python manage.py check --deploy || exit 1

# Start command
CMD ["python", "manage.py", "runserver", "0.0.0.0:8000"]
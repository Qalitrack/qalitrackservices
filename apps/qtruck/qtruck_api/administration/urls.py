from django.urls import path
from .views import flush_database

urlpatterns = [
    # Admin database operations
    path('flush-database/', flush_database, name='flush_database'),
]
# Generated manually to remove driver field from Expense

from django.db import migrations, models
import django.db.models.deletion


class Migration(migrations.Migration):

    dependencies = [
        ('core', '0003_remove_material_destination_alter_material_trip'),
    ]

    operations = [
        migrations.RemoveField(
            model_name='expense',
            name='driver',
        ),
        migrations.AlterField(
            model_name='expense',
            name='trip',
            field=models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, related_name='expenses', to='core.trip'),
        ),
    ]
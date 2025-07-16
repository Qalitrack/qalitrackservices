using Xunit;
using CustomerService.Core.Entities;

namespace CustomerService.Tests;

public class BasicTests
{
    [Fact]
    public void Customer_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();
        
        // Assert
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.Equal("KES", customer.Currency);
        Assert.Equal(30, customer.PaymentTermsDays);
        Assert.False(customer.IsSupplier);
        Assert.True(customer.IsBuyer);
    }
    
    [Fact]
    public void Order_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var order = new Order();
        
        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(OrderType.Purchase, order.OrderType);
        Assert.Equal("KES", order.Currency);
    }
    
    [Fact]
    public void Contact_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var contact = new Contact();
        
        // Assert
        Assert.True(contact.IsActive);
        Assert.True(contact.PreferEmail);
        Assert.False(contact.PreferPhone);
        Assert.False(contact.PreferSMS);
        Assert.Equal("en", contact.PreferredLanguage);
        Assert.True(contact.NotifyOnOrderUpdates);
        Assert.True(contact.NotifyOnContractRenewals);
    }

    [Fact]
    public void Contract_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var contract = new Contract();
        
        // Assert
        Assert.Equal(ContractStatus.Draft, contract.Status);
        Assert.Equal(30, contract.PaymentDueDays);
        Assert.False(contract.AutoRenew);
    }

    [Fact]
    public void OrderStatusHistory_Creation_ShouldSetCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var statusHistory = new OrderStatusHistory();
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        Assert.True(statusHistory.ChangedAt >= beforeCreation);
        Assert.True(statusHistory.ChangedAt <= afterCreation);
    }
}